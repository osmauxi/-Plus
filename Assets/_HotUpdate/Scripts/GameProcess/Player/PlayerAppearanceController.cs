using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectGame.HotFix.Character;
using ProjectGame.HotFix.Config;
using ProjectGame.HotFix.Core.Session;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ProjectGame.HotFix.Gameplay.Player
{
    /// <summary>
    /// Gameplay 玩家角色模型加载器。
    /// PlayerRuntimeRoot 保持固定，CharacterId 只决定 VisualRoot 下加载的角色模型。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerAppearanceController : MonoBehaviour
    {
        [Serializable]
        private sealed class GameplayCharacterPresentation
        {
            [Tooltip("与 Config_Lobby_Skins 的键一致。")]
            [SerializeField] private int _characterId;
            [Tooltip("该角色在 Gameplay 中使用的 Addressables 预制件地址；留空时沿用大厅模型地址。")]
            [SerializeField] private string _modelAddress;
            [Tooltip("该 Gameplay 模型骨架对应的 Animator Controller；留空时使用默认 Controller。")]
            [SerializeField] private RuntimeAnimatorController _animatorController;

            public int CharacterId => _characterId;
            public string ModelAddress => _modelAddress;
            public RuntimeAnimatorController AnimatorController => _animatorController;
        }

        [Tooltip("动态角色模型的固定父节点；PlayerRuntimeRoot 的网络 Transform 不随外观替换。")]
        [SerializeField] private Transform _visualRoot;

        [Header("Gameplay Animation")]
        [Tooltip("没有角色专用配置时使用的默认 Controller；留空则保留角色预制件自身的 Controller。")]
        [SerializeField] private RuntimeAnimatorController _gameplayAnimatorController;

        [Tooltip("按 CharacterId 配置 Gameplay 专用模型与骨架 Controller。大厅仍只读取 Config_Lobby_Skins，不受此表影响。")]
        [SerializeField] private GameplayCharacterPresentation[] _gameplayCharacters = Array.Empty<GameplayCharacterPresentation>();

        // Addressables 实例句柄；Clear/OnDestroy 必须通过它释放实例，不能直接 Destroy。
        private AsyncOperationHandle<GameObject> _characterHandle;
        // 当前异步加载的生命周期令牌；用于外部取消与对象销毁时停止加载。
        private CancellationTokenSource _loadCts;

        /// <summary>角色实例、ModelView 和动画桥均已完成校验并可安全读取。</summary>
        public bool IsLoaded { get; private set; }
        /// <summary>固定存在的模型表现根；动态角色、武器挂点和 Camera Render Target 共用此节点。</summary>
        public Transform VisualRoot => _visualRoot;
        /// <summary>本局已绑定的角色配置 ID；-1 表示尚未加载。</summary>
        public int CharacterId { get; private set; } = -1;

        /// <summary>Addressables 创建的动态模型根对象。</summary>
        public GameObject CharacterInstance { get; private set; }
        /// <summary>向表现驱动暴露 Animator、骨骼和动画桥的模型视图。</summary>
        public PlayerModelView ModelView { get; private set; }
        /// <summary>武器挂点、装备姿态与 IK 的旧桥接入口；Gameplay 状态由 PlayerAnimationDriver 驱动。</summary>
        public CharacterAnimationBridge AnimationBridge { get; private set; }

        /// <summary>
        /// 加载本局固定角色。Gameplay 中 CharacterId 不允许动态修改。
        /// </summary>
        public async UniTask LoadAsync(PlayerSessionData sessionData, CancellationToken cancellationToken)
        {
            if (IsLoaded)
            {
                if (CharacterId == sessionData.CharacterId)
                    return;

                throw new InvalidOperationException($"Gameplay 中不允许修改玩家角色：Current={CharacterId}，Requested={sessionData.CharacterId}");
            }

            if (_loadCts != null)
                throw new InvalidOperationException("角色模型正在加载中，不能重复调用 LoadAsync。");

            if (_visualRoot == null)
                throw new InvalidOperationException($"{nameof(PlayerAppearanceController)} 没有配置 VisualRoot。");

            Config_Lobby_Skins config = GetCharacterConfig(sessionData.CharacterId);
            GameplayCharacterPresentation presentation = GetGameplayPresentation(sessionData.CharacterId);
            string characterAddress = presentation != null &&
                                      !string.IsNullOrWhiteSpace(presentation.ModelAddress)
                ? presentation.ModelAddress
                : config.ModleName;
            RuntimeAnimatorController animatorController = presentation != null &&
                                                          presentation.AnimatorController != null
                ? presentation.AnimatorController
                : _gameplayAnimatorController;

            _loadCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            CancellationToken loadToken = _loadCts.Token;

            AsyncOperationHandle<GameObject> handle = default;

            try
            {
                handle = Addressables.InstantiateAsync(characterAddress, _visualRoot);
                await handle.ToUniTask();

                if (loadToken.IsCancellationRequested)
                {
                    ReleaseLoadedHandle(handle);
                    loadToken.ThrowIfCancellationRequested();
                }

                if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
                {
                    ReleaseLoadedHandle(handle);
                    throw new InvalidOperationException($"角色 CharacterId={sessionData.CharacterId} 加载失败：{characterAddress}");
                }

                GameObject characterInstance = handle.Result;
                characterInstance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

                if (!characterInstance.TryGetComponent(out PlayerModelView modelView))
                {
                    ReleaseLoadedHandle(handle);
                    throw new InvalidOperationException($"角色 CharacterId={sessionData.CharacterId} 缺少 {nameof(PlayerModelView)}。");
                }

                if (modelView.AnimationBridge == null)
                {
                    ReleaseLoadedHandle(handle);
                    throw new InvalidOperationException($"角色 CharacterId={sessionData.CharacterId} 的 {nameof(PlayerModelView)} 没有配置 {nameof(CharacterAnimationBridge)}。");
                }

                if (modelView.Animator == null)
                {
                    ReleaseLoadedHandle(handle);
                    throw new InvalidOperationException($"角色 CharacterId={sessionData.CharacterId} 的 {nameof(PlayerModelView)} 没有配置 Animator。");
                }

                // 在公开 ModelView/IsLoaded 前先安装 Gameplay Controller。
                // PlayerAnimationDriver 在 LateUpdate 观察 ModelView，因而不会绑定到仍使用旧 Controller 的中间状态。
                if (animatorController != null)
                    modelView.Animator.runtimeAnimatorController = animatorController;

                _characterHandle = handle;
                CharacterInstance = characterInstance;
                ModelView = modelView;
                AnimationBridge = modelView.AnimationBridge;
                CharacterId = sessionData.CharacterId;
                IsLoaded = true;

                Debug.Log($"[{nameof(PlayerAppearanceController)}] 角色加载完成：CharacterId={CharacterId}，Address={characterAddress}");
            }
            finally
            {
                _loadCts?.Dispose();
                _loadCts = null;
            }
        }

        /// <summary>
        /// 取消未完成加载、释放已实例化 Addressable，并清空全部对外模型引用。
        /// PlayerAnimationDriver 下一帧会检测到 ModelView 为空并停止写 Animator。
        /// </summary>
        public void Clear()
        {
            CancelLoad();

            if (_characterHandle.IsValid())
                Addressables.ReleaseInstance(_characterHandle);

            _characterHandle = default;

            CharacterInstance = null;
            ModelView = null;
            AnimationBridge = null;
            CharacterId = -1;
            IsLoaded = false;
        }

        /// <summary>从已加载配置表解析角色资源键；找不到时立即失败，避免异步流程加载错误默认模型。</summary>
        private static Config_Lobby_Skins GetCharacterConfig(int characterId)
        {
            Dictionary<int, Config_Lobby_Skins> table = ConfigManager.Instance.GetTable<Config_Lobby_Skins>();

            if (table != null && table.TryGetValue(characterId, out Config_Lobby_Skins config))
                return config;

            throw new KeyNotFoundException($"不存在角色配置：CharacterId={characterId}");
        }

        private GameplayCharacterPresentation GetGameplayPresentation(int characterId)
        {
            if (_gameplayCharacters == null)
                return null;

            for (int i = 0; i < _gameplayCharacters.Length; i++)
            {
                GameplayCharacterPresentation presentation = _gameplayCharacters[i];
                if (presentation != null && presentation.CharacterId == characterId)
                    return presentation;
            }

            return null;
        }

        /// <summary>请求取消当前加载；实际资源释放仍由 LoadAsync 的取消分支或 Clear 负责。</summary>
        private void CancelLoad()
        {
            _loadCts?.Cancel();
        }

        /// <summary>
        /// 根据 Addressables 句柄状态选择 ReleaseInstance 或 Release。
        /// 成功实例化的对象必须 ReleaseInstance；失败/未实例化句柄只释放操作本身。
        /// </summary>
        private static void ReleaseLoadedHandle(AsyncOperationHandle<GameObject> handle)
        {
            if (!handle.IsValid())
                return;

            if (handle.Status == AsyncOperationStatus.Succeeded)
                Addressables.ReleaseInstance(handle);
            else
                Addressables.Release(handle);
        }

        /// <summary>固定玩家根销毁时统一走 Clear，确保异步任务和 Addressables 实例都被收回。</summary>
        private void OnDestroy()
        {
            Clear();
        }
    }
}
