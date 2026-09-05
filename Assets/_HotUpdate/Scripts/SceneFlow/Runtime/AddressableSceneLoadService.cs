using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace ProjectGame.HotFix.SceneFlow
{
    /// <summary>
    /// 仅管理本机Addressables Scene的Load/Unload与Handle生命周期
    /// </summary>
    public sealed class AddressableSceneLoadService
    {
        public static AddressableSceneLoadService Shared { get; } = new AddressableSceneLoadService();
        /// <summary>
        /// 存场景加载时的Addressable Handle句柄，保证正确结束资源生命周期
        /// </summary>
        private readonly Dictionary<string, AsyncOperationHandle<SceneInstance>> _loadedScenes =
                new Dictionary<string, AsyncOperationHandle<SceneInstance>>(StringComparer.Ordinal);
        private readonly Dictionary<string, UniTaskCompletionSource> _unloadingScenes =
            new Dictionary<string, UniTaskCompletionSource>(StringComparer.Ordinal);

        private AddressableSceneLoadService()
        {
        }

        public bool IsLoaded(string sceneAddress)
        {
            return _loadedScenes.TryGetValue(
                       sceneAddress,
                       out AsyncOperationHandle<SceneInstance> handle) &&
                       IsLoadedHandle(handle);
        }

        public async UniTask<Scene> LoadSceneAsync(string sceneAddress,LoadSceneMode loadMode,CancellationToken cancellationToken)
        {
            ValidateAddress(sceneAddress);
            cancellationToken.ThrowIfCancellationRequested();
            if (_unloadingScenes.TryGetValue(sceneAddress, out UniTaskCompletionSource pendingUnload))
                await pendingUnload.Task.AttachExternalCancellation(cancellationToken);

            if (_loadedScenes.TryGetValue(
                    sceneAddress,
                    out AsyncOperationHandle<SceneInstance> existing))
            {
                if (existing.IsValid() && !existing.IsDone)
                    await WaitForCompletionAsync(existing, cancellationToken);
                if (IsLoadedHandle(existing))
                    return existing.Result.Scene;

                if (existing.IsValid())
                    Addressables.Release(existing);

                _loadedScenes.Remove(sceneAddress);
            }
            //如果上面取handle没取出来，说明存储的Handle已经不正常，直接删掉重新加载尝试返回
            AsyncOperationHandle<SceneInstance> handle =
                Addressables.LoadSceneAsync(
                    sceneAddress,
                    loadMode,
                    activateOnLoad: true,
                    priority: 100);
            // Track immediately. A timeout must not make an in-flight load invisible to rollback.
            _loadedScenes.Add(sceneAddress, handle);

            try
            {
                //每帧等加载
                await WaitForCompletionAsync(handle, cancellationToken);

                if (handle.Status != AsyncOperationStatus.Succeeded ||
                    !handle.Result.Scene.IsValid())
                {
                    throw handle.OperationException ?? new InvalidOperationException(
                                                $"Addressables 场景加载失败：{sceneAddress}");
                }

                cancellationToken.ThrowIfCancellationRequested();
                Debug.Log(
                    $"[AddressableSceneLoadService] 本机场景加载完成：" +
                    $"{sceneAddress}，Mode={loadMode}");
                return handle.Result.Scene;
            }
            catch (OperationCanceledException)
            {
                // Addressables cannot cancel the physical load. Keep ownership so Unload can
                // wait for it and release the scene, even after the caller's timeout.
                throw;
            }
            catch
            {
                _loadedScenes.Remove(sceneAddress);
                if (handle.IsValid())
                    Addressables.Release(handle);
                throw;
            }
        }

        public async UniTask UnloadSceneAsync(string sceneAddress,CancellationToken cancellationToken)
        {
            ValidateAddress(sceneAddress);
            cancellationToken.ThrowIfCancellationRequested();
            if (_unloadingScenes.TryGetValue(sceneAddress, out UniTaskCompletionSource pendingUnload))
            {
                await pendingUnload.Task.AttachExternalCancellation(cancellationToken);
                return;
            }

            if (!_loadedScenes.TryGetValue(
                    sceneAddress,
                    out AsyncOperationHandle<SceneInstance> loadHandle))
            {
                return;
            }

            if (loadHandle.IsValid() && !loadHandle.IsDone)
                await WaitForCompletionAsync(loadHandle, cancellationToken);

            // Another Unload caller may have resumed from the same pending Load first.
            if (_unloadingScenes.TryGetValue(sceneAddress, out pendingUnload))
            {
                await pendingUnload.Task.AttachExternalCancellation(cancellationToken);
                return;
            }
            if (!_loadedScenes.TryGetValue(sceneAddress, out var currentHandle) || !currentHandle.Equals(loadHandle))
                return;

            if (!IsLoadedHandle(loadHandle))
            {
                if (loadHandle.IsValid())
                    Addressables.Release(loadHandle);

                _loadedScenes.Remove(sceneAddress);
                return;
            }

            var completion = new UniTaskCompletionSource();
            _unloadingScenes.Add(sceneAddress, completion);
            CompleteUnloadAsync(sceneAddress, loadHandle, completion).Forget();
            await completion.Task.AttachExternalCancellation(cancellationToken);
        }

        private async UniTaskVoid CompleteUnloadAsync(string sceneAddress,
            AsyncOperationHandle<SceneInstance> loadHandle, UniTaskCompletionSource completion)
        {
            AsyncOperationHandle<SceneInstance> unloadHandle = default;
            Exception failure = null;
            try
            {
                unloadHandle =
                Addressables.UnloadSceneAsync(
                    loadHandle,
                    autoReleaseHandle: false);
                // Once issued, physical Unload owns its handles through completion, even if
                // the caller cancels. Retries await the same operation instead of unloading twice.
                await WaitForCompletionAsync(unloadHandle, CancellationToken.None);

                if (unloadHandle.Status != AsyncOperationStatus.Succeeded)
                {
                    throw unloadHandle.OperationException ??
                          new InvalidOperationException(
                              $"Addressables 场景卸载失败：{sceneAddress}");
                }

                _loadedScenes.Remove(sceneAddress);
                Debug.Log(
                    $"[AddressableSceneLoadService] 本机场景卸载完成：" +
                    sceneAddress);
            }
            catch (Exception exception) { failure = exception; }
            finally
            {
                _unloadingScenes.Remove(sceneAddress);
                if (unloadHandle.IsValid())
                    Addressables.Release(unloadHandle);
            }
            if (failure == null) completion.TrySetResult();
            else completion.TrySetException(failure);
        }

        private static bool IsLoadedHandle(AsyncOperationHandle<SceneInstance> handle)
        {
            return handle.IsValid() &&
                   handle.Status == AsyncOperationStatus.Succeeded &&
                   handle.Result.Scene.IsValid() &&
                   handle.Result.Scene.isLoaded;
        }

        private static async UniTask WaitForCompletionAsync<T>(AsyncOperationHandle<T> handle,CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            while (!handle.IsDone)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await UniTask.Yield(PlayerLoopTiming.Update,cancellationToken);
            }
            cancellationToken.ThrowIfCancellationRequested();
        }

        private static void ValidateAddress(string sceneAddress)
        {
            if (string.IsNullOrWhiteSpace(sceneAddress))
            {
                throw new ArgumentException(
                    "Addressables 场景地址不能为空",
                    nameof(sceneAddress));
            }
        }
    }
}
