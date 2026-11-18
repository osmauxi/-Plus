using Unity.Profiling;
using UnityEngine;

namespace ProjectGame.Core.Monsters
{
    public readonly struct MonsterQuantizationData
    {
        public readonly Vector2 Origin;
        public readonly Vector2 Size;

        public MonsterQuantizationData(Vector2 origin, Vector2 size)
        {
            Origin = origin;
            Size = size;
        }
    }

    public readonly struct MonsterPackedState
    {
        public readonly uint PackedPose;
        public readonly byte PackedEvents;

        public byte AttackSequence => (byte)(PackedEvents >> 4);
        public byte HitSequence => (byte)(PackedEvents & 0x0F);

        public MonsterPackedState(uint packedPose, byte attackSequence, byte hitSequence)
        {
            PackedPose = packedPose;
            PackedEvents = (byte)(((attackSequence & 0x0F) << 4) | (hitSequence & 0x0F));
        }

        public MonsterPackedState(uint packedPose, byte packedEvents)
        {
            PackedPose = packedPose;
            PackedEvents = packedEvents;
        }
    }

    /// <summary>
    /// 每个复制 Tick 只执行一次姿态量化，结果按 Slot 缓存供本地帧和群组快照复用。
    /// </summary>
    public static class MonsterPresentationPackKernel
    {
        private const int PositionMax = 4095;
        private static readonly ProfilerMarker PackMarker =
            new("Monster.Replication.PackActive");

        public static void PackActive(
            int[] activeSlots,
            int activeCount,
            MonsterMotionData[] motion,
            MonsterPresentationData[] presentation,
            MonsterQuantizationData quantization,
            MonsterPackedState[] packedStateBySlot)
        {
            using var packScope = PackMarker.Auto();
            for (int i = 0; i < activeCount; i++)
            {
                int slot = activeSlots[i];
                MonsterMotionData pose = motion[slot];
                MonsterPresentationData visual = presentation[slot];
                packedStateBySlot[slot] = new MonsterPackedState(
                    PackPose(pose.Position, pose.Yaw, quantization),
                    visual.AttackSequence,
                    visual.HitSequence);
            }
        }

        public static uint PackPose(
            Vector2 position,
            float yaw,
            MonsterQuantizationData quantization)
        {
            int x = Mathf.RoundToInt(Mathf.Clamp01(
                (position.x - quantization.Origin.x) / quantization.Size.x) * PositionMax);
            int z = Mathf.RoundToInt(Mathf.Clamp01(
                (position.y - quantization.Origin.y) / quantization.Size.y) * PositionMax);
            float normalizedYaw = Mathf.Repeat(yaw, 360f);
            int packedYaw = Mathf.RoundToInt(normalizedYaw * (256f / 360f)) & 0xFF;
            return (uint)(x | (z << 12) | (packedYaw << 24));
        }

        public static void UnpackPose(
            uint packed,
            MonsterQuantizationData quantization,
            out Vector2 position,
            out float yaw)
        {
            int x = (int)(packed & 0xFFF);
            int z = (int)((packed >> 12) & 0xFFF);
            int packedYaw = (int)(packed >> 24);
            position = new Vector2(
                quantization.Origin.x + x * (quantization.Size.x / PositionMax),
                quantization.Origin.y + z * (quantization.Size.y / PositionMax));
            yaw = packedYaw * (360f / 256f);
        }
    }
}
