using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    /// <summary>
    /// 房间局部X/Z各12bit、Yaw8bit的独立完整数据
    /// </summary>
    public sealed class MonsterPoseQuantizer
    {
        //12bit为4096个值 0-4095，压缩的结果
        private const int PositionMax = 4095;
        private readonly Vector2 _origin;
        private readonly Vector2 _size;

        public MonsterPoseQuantizer(Vector2 origin, Vector2 size)
        {
            if (!MonsterMath.IsFinite(origin) || !MonsterMath.IsPositiveFinite(size.x) ||
                !MonsterMath.IsPositiveFinite(size.y))
                throw new ArgumentOutOfRangeException(nameof(size));
            _origin = origin;
            _size = size;
        }

        public uint Pack(Vector2 position, float yaw)
        {
            if (!MonsterMath.IsFinite(position) || !MonsterMath.IsFinite(yaw))
                throw new ArgumentOutOfRangeException(nameof(position));
            //量化精度取决于房间大小,一个量化单位为size/4095
            int x = Mathf.RoundToInt(Mathf.Clamp01((position.x - _origin.x) / _size.x) * PositionMax);
            int z = Mathf.RoundToInt(Mathf.Clamp01((position.y - _origin.y) / _size.y) * PositionMax);
            //取yaw为0-360度
            float normalizedYaw = Mathf.Repeat(yaw, 360f);
            //量化取了256档，也就是把360份等效换成256份，逻辑为原始yaw / 360 * 256,cpu有常量折叠机制
            //写（256f / 360f）会先被算好成常量，如果直接放进变量yaw的计算会多一个除法，计算会变慢
            //& 0xFF保留最低8bit，对应256份
            int packedYaw = Mathf.RoundToInt(normalizedYaw * (256f / 360f)) & 0xFF;
            //使用左移，将X,Z,Yaw压到一个uint中，0-11为X,12-23为Z,24-31为Yaw，共32bit四个字节完成
            return (uint)(x | (z << 12) | (packedYaw << 24));
        }
        //反过来就是解包
        public void Unpack(uint packed, out Vector2 position, out float yaw)
        {
            int x = (int)(packed & 0xFFF);
            int z = (int)((packed >> 12) & 0xFFF);
            int packedYaw = (int)(packed >> 24);
            position = new Vector2(
                _origin.x + x * (_size.x / PositionMax),
                _origin.y + z * (_size.y / PositionMax));
            yaw = packedYaw * (360f / 256f);
        }
    }
    
    public readonly struct MonsterPackedState
    {
        //4字节x+z+yaw数据
        public readonly uint PackedPose;
        public readonly byte AttackSequence;
        public readonly byte HitSequence;
        public MonsterPackedState(uint packedPose, byte attackSequence, byte hitSequence)
        {
            PackedPose = packedPose;
            AttackSequence = attackSequence;
            HitSequence = hitSequence;
        }
    }

    /// <summary>
    /// 整个房间这一Tick的表现快照
    /// </summary>
    public sealed class MonsterPresentationFrame
    {
        /// <summary>
        /// 掩码01表示slot的存活信息，一个byte表示八个slot，slot映射到这里取到自己的
        /// </summary>
        private readonly byte[] _activeMask;
        private readonly MonsterPackedState[] _activeStates;
        public uint ServerTick { get; }
        public int SlotCount { get; }
        public IReadOnlyList<byte> ActiveMask => _activeMask;
        public IReadOnlyList<MonsterPackedState> ActiveStates => _activeStates;

        public MonsterPresentationFrame(uint serverTick, int slotCount,
            IReadOnlyList<byte> activeMask, IReadOnlyList<MonsterPackedState> activeStates)
        {
            //整数除法向上取整，保证掩码位够用
            //8 + 7） / 8 = 1 || 9 + 7） / 8 = 2  
            int maskLength = (slotCount + 7) / 8;
            if (activeMask.Count != maskLength) 
                throw new ArgumentException("Monster ActiveMask 长度与 SlotCount 不一致。", nameof(activeMask));
            _activeMask = new byte[maskLength];
            for (int i = 0; i < maskLength; i++) 
                _activeMask[i] = activeMask[i];
            //检测最后一个byte的无效bit
            if (maskLength > 0 && (slotCount & 7) != 0)
            {
                int validBits = slotCount & 7;
                int invalidMask = ~((1 << validBits) - 1) & 0xFF;
                if ((_activeMask[maskLength - 1] & invalidMask) != 0)
                    throw new ArgumentException("Monster ActiveMask 在 SlotCount 外设置了 bit。", nameof(activeMask));
            }
            int activeCount = 0;
            //找activeCount
            for (int slot = 0; slot < slotCount; slot++)
                if (IsSet(_activeMask, slot)) 
                    activeCount++;
            if (activeStates.Count != activeCount)
                throw new ArgumentException("Monster ActiveStates 数量与 ActiveMask 不一致。", nameof(activeStates));
            //防御性复制，保证数据不被外部改变，这整个构造都是防御性检测，因为Capture本身public，信息来源不再可信
            _activeStates = new MonsterPackedState[activeCount];
            //优化：_activeStates只存了Active状态的怪物数据，死的怪物数据不会参与网络同步
            //而客户端也通过ActiveMask一个一个对应传来的数据
            for (int i = 0; i < activeCount; i++) 
                _activeStates[i] = activeStates[i];
            ServerTick = serverTick;
            SlotCount = slotCount;
        }

        public bool IsActive(int slot)
        {
            if ((uint)slot >= (uint)SlotCount) throw new ArgumentOutOfRangeException(nameof(slot));
            return IsSet(_activeMask, slot);
        }
        /// <summary>
        /// 反向读bit，slot >> 3是找哪一个byte，等效/8，& 7剩最后三位0-7，对应这个byte的具体一位bit，等效%8
        /// 整个式子求slot对应的mask中的那个bit位是否为0
        /// </summary>
        internal static bool IsSet(byte[] mask, int slot) => (mask[slot >> 3] & (1 << (slot & 7))) != 0;
    }

    public static class MonsterPresentationPackingSystem
    {
        /// <summary>
        /// 服务器如何生成这一帧
        /// </summary>
        public static MonsterPresentationFrame Capture(MonsterWorld world, uint serverTick,
            MonsterPoseQuantizer quantizer)
        {
            if (world == null) throw new ArgumentNullException(nameof(world));
            if (quantizer == null) throw new ArgumentNullException(nameof(quantizer));
            //slot用1bit表示Active；每只存活怪物包含4字节姿态和2字节离散表现序列。
            var mask = new byte[(world.SlotCount + 7) / 8];
            var states = new MonsterPackedState[world.AliveCount];
            MonsterMetaData[] meta = world.Meta;
            MonsterMotionData[] motion = world.Motion;
            MonsterPresentationData[] presentation = world.Presentation;
            int stateIndex = 0;
            for (int slot = 0; slot < world.SlotCount; slot++)
            {
                //过滤非active，注意这里mask话没有给值，全0
                if (!meta[slot].IsActive) 
                    continue;
                //对应位取或，把Slot对应位标记为1，active且不影响其他位
                mask[slot >> 3] |= (byte)(1 << (slot & 7));
                states[stateIndex++] = new MonsterPackedState(
                    quantizer.Pack(motion[slot].Position, motion[slot].Yaw),
                    presentation[slot].AttackSequence,
                    presentation[slot].HitSequence);
            }
            if (stateIndex != states.Length)
                throw new InvalidOperationException("Monster AliveCount 与 Active Slot 数量不一致。");
            return new MonsterPresentationFrame(serverTick, world.SlotCount, mask, states);
        }
    }

    public enum MonsterFrameApplyResult : byte
    {
        Applied,
        Stale,
        BeforeRoomStart,
        MissingSpawnBatch,
        SlotCountMismatch,
    }

    public readonly struct MonsterReplicaState
    {
        public readonly int Slot;
        public readonly ushort ConfigIndex;
        public readonly bool IsActive;
        public readonly Vector2 Position;
        public readonly float Yaw;
        public readonly byte AttackSequence;
        public readonly byte HitSequence;

        public MonsterReplicaState(int slot, ushort configIndex, bool isActive,
            Vector2 position, float yaw, byte attackSequence, byte hitSequence)
        {
            Slot = slot;
            ConfigIndex = configIndex;
            IsActive = isActive;
            Position = position;
            Yaw = yaw;
            AttackSequence = attackSequence;
            HitSequence = hitSequence;
        }
    }

    /// <summary>
    /// 客户端仅持有前一帧的历史状态和当前最新状态，处理收包，应用包和输出插值包给View层
    /// </summary>
    public sealed class MonsterReplica
    {
        private ushort[] _config = Array.Empty<ushort>();
        private bool[] _active = Array.Empty<bool>();
        private Vector2[] _previousPosition = Array.Empty<Vector2>();
        private Vector2[] _currentPosition = Array.Empty<Vector2>();
        private float[] _previousYaw = Array.Empty<float>();
        private float[] _currentYaw = Array.Empty<float>();
        //攻击为离散数据不需要previous量做插值
        private byte[] _attackSequence = Array.Empty<byte>();
        private byte[] _hitSequence = Array.Empty<byte>();
        /// <summary>
        /// 这个房间最早允许接受多少Tick的包，防乱序的
        /// </summary>
        private uint _minimumAcceptedTick;
        private uint _latestTick;
        private bool _hasFrame;

        public int SlotCount { get; private set; }
        public uint LatestTick => _latestTick;
        public bool HasFrame => _hasFrame;
        /// <summary>
        /// 宣布新的房间开始，之前的网络状态作废，数组不清直接覆盖
        /// </summary>
        public void BeginRoom(uint minimumAcceptedTick)
        {
            SlotCount = 0;
            _minimumAcceptedTick = minimumAcceptedTick;
            _latestTick = 0;
            _hasFrame = false;
            Array.Clear(_active, 0, _active.Length);
        }
        /// <summary>
        /// 通过MonsterSpawnBatch与量化器解包数据，然后在Slot生成怪物
        /// </summary>
        public void ApplySpawnBatch(MonsterSpawnBatch batch, MonsterPoseQuantizer quantizer)
        {
            if (batch == null) 
                throw new ArgumentNullException(nameof(batch));
            if (quantizer == null) 
                throw new ArgumentNullException(nameof(quantizer));
            if (batch.StartSlot != SlotCount)
                throw new InvalidOperationException($"Monster SpawnBatch 必须连续追加：Expected={SlotCount}, Actual={batch.StartSlot}。");
            EnsureCapacity(checked(batch.StartSlot + batch.Count));
            for (int i = 0; i < batch.Count; i++)
            {
                int slot = batch.StartSlot + i;
                //解包数据
                MonsterSpawnData item = batch.Items[i];
                quantizer.Unpack(item.PackedPose, out Vector2 position, out float yaw);
                //接着slot沿序往下保存数据
                _config[slot] = item.ConfigIndex;
                _active[slot] = true;
                //初始previous = current
                _previousPosition[slot] = _currentPosition[slot] = position;
                _previousYaw[slot] = _currentYaw[slot] = yaw;
                _attackSequence[slot] = 0;
                _hitSequence[slot] = 0;
            }
            SlotCount += batch.Count;
        }
        /// <summary>
        /// 收到一帧网络状态
        /// </summary>
        public MonsterFrameApplyResult Apply(MonsterPresentationFrame frame, MonsterPoseQuantizer quantizer)
        {
            if (frame == null) 
                throw new ArgumentNullException(nameof(frame));
            if (quantizer == null) 
                throw new ArgumentNullException(nameof(quantizer));
            //一定比最低Tick新，不然视作此房间开始前的旧包
            if (MonsterMath.IsNewer(_minimumAcceptedTick, frame.ServerTick))
                return MonsterFrameApplyResult.BeforeRoomStart;
            //有帧了，此包还没有最新Tick新，视作旧包/乱序包
            if (_hasFrame && !MonsterMath.IsNewer(frame.ServerTick, _latestTick))
                return MonsterFrameApplyResult.Stale;
            //包SlotCount比最新的怪物数量大，没有沿序，视作少收了SpawnBatch包
            if (frame.SlotCount > SlotCount)
                return MonsterFrameApplyResult.MissingSpawnBatch;
            //包SlotCount比当前还少，视作客户端与实际情况出现不同步
            if (frame.SlotCount != SlotCount) 
                return MonsterFrameApplyResult.SlotCountMismatch;

            int stateIndex = 0;
            for (int slot = 0; slot < SlotCount; slot++)
            {
                //遍历Slot，死的不管
                bool active = frame.IsActive(slot);
                _active[slot] = active;
                if (!active) 
                    continue;
                //收到新包，解包后把当前信息丢给previous，更新current
                MonsterPackedState packed = frame.ActiveStates[stateIndex++];
                quantizer.Unpack(packed.PackedPose, out Vector2 position, out float yaw);
                _previousPosition[slot] = _currentPosition[slot];
                _previousYaw[slot] = _currentYaw[slot];
                _currentPosition[slot] = position;
                _currentYaw[slot] = yaw;
                _attackSequence[slot] = packed.AttackSequence;
                _hitSequence[slot] = packed.HitSequence;
            }
            _latestTick = frame.ServerTick;
            _hasFrame = true;
            return MonsterFrameApplyResult.Applied;
        }
        /// <summary>
        /// 输出View层用的插值后的包，因为当前逻辑简单只使用简单的平滑
        /// </summary>
        public bool TryGetState(int slot, float alpha, out MonsterReplicaState state)
        {
            state = default;
            if ((uint)slot >= (uint)SlotCount) 
                return false;
            alpha = Mathf.Clamp01(alpha);
            state = new MonsterReplicaState(slot, _config[slot], _active[slot],
                Vector2.LerpUnclamped(_previousPosition[slot], _currentPosition[slot], alpha),
                Mathf.LerpAngle(_previousYaw[slot], _currentYaw[slot], alpha),
                _attackSequence[slot], _hitSequence[slot]);
            return true;
        }

        private void EnsureCapacity(int count)
        {
            if (count <= _config.Length) return;
            int capacity = Math.Max(64, _config.Length);
            while (capacity < count) capacity = checked(capacity * 2);
            Array.Resize(ref _config, capacity);
            Array.Resize(ref _active, capacity);
            Array.Resize(ref _previousPosition, capacity);
            Array.Resize(ref _currentPosition, capacity);
            Array.Resize(ref _previousYaw, capacity);
            Array.Resize(ref _currentYaw, capacity);
            Array.Resize(ref _attackSequence, capacity);
            Array.Resize(ref _hitSequence, capacity);
        }
    }
}
