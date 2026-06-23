using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;

namespace ProjectGame.HotFix.UI
{
    /// <summary>
    /// 全新异步加载 UI 静态门面 (基于 Addressables)
    /// </summary>
    public static class LoadingUI
    {
        private static LoadingView _view;
        //保存 Addressables 的句柄，防止内存泄漏，方便未来彻底释放
        private static AsyncOperationHandle<GameObject> _handle;
        private static bool _isInitializing = false;

        /// <summary>
        /// 异步显示加载 UI。
        /// </summary>
        public static async UniTask Show(string message = "加载中...")
        {
            if (_view != null)
            {
                _view.Show(message);
                return;
            }

            //防止在多线程/异步环境下被同时调用两次实例化
            if (_isInitializing) 
                return;
            _isInitializing = true;

            _handle = Addressables.InstantiateAsync("LoadingUI");
            await _handle.ToUniTask();

            if (_handle.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject go = _handle.Result;

                //跨场景不销毁
                Object.DontDestroyOnLoad(go);

                Debug.Log(go.gameObject.name);
                _view = go.GetComponentInChildren<LoadingView>(true);
                _view = go.GetComponent<LoadingView>();
                Debug.Log(_view);
                if (_view != null)
                {
                    _view.Show(message);
                }
                else
                {
                    Debug.LogError("[LoadingUI] 预制件根节点上没有挂载 LoadingView 脚本");
                }
            }
            else
            {
                Debug.LogError("[LoadingUI] Addressables 加载UI失败");
            }

            _isInitializing = false;
        }

        public static void Hide()
        {
            if (_view != null)
            {
                _view.Hide();
            }
        }

        /// <summary>
        /// 如果未来你想彻底销毁这个 UI 释放内存，调用这里
        /// </summary>
        public static void Dispose()
        {
            if (_handle.IsValid())
            {
                Addressables.ReleaseInstance(_handle);
                _view = null;
            }
        }
    }
}