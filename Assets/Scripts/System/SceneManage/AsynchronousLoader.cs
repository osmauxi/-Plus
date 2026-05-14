using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 使用方式：AsynchronousLoader.Instance.LoadScene(场景名/索引, 加载完成回调);
public class AsynchronousLoader : NetworkBehaviour
{
    public static AsynchronousLoader Instance { get; private set; }

    [Header("加载UI配置")]
    public GameObject loadingPanel;
    public Slider progressSlider;
    public TextMeshProUGUI progressText;

    [Header("加载设置")]
    public bool autoHidePanel = true;
    private Queue<Action> _onLoadCompleted = new Queue<Action>(); // 加载完成回调
    public LoadSceneMode defaultLoadMode = LoadSceneMode.Single;

    private void Awake()
    {
        // 单例逻辑：确保全局唯一，跨场景不销毁
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        loadingPanel.SetActive(false);
        DontDestroyOnLoad(gameObject);

    }
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.OnSceneEvent += OnSceneEvent;
        }
    }
    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null)
        {
            NetworkManager.Singleton.SceneManager.OnSceneEvent -= OnSceneEvent;
        }
    }
    #region 公开加载接口
        /// <param name="sceneName">要加载的场景名</param>
        /// <param name="onCompleted">加载完成回调（可选）</param>
        /// <param name="onFailed">加载失败回调</param>
        /// <param name="loadMode">加载模式</param>
        public void LoadScene(string sceneName ,LoadSceneMode loadMode, Action onCompleted = null)
        {
            if (!IsServer) 
            {
                return;
            }

            _onLoadCompleted?.Enqueue(onCompleted);
            ShowLoadingPanelClientRpc();

            var status = NetworkManager.Singleton.SceneManager.LoadScene(sceneName, loadMode);

            if (status != SceneEventProgressStatus.Started)
            {
                Debug.LogError($"场景加载启动失败: {status}");
                HideLoadingPanelClientRpc();
            }
        }

        public void UnLoadScene(string sceneName, Action onCompleted = null) 
        {
            if (!IsServer)
            {
                return;
            }
            _onLoadCompleted.Enqueue(onCompleted);
            ShowLoadingPanelClientRpc();
            var status = NetworkManager.Singleton.SceneManager.UnloadScene(UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneName));
            if (status != SceneEventProgressStatus.Started)
            {
                Debug.LogError($"场景卸载启动失败: {status}");
                HideLoadingPanelClientRpc();
            }
        }
        #endregion
    // 监听网络场景加载的各个阶段
    private void OnSceneEvent(SceneEvent sceneEvent)
    {
        switch (sceneEvent.SceneEventType)
        {
            case SceneEventType.LoadEventCompleted:
            case SceneEventType.UnloadEventCompleted:
                // 当服务器和所有客户端都完成了场景加载
                Debug.Log($"所有客户端加载完成：{sceneEvent.SceneName}");

                if (_onLoadCompleted.Count > 0)
                {
                    _onLoadCompleted.Dequeue()?.Invoke();
                }

                // 通知所有客户端隐藏面板
                if (sceneEvent.SceneEventType == SceneEventType.LoadEventCompleted)
                {
                    HideLoadingPanelClientRpc();
                }
                break;

                // 你可以在这里扩展处理掉线、超时等情况
        }
    }
    #region ClientRpc (客户端表现逻辑)

        [ClientRpc]
        private void ShowLoadingPanelClientRpc()
        {
            loadingPanel.SetActive(true);
            // 开启一个伪造的进度条协程，让玩家感觉在动
            StartCoroutine(FakeProgressCoroutine());
        }

        [ClientRpc]
        private void HideLoadingPanelClientRpc()
        {
            // 收到服务端指令，加载彻底完成，填满进度条并关闭
            StartCoroutine(FinishLoadingCoroutine());
        }

        #endregion
    #region UI协程 (伪进度条)

        private IEnumerator FakeProgressCoroutine()
        {
            float currentProgress = 0f;
            // 快速加载到 90%，然后等待
            while (currentProgress < 0.9f)
            {
                UpdateUI(currentProgress);
                // 模拟不规则的加载速度
                currentProgress += UnityEngine.Random.Range(0.01f, 0.05f);
                yield return new WaitForSeconds(UnityEngine.Random.Range(0.05f, 0.2f));
            }
        }

        private IEnumerator FinishLoadingCoroutine()
        {
            // 瞬间补满到 100%
            UpdateUI(1.0f);

            yield return new WaitForSeconds(0.2f);

            if (autoHidePanel)
            {
                loadingPanel.SetActive(false);
            }

            // 停止之前的伪进度协程（如果有必要，可以记录Coroutine引用来Stop）
        }

        private void UpdateUI(float progress)
        {
            progressSlider.value = Mathf.Clamp01(progress);
            progressText.text = $"{Mathf.CeilToInt(progress * 100)}%";
        }

        #endregion
}

