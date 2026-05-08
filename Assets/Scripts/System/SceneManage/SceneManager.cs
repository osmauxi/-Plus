using System.Collections;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneManager : MonoBehaviour// 保留SceneManagerGlobal作为全局场景控制器，复用AsynchronousLoader的加载功能
{
    public static SceneManager Instance { get; private set; }
    public GameObject playerPrefab;
    [Header("加载配置")]
    [SerializeField] private float minLoadTime = 0.5f;
    private string currentGameSceneName = "";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void TransitionToGameScene()
    {
        GameStateController.instance.ChangeState(GameState.GameLoading);

        // 2. 首先以 Single 模式加载 UIScene (作为常驻底座)
        // 注意：这里建议使用 Unity 原生的 SceneManager 或是 NetworkSceneManager
        // 假设 AsynchronousLoader 封装了 NetworkSceneManager.LoadScene
        AsynchronousLoader.Instance.LoadScene("UIScene", LoadSceneMode.Single, () =>
        {
            // 3. 在 UIScene 加载完成后，生成玩家
            // 确保玩家生成的代码会将玩家移动到 UIScene 中
            SpawnAndSetupPlayers();

            // 4. 接着以 Additive 方式叠加加载初始的 GameScene
            AsynchronousLoader.Instance.LoadScene("GameScene", LoadSceneMode.Additive, () =>
            {
                // 5. 关卡准备就绪，进入地图生成状态
                GameStateController.instance.ChangeState(GameState.MapGenerating);
            });
        });
    }
    private void SpawnAndSetupPlayers()
    {
        if (!NetworkManager.Singleton.IsServer) return;

        // 获取当前加载好的 UIScene 引用
        Scene uiScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName("UIScene");

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            // 调用你原有的生成逻辑

            GameObject playerInstance = Instantiate(playerPrefab);
            NetworkObject netObj = playerInstance.GetComponent<NetworkObject>();
            Rigidbody rb = playerInstance.GetComponent<Rigidbody>();
            rb.interpolation = RigidbodyInterpolation.None;
            playerInstance.transform.position = new Vector3(0, 1, 0);
            netObj.SpawnAsPlayerObject(client.ClientId, true);
            //生成后，将玩家对象移入 UIScene
            var playerObj = client.PlayerObject;
            if (playerObj != null)
            {
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(playerObj.gameObject, uiScene);
            }
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }

    public void TransitionToNextLayer()
    {
        if (!NetworkManager.Singleton.IsServer) return;

        // 1. 切入过场状态，冻结游戏逻辑
        GameStateController.instance.ChangeState(GameState.MapExchanging);

        // 2. 层数增加 (GameDirector 会自动读取到这个新层数)
        GameStateController.instance.CurrentLevel.Value++;

        // 3. 强行回收旧地图里所有的门、房间预制体
        RoomManager.Instance.ClearAllLevelVisuals();

        // 4. 开始极其优雅的 卸载 -> 重新加载 流程
        AsynchronousLoader.Instance.UnLoadScene("GameScene", () =>
        {
            // 旧的 GameScene 连同里面的对象池、尸体、子弹已经全部灰飞烟灭
            AsynchronousLoader.Instance.LoadScene("GameScene", LoadSceneMode.Additive, () =>
            {
                // 等待一帧，确保新场景的网络对象完成初始化
                StartCoroutine(RepositionAndStartNextLayer());
            });
        });
    }

    private IEnumerator RepositionAndStartNextLayer()
    {
        yield return null;

        foreach (var player in PlayerManager.Instance.AllPlayers)
        {
            Vector3 newPos = new Vector3(UnityEngine.Random.Range(-2f, 2f), 3f, UnityEngine.Random.Range(-2f, 2f));
            player.TeleportClientRpc(newPos);
        }

        // 6. 指挥发牌员和地图生成器开工！
        GameStateController.instance.ChangeState(GameState.MapGenerating);
    }

    public void LoadPanelOn()
    {
        AsynchronousLoader.Instance.loadingPanel.SetActive(true);
    }
    public Scene GetSceneByName(string name) 
    {
        return UnityEngine.SceneManagement.SceneManager.GetSceneByName(name);
    }
}

