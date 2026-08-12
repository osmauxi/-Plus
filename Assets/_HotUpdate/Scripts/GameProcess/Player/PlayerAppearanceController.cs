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
        [SerializeField] private Transform _visualRoot;

        [Header("Gameplay Animation")]
        [SerializeField] private RuntimeAnimatorController _gameplayAnimatorController;

        private AsyncOperationHandle<GameObject> _characterHandle;
        private CancellationTokenSource _loadCts;

        public bool IsLoaded { get; private set; }
        public int CharacterId { get; private set; } = -1;

        public GameObject CharacterInstance { get; private set; }
        public PlayerModelView ModelView { get; private set; }
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

            _loadCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            CancellationToken loadToken = _loadCts.Token;

            AsyncOperationHandle<GameObject> handle = default;

            try
            {
                handle = Addressables.InstantiateAsync(config.ModleName, _visualRoot);
                await handle.ToUniTask();

                if (loadToken.IsCancellationRequested)
                {
                    ReleaseLoadedHandle(handle);
                    loadToken.ThrowIfCancellationRequested();
                }

                if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
                {
                    ReleaseLoadedHandle(handle);
                    throw new InvalidOperationException($"角色 CharacterId={sessionData.CharacterId} 加载失败：{config.ModleName}");
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

                if (_gameplayAnimatorController != null)
                    modelView.Animator.runtimeAnimatorController = _gameplayAnimatorController;

                _characterHandle = handle;
                CharacterInstance = characterInstance;
                ModelView = modelView;
                AnimationBridge = modelView.AnimationBridge;
                CharacterId = sessionData.CharacterId;
                IsLoaded = true;

                Debug.Log($"[{nameof(PlayerAppearanceController)}] 角色加载完成：CharacterId={CharacterId}");
            }
            finally
            {
                _loadCts?.Dispose();
                _loadCts = null;
            }
        }

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

        private static Config_Lobby_Skins GetCharacterConfig(int characterId)
        {
            Dictionary<int, Config_Lobby_Skins> table = ConfigManager.Instance.GetTable<Config_Lobby_Skins>();

            if (table != null && table.TryGetValue(characterId, out Config_Lobby_Skins config))
                return config;

            throw new KeyNotFoundException($"不存在角色配置：CharacterId={characterId}");
        }

        private void CancelLoad()
        {
            _loadCts?.Cancel();
        }

        private static void ReleaseLoadedHandle(AsyncOperationHandle<GameObject> handle)
        {
            if (!handle.IsValid())
                return;

            if (handle.Status == AsyncOperationStatus.Succeeded)
                Addressables.ReleaseInstance(handle);
            else
                Addressables.Release(handle);
        }

        private void OnDestroy()
        {
            Clear();
        }
    }
}