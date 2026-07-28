using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Netcode;
using UnityEngine;

namespace ProjectGame.HotFix.SceneFlow
{
    public sealed class GameSceneFlowController : NetworkBehaviour
    {
        [SerializeField] private GameObject _playerPrefab;

        private readonly NetworkSceneLoadService __networkSceneLoader = new NetworkSceneLoadService();

        private CancellationTokenSource _flowCts;

        public async UniTask TransitionToGameSceneAsync()
        {
            if (!IsServer)
            {
                return;
            }

            CancelCurrentFlow();

            _flowCts = new CancellationTokenSource();
            CancellationToken ct = _flowCts.Token;

            try
            {
                //GameStateController
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("进入游戏流程被取消。");

            }
            catch (Exception e)
            {
                Debug.LogError($"进入游戏流程失败: {e}");

            }
        }

        private void CancelCurrentFlow()
        {
            throw new NotImplementedException();
        }
    }
}