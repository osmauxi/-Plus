using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectGame.HotFix.SceneFlow
{
    //将NGO的场景加载逻辑封装成一个UniTask服务类，提供给GameSceneFlowController使用。
    public sealed class NetworkSceneLoadService
    {
        public async UniTask LoadSceneAsync(string sceneName,CancellationToken ct,LoadSceneMode loadMode = LoadSceneMode.Single)
        {
            EnsureServer();

            NetworkSceneManager sceneManager = NetworkManager.Singleton.SceneManager;
            //原生场景加载方法是旧式的非阻塞事件驱动方法，没办法返回Task并await
            //UniTaskCompletionSource暴露出一个Task接口，允许我们在事件回调中手动触发完成信号，从而实现异步等待。
            //它对内保留一套触发按钮：包括.TrySetResult()（宣告成功）、.TrySetCanceled()（宣告取消）和.TrySetException()（宣告失败）
            //同CTS，他是一次性的，触发一次后就失效了，后续再触发会返回false。
            UniTaskCompletionSource completion = new UniTaskCompletionSource();

            //对比Lambda表达式sceneManager.OnSceneEvent += (sceneEvent) => { /* 处理逻辑 */ };
            //局部函数首先可以直接访问方法内局部变量，其次因为有确切方法名相比Lambda可以直接-=取消绑定
            void OnSceneEvent(SceneEvent sceneEvent)
            {
                //OnSceneEvent是全局的事件回调，变任意场景就会触发，所以先判断是否是我们需要的场景
                if(sceneEvent.SceneName != sceneName)
                {
                    return;
                }
                //LoadEventCompleted指所有客户端均加载完成且彼此确认。
                if(sceneEvent.SceneEventType == SceneEventType.LoadEventCompleted) 
                {
                    //标记场景加载完成，触发completion的Task完成信号，从而让LoadSceneAsync方法的await语句继续执行。
                    completion.TrySetResult();
                }
            }

            sceneManager.OnSceneEvent += OnSceneEvent;

            try 
            {
                var status = sceneManager.LoadScene(sceneName, loadMode);

                if(status != SceneEventProgressStatus.Started) 
                {
                    throw new InvalidOperationException($"网络场景加载启动失败: {sceneName}, Status: {status}");
                }

                //将ct注册进UniTaskCompletionSource，当外部传入的CancellationToken触发了取消信号时，就触发completion的取消宣告。
                //因为Register返回的是一个IDisposable对象，所以必须用using包裹，确保在方法结束时解除注册。
                using (ct.Register(() => completion.TrySetCanceled(ct)))
                {
                    //成功注册进了OnSceneEvent，失败注册进了CancellationToken，这里开始等待LoadScene执行，根
                    //据结果触发completion的不同状态，LoadSceneAsync方法的await语句会继续执行。   
                    await completion.Task;                   
                }
            }
            finally 
            {
                //不论如何必须解除绑定。
                sceneManager.OnSceneEvent -= OnSceneEvent;
            }
        }

        public async UniTask UnloadSceneAsync(string sceneName,CancellationToken ct) 
        {
            EnsureServer();

            Scene unityScene = SceneManager.GetSceneByName(sceneName);

            if (!unityScene.IsValid() || !unityScene.isLoaded)
            {
                Debug.LogWarning($"尝试卸载不存在或未加载的场景: {sceneName}");
                return;
            }

            NetworkSceneManager sceneManager = NetworkManager.Singleton.SceneManager;
            UniTaskCompletionSource completion = new UniTaskCompletionSource();

            void OnSceneEvent(SceneEvent sceneEvent)
            {
                if (sceneEvent.SceneName != sceneName)
                {
                    return;
                }

                if (sceneEvent.SceneEventType == SceneEventType.UnloadEventCompleted)
                {
                    completion.TrySetResult();
                }
            }

            sceneManager.OnSceneEvent += OnSceneEvent;

            try
            {
                var status = sceneManager.UnloadScene(unityScene);

                if (status != SceneEventProgressStatus.Started)
                {
                    throw new InvalidOperationException(
                        $"网络场景卸载启动失败: {sceneName}, Status: {status}");
                }

                using (ct.Register(() => completion.TrySetCanceled(ct)))
                {
                    await completion.Task;
                }
            }
            finally
            {
                sceneManager.OnSceneEvent -= OnSceneEvent;
            }
        }

        private void EnsureServer()
        {
            if (NetworkManager.Singleton == null)
            {
                throw new InvalidOperationException("NetworkManager.Singleton 为空。");
            }

            if (!NetworkManager.Singleton.IsServer)
            {
                throw new InvalidOperationException("只有 Server/Host 可以发起 NGO 场景切换。");
            }

            if (NetworkManager.Singleton.SceneManager == null)
            {
                throw new InvalidOperationException("NetworkSceneManager 为空，可能 NetworkManager 尚未启动。");
            }
        }
    }
}