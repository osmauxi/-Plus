using System;
using ProjectGame.HotFix.Gameplay.Player.Sync;
using ProjectGame.HotFix.Gameplay.Weapon;
using ProjectGame.HotFix.Gameplay.Weapon.Effects;
using ProjectGame.HotFix.UI.Gameplay;
using Unity.Netcode;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Player
{
    /// <summary>
    /// 薄适配层：绑定ID/枪口，读取玩家快照，提交权威射击；不实现任何武器规则。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerSyncController))]
    [RequireComponent(typeof(ProjectileHitTargetAdapter))]
    public sealed class PlayerWeaponController : NetworkBehaviour, IWeaponStateSource,
        IPlayerEffectStateSource, IEffectRollOfferSource
    {
        /// <summary>
        /// 同步Effect获取顺序，在所有Client都能复现状态快照
        /// </summary>
        private readonly NetworkList<ushort> _effectAcquisitionOrder = new();
        private PlayerSyncController _sync;
        private WeaponRuntimeService _service;
        private Transform _muzzle;
        private PlayerEffectLoadout _effects;
        private EffectRollOfferAuthority _effectRollAuthority;
        private EffectSet _currentEffectSet = EffectSet.Empty;
        private EffectRollOffer _currentEffectRollOffer;
        private EffectOwnerStatSnapshot _ownerEffectStats;
        /// <summary>
        /// 代表谁有资格把这份Owner属性计算结果真正应用到Gameplay。
        /// </summary>
        private IEffectOwnerStatSink _ownerStatSink;
        private float _baseShieldCapacity;
        private bool _networkListSubscribed;
        private uint _committedSequence;
        private uint _effectRollBroadcastSequence;
        private WeaponRuntimeState _lastPublished;
        public WeaponDefinition Definition { get; private set; }
        public WeaponRuntimeState CurrentWeaponState => Definition != null ? _sync.ActionState.Weapon : default;
        public System.Collections.Generic.IReadOnlyList<ushort> EffectAcquisitionOrder =>
            _effects?.AcquisitionOrder ?? Array.Empty<ushort>();
        public EffectSet CurrentEffectSet => _currentEffectSet;
        public EffectOwnerStatSnapshot CurrentOwnerEffectStats => _ownerEffectStats;
        public EffectRollOffer CurrentEffectRollOffer => _currentEffectRollOffer;
        public event Action<WeaponRuntimeState> WeaponStateChanged;
        /// <summary>
        /// 给外部观察Effect改变的委托，如UI等
        /// </summary>
        public event Action<EffectSet, EffectOwnerStatSnapshot> EffectsChanged;
        public event Action<EffectRollOffer> EffectRollOffered;
        public event Action<uint, ushort, EffectRollSelectionResult> EffectRollResolved;

        public void SetEffectOwnerStatSink(IEffectOwnerStatSink sink) => _ownerStatSink = sink;

        public void Bind(int weaponId, Transform muzzle, WeaponRuntimeService service,
            float baseShieldCapacity = 0f)
        {
            Unbind();
            _sync = GetComponent<PlayerSyncController>();
            if (service == null || !service.IsInitialized || muzzle == null)
                throw new InvalidOperationException("Weapon 服务或 WeaponView.Muzzle 缺失，不能完成玩家 Ready。");
            _service = service;
            _muzzle = muzzle;
            _baseShieldCapacity = baseShieldCapacity;
            Definition = service.Catalog.Get(weaponId);
            _effects = service.CreatePlayerEffectLoadout();
            _effectRollAuthority = new EffectRollOfferAuthority(service.EffectRolls, _effects);
            _sync.ConfigureWeapon(Definition);
            ApplyReplicatedEffects(0);
            GetComponent<ProjectileHitTargetAdapter>().BindIdentity(_sync.NetworkObjectId);
            _committedSequence = _sync.ActionState.Weapon.ShotSequence;
            _sync.ServerTickCompleted += CommitAuthoritativeShot;
            PublishState();
        }
        /// <summary>
        /// 获取Effect时调用，这里只会改变网络列表_effectAcquisitionOrder
        /// 所有本地数据通过广播OnListChanged进行Rebuild，分层明确
        /// </summary>
        public EffectAcquireResult TryGrantEffect(ushort effectId)
        {
            if (_sync == null || !_sync.IsServer || !IsSpawned || _effects == null)
                return EffectAcquireResult.UnknownEffect;
            EffectAcquireResult result = _effects.CanAcquire(effectId);
            if (result == EffectAcquireResult.Success)
            {
                CancelEffectRollOnServer();
                _effectAcquisitionOrder.Add(effectId);
                TryDeliverNextQueuedEffectRollOnServer();
            }
            return result;
        }

        /// <summary>
        /// 由房间奖励/宝箱等服务端玩法调用；新候选会覆盖尚未选择的旧候选
        /// </summary>
        public EffectRollOffer CreateEffectRollOfferOnServer(WeaponEffectRollPool pool, int amount,
            int seed, bool chaos = false, bool fallbackToChaos = true)
        {
            if (_sync == null || !_sync.IsServer || !IsSpawned || _effectRollAuthority == null)
                throw new InvalidOperationException("只有已 Spawn 且完成武器绑定的服务端玩家可以生成 Effect Roll。");

            EffectRollOffer offer = _effectRollAuthority.Create(
                pool, amount, seed, chaos, fallbackToChaos);
            DeliverEffectRollOffer(offer);
            return offer;
        }

        private void DeliverEffectRollOffer(EffectRollOffer offer)
        {
            _currentEffectRollOffer = offer;
            if (NetworkManager.IsClient && OwnerClientId == NetworkManager.LocalClientId)
                PublishEffectRollOffer(offer);
            else
                ReceiveEffectRollOfferClientRpc(offer.Id, (byte)offer.Pool, offer.IsChaos,
                    offer.CopyEffectIds(), offer.CopyCurrentLevels(), CreateOwnerClientRpcParams());
        }

        /// <summary>
        /// MVP 调试/玩法入口：只允许 Server/Host 发起，并为当前所有已就绪玩家各生成一份权威三选一。
        /// 每份 Offer 仍通过原有 owner-targeted ClientRpc 发送，因此客户端只看到自己的候选。
        /// </summary>
        public bool RequestEffectRollBroadcast(WeaponEffectRollPool pool)
        {
            if (_sync == null || !_sync.IsServer || !IsSpawned ||
                !Enum.IsDefined(typeof(WeaponEffectRollPool), pool))
                return false;

            PlayerManager manager = PlayerManager.Instance;
            if (manager == null || !manager.IsInitialized) return false;

            int accepted = 0;
            System.Collections.Generic.IReadOnlyList<PlayerRuntime> players = manager.RuntimePlayers;
            for (int i = 0; i < players.Count; i++)
            {
                PlayerRuntime player = players[i];
                if (player == null ||
                    !player.TryGetComponent(out PlayerWeaponController target) ||
                    target.Definition == null || target._effectRollAuthority == null)
                    continue;

                int seed = target.CreateBroadcastSeed(pool, target.OwnerClientId);
                EffectRollOffer offer = target._effectRollAuthority.Request(
                    pool, 3, seed, chaos: false, fallbackToChaos: true);
                if (offer != null)
                    target.DeliverEffectRollOffer(offer);
                accepted++;
            }
            return accepted > 0;
        }

        private int CreateBroadcastSeed(WeaponEffectRollPool pool, ulong clientId)
        {
            uint sequence = unchecked(++_effectRollBroadcastSequence);
            if (sequence == 0) sequence = unchecked(++_effectRollBroadcastSequence);
            return unchecked(Environment.TickCount ^ (int)(sequence * 397u) ^
                             (int)clientId ^ (int)(clientId >> 32) ^ ((int)pool << 24));
        }

        /// <summary>
        /// UI 只能提交当前服务端下发的 OfferID + EffectID；返回值只表示请求已发送
        /// </summary>
        public bool RequestEffectSelection(uint offerId, ushort effectId)
        {
            if (!IsSpawned || !IsClient || !IsOwner || _currentEffectRollOffer == null ||
                _currentEffectRollOffer.Id != offerId || !_currentEffectRollOffer.Contains(effectId))
                return false;

            if (IsServer) ResolveEffectSelectionOnServer(OwnerClientId, offerId, effectId);
            else SelectEffectServerRpc(offerId, effectId);
            return true;
        }

        [ServerRpc]
        private void SelectEffectServerRpc(uint offerId, ushort effectId, ServerRpcParams rpcParams = default)
            => ResolveEffectSelectionOnServer(rpcParams.Receive.SenderClientId, offerId, effectId);

        private void ResolveEffectSelectionOnServer(ulong senderClientId, uint offerId, ushort effectId)
        {
            if (!IsServer || senderClientId != OwnerClientId || _effectRollAuthority == null) return;
            EffectRollSelectionResult result = _effectRollAuthority.TryConsume(offerId, effectId);
            if (result == EffectRollSelectionResult.Success)
                _effectAcquisitionOrder.Add(effectId);
            DeliverEffectRollResult(offerId, effectId, result);
            if (_effectRollAuthority.ActiveOffer == null)
                TryDeliverNextQueuedEffectRollOnServer();
        }

        [ClientRpc]
        private void ReceiveEffectRollOfferClientRpc(uint offerId, byte pool, bool chaos,
            ushort[] effectIds, byte[] currentLevels, ClientRpcParams rpcParams = default)
        {
            if (!IsOwner || _service == null) return;
            var rollPool = (WeaponEffectRollPool)pool;
            var offer = new EffectRollOffer(offerId, rollPool, chaos,
                _service.EffectRolls.ResolveOptions(effectIds, currentLevels));
            PublishEffectRollOffer(offer);
        }

        [ClientRpc]
        private void ReceiveEffectRollResultClientRpc(uint offerId, ushort effectId, byte result,
            ClientRpcParams rpcParams = default)
        {
            if (!IsOwner) return;
            PublishEffectRollResult(offerId, effectId, (EffectRollSelectionResult)result);
        }

        private void DeliverEffectRollResult(uint offerId, ushort effectId, EffectRollSelectionResult result)
        {
            if (NetworkManager.IsClient && OwnerClientId == NetworkManager.LocalClientId)
                PublishEffectRollResult(offerId, effectId, result);
            else
                ReceiveEffectRollResultClientRpc(
                    offerId, effectId, (byte)result, CreateOwnerClientRpcParams());
        }

        private void PublishEffectRollOffer(EffectRollOffer offer)
        {
            _currentEffectRollOffer = offer;
            EffectRollOffered?.Invoke(offer);
            GameplayUIRequests.Show(GameplayUIId.EffectRoll);
        }

        private void PublishEffectRollResult(uint offerId, ushort effectId, EffectRollSelectionResult result)
        {
            if (_currentEffectRollOffer != null && _currentEffectRollOffer.Id == offerId &&
                (result == EffectRollSelectionResult.Success ||
                 result == EffectRollSelectionResult.AcquireRejected ||
                 result == EffectRollSelectionResult.Cancelled))
            {
                _currentEffectRollOffer = null;
                GameplayUIRequests.Hide(GameplayUIId.EffectRoll);
            }
            EffectRollResolved?.Invoke(offerId, effectId, result);
        }

        private void CancelEffectRollOnServer()
        {
            EffectRollOffer active = _effectRollAuthority?.ActiveOffer;
            if (active == null) return;
            _effectRollAuthority.Cancel();
            DeliverEffectRollResult(active.Id, 0, EffectRollSelectionResult.Cancelled);
        }

        private void TryDeliverNextQueuedEffectRollOnServer()
        {
            if (_effectRollAuthority != null &&
                _effectRollAuthority.TryCreateNext(out EffectRollOffer next))
                DeliverEffectRollOffer(next);
        }

        private ClientRpcParams CreateOwnerClientRpcParams() => new()
        {
            Send = new ClientRpcSendParams { TargetClientIds = new[] { OwnerClientId } }
        };

        public void ClearEffectsOnServer()
        {
            if (_sync == null || !_sync.IsServer || !IsSpawned)
                throw new InvalidOperationException("只有已 Spawn 的服务端玩家可以清空 Effect。");
            _effectRollAuthority?.ClearPending();
            CancelEffectRollOnServer();
            _effectAcquisitionOrder.Clear();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            SubscribeEffectList();
            if (Definition != null) ApplyReplicatedEffects(0);
        }

        public override void OnNetworkDespawn()
        {
            UnsubscribeEffectList();
            base.OnNetworkDespawn();
        }

        private void SubscribeEffectList()
        {
            if (_networkListSubscribed) return;
            _effectAcquisitionOrder.OnListChanged += OnEffectListChanged;
            _networkListSubscribed = true;
        }

        private void UnsubscribeEffectList()
        {
            if (!_networkListSubscribed) return;
            _effectAcquisitionOrder.OnListChanged -= OnEffectListChanged;
            _networkListSubscribed = false;
        }

        private void OnEffectListChanged(NetworkListEvent<ushort> change)
        {
            //标记这次变化是由哪个Effect被获取/升级造成的
            //NetworkListEvent<ushort>.EventType.Add判定这次的修改信息是不是加一个新元素
            //change.Value是新增的具体的值
            ushort acquiredEffectId = change.Type == NetworkListEvent<ushort>.EventType.Add
                ? change.Value : (ushort)0;
            ApplyReplicatedEffects(acquiredEffectId);
        }
        //Weapon模块内部完成了对所有生成子弹的模拟和同步，但武器操作本身是跟玩家状态强相关的
        //所以在Effect更新，新数据快照必须同步到玩家内部模拟模块中，因为实际判定开枪是在这里
        //注意单Client所有WeaponController都会走一遍此方法
        private void ApplyReplicatedEffects(ushort acquiredEffectId)
        {
            if (Definition == null || _service == null || _effects == null) 
                return;
            //这里复制一份_effectAcquisitionOrder，它本身是网络变量，不能直接用
            var order = new ushort[_effectAcquisitionOrder.Count];
            for (int i = 0; i < order.Length; i++) 
                order[i] = _effectAcquisitionOrder[i];
            //Rebuild更新了玩家当前有哪些Effect，接下来是数值更新
            _effects.Rebuild(order);
            
            if (_sync.IsServer)
            {
                //在Server端，每个Client都重算，创建Snapshot，注册权威快照，会触发特殊Effect的效果
                WeaponEffectApplication application = _service.RegisterPlayerEffectState(
                    _sync.NetworkObjectId, Definition, _effects, _baseShieldCapacity, acquiredEffectId);
                ushort version = unchecked((ushort)(CurrentWeaponState.SnapshotVersion + 1));
                if (version == 0) 
                    version = 1;
                //把新的Effect快照数据等正式装到玩家身上
                _sync.ConfigureWeaponRuntime(application.Calculation.WeaponStats,
                    application.Effects.Id, version);
                //缓存最新数据，外部会有脚本访问这些数据
                _currentEffectSet = application.Effects;
                _ownerEffectStats = application.Calculation.OwnerStats;
            }
            else
            {
                //各个Client开始重算，返回新的快照包
                WeaponEffectCalculation calculation = _service.PreviewPlayerEffectState(
                    Definition, _effects, _baseShieldCapacity);
                //只更新本地计算参数
                _sync.ConfigureWeaponStats(calculation.WeaponStats);
                //这里的EffectSet ID不是权威世界ID，只用来做显示和预测
                _currentEffectSet = _effects.AcquisitionCount == 0 ? EffectSet.Empty : _effects.CreateSnapshot(1);
                _ownerEffectStats = calculation.OwnerStats;
            }

            EffectsChanged?.Invoke(_currentEffectSet, _ownerEffectStats);
            //只有服务器能Apply EffectOwnerStatSnapshot，客户端能算但是没资格改
            if (_sync.IsServer) 
                _ownerStatSink?.Apply(_sync.NetworkObjectId, _ownerEffectStats);
        }
        /// <summary>
        /// 绑定ServerTickCompleted，在完成这一Tick的权威模拟后调用
        /// 通过比较ShotSequence来判断开枪，读取方向触发CommitShot，只有服务器有资格创建子弹
        /// </summary>
        private void CommitAuthoritativeShot(PlayerSimulationState state)
        {
            if (Definition == null || !_sync.IsServer || !state.ActionState.Weapon.IsEquipped) 
                return;
            uint sequence = state.ActionState.Weapon.ShotSequence;
            if (sequence == _committedSequence) 
                return;
            _committedSequence = sequence;
            Vector3 aim = new Vector3(state.AimDirection.x, 0, state.AimDirection.y);
            if (aim.sqrMagnitude < 0.0001f) 
                aim = state.Rotation * Vector3.forward;
            _service.CommitShot(_sync.NetworkObjectId, state.ActionState.Weapon, state.Tick, _muzzle.position, aim);
        }

        private void LateUpdate()
        {
            if (Definition != null && !_lastPublished.Equals(CurrentWeaponState)) 
                PublishState();
        }
        private void PublishState()
        {
            _lastPublished = CurrentWeaponState;
            WeaponStateChanged?.Invoke(_lastPublished);
        }
        public void Unbind()
        {
            if (_sync != null && _sync.IsServer)
                _service?.RemovePlayerEffectState(_sync.NetworkObjectId);
            if (_sync != null && _sync.IsServer && IsSpawned && _effectAcquisitionOrder.Count > 0)
                _effectAcquisitionOrder.Clear();
            GetComponent<ProjectileHitTargetAdapter>()?.Unbind();
            if (_sync != null) _sync.ServerTickCompleted -= CommitAuthoritativeShot;
            bool wasBound = Definition != null;
            Definition = null; _service = null; _muzzle = null; _effects = null;
            _effectRollAuthority = null; _currentEffectRollOffer = null;
            _currentEffectSet = EffectSet.Empty; _ownerEffectStats = default;
            _ownerStatSink = null; _baseShieldCapacity = 0f; _committedSequence = 0;
            _effectRollBroadcastSequence = 0;
            if (wasBound) PublishState();
            // 回池后不能把上一名玩家的 UI 引用留给下一次 Spawn。
            if (wasBound) WeaponStateChanged = null;
            if (wasBound) EffectsChanged = null;
            if (wasBound) EffectRollOffered = null;
            if (wasBound) EffectRollResolved = null;
        }
        private void OnDestroy() => Unbind();
    }
}
