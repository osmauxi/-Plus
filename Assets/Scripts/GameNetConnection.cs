using System;
using System.Net;
using System.Net.Sockets;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using TMPro;

/// <summary>
/// 局域网联机总控脚本，负责 Host/Client 启动、IP 输入与连接状态展示。
/// </summary>
public class GameNetConnection : NetworkBehaviour
{
    private const ushort DefaultPort = 7777;

    #region Inspector

    [Header("UI 引用")]
    [SerializeField] private GameObject IpSetUI;
    [SerializeField] private TextMeshProUGUI statusText;
    [Tooltip("可选：显示主机 IP 供他人连接")]
    [SerializeField] private TextMeshProUGUI hostIPHintText;

    #endregion
    #region 连接状态

    public enum ConnectionStatus
    {
        Disconnected,   // 未连接
        StartingHost,   // 正在启动主机
        StartingClient, // 正在连接
        HostRunning,    // 主机运行中
        ClientConnected,// 客户端已连接
        Disconnecting,  // 正在断开
        Failed         // 连接失败
    }

    private ConnectionStatus _status = ConnectionStatus.Disconnected;
    private string _lastStatusMessage = "";
    public bool IsSoloMode = false;
    /// <summary> 当前连接状态 </summary>
    public ConnectionStatus Status => _status;

    /// <summary> 可显示给用户的完整状态文本 </summary>
    public string StatusDisplayText => BuildStatusText();

    /// <summary> 状态变更时触发，便于 UI 订阅更新 </summary>
    public event Action<ConnectionStatus, string> OnStatusChanged;

    #endregion

    private void Awake()
    {
        if (IpSetUI != null)
            IpSetUI.SetActive(false);
        RefreshStatusDisplay();
    }

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
        NetworkManager.Singleton.OnServerStopped += OnServerStopped;
        NetworkManager.Singleton.OnClientStarted += OnClientStarted;
        NetworkManager.Singleton.OnClientStopped += OnClientStopped;
        NetworkManager.Singleton.OnTransportFailure += OnTransportFailure;
        NetworkManager.Singleton.OnClientConnectedCallback += OnPlayerJoined;
    }

    private void OnDisable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
        NetworkManager.Singleton.OnServerStopped -= OnServerStopped;
        NetworkManager.Singleton.OnClientStarted -= OnClientStarted;
        NetworkManager.Singleton.OnClientStopped -= OnClientStopped;
        NetworkManager.Singleton.OnTransportFailure -= OnTransportFailure;
        NetworkManager.Singleton.OnClientConnectedCallback -= OnPlayerJoined;
    }

    private void Update()
    {
        if (_status == ConnectionStatus.HostRunning || _status == ConnectionStatus.ClientConnected)
            RefreshStatusDisplay();
    }

    #region UI 按钮回调
    public void OnStartSoloB()
    {
        IsSoloMode = true;
        // 单人模式：启动 Host 即可，不等待他人连接
        StartGame(true, "127.0.0.1");
    }
    public void OnStartHostB()
    {
        StartGame(true, "127.0.0.1");
    }

    public void OnStartClientB()
    {
        if (IpSetUI != null)
            IpSetUI.SetActive(true);
    }

    public void GetIPInput(string ip)
    {
        string cleanIP = (ip ?? string.Empty).Trim().Replace("\u200B", "");
        StartGame(false, cleanIP);
    }

    public void OnShutdownNetworkB()
    {
        SetStatus(ConnectionStatus.Disconnecting, "正在断开连接...");
        NetworkManager.Singleton?.Shutdown();
    }

    #endregion
    #region 网络启动

    public void StartGame(bool isHost, string targetIP)
    {
        if (NetworkManager.Singleton == null)
        {
            SetStatus(ConnectionStatus.Failed, "NetworkManager 未找到");
            Debug.LogWarning("NetworkManager.Singleton is null.");
            return;
        }

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (transport == null)
        {
            SetStatus(ConnectionStatus.Failed, "UnityTransport 未找到");
            return;
        }

        if (isHost)
        {
            SetStatus(ConnectionStatus.StartingHost, "正在启动主机...");
            transport.SetConnectionData("0.0.0.0", DefaultPort);
            if (NetworkManager.Singleton.StartHost())
                Debug.Log("Host Started");
            else
                SetStatus(ConnectionStatus.Failed, "主机启动失败");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(targetIP))
            {
                SetStatus(ConnectionStatus.Failed, "请输入 Host 的 IP 地址");
                return;
            }

            SetStatus(ConnectionStatus.StartingClient, $"正在连接 {targetIP}:{DefaultPort}...");
            transport.SetConnectionData(targetIP, DefaultPort);
            if (NetworkManager.Singleton.StartClient())
                Debug.Log("Client Started - 等待服务器切换场景...");
            else
                SetStatus(ConnectionStatus.Failed, "客户端启动失败");
        }
    }

    #endregion
    #region 网络事件

    private void OnServerStarted()
    {
        string localIP = GetLocalIPAddress();
        SetStatus(ConnectionStatus.HostRunning, $"主机已启动 | 本机IP: {localIP}");

        // 如果是单人模式或者双人联机的创建者
        // 我们在这里通知 SceneManager 开始初始场景加载
        if (NetworkManager.Singleton.IsHost)
        {
            if (IsSoloMode)
            {
                // 先进入加载流程：UIScene -> GameScene
                SceneManager.Instance.TransitionToGameScene();
            }
        }
    }
    private void OnPlayerJoined(ulong clientId)
    {
        if (!IsServer) return;

        // 如果当前在“等待”状态，且人数达到了 2 人（Host + Client）
        if (NetworkManager.Singleton.ConnectedClientsList.Count == 2)
        {
            Debug.Log("队友已就位，开始加载游戏关卡...");
            // 由主机发起，所有人同步加载 GameScene
            SceneManager.Instance.TransitionToGameScene();
        }
    }
    private void OnServerStopped(bool _)
    {
        SetStatus(ConnectionStatus.Disconnected, "主机已关闭");
        UpdateHostIPHint("");
    }

    private void OnClientStarted()
    {
        if (!NetworkManager.Singleton.IsHost)
            SetStatus(ConnectionStatus.StartingClient, "正在连接服务器...");
    }

    private void OnClientStopped(bool wasHost)
    {
        var reason = NetworkManager.Singleton?.DisconnectReason ?? "";
        string msg = string.IsNullOrEmpty(reason) ? "已断开连接" : $"断开: {reason}";
        SetStatus(ConnectionStatus.Disconnected, msg);

        if (!wasHost && IpSetUI != null)
            IpSetUI.SetActive(true);
    }

    private void OnClientConnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            SetStatus(ConnectionStatus.ClientConnected, "已连接服务器");
            if (IpSetUI != null)
                IpSetUI.SetActive(false);
        }
        else if (NetworkManager.Singleton.IsServer)
        {
            RefreshStatusDisplay();
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (NetworkManager.Singleton.IsServer)
            RefreshStatusDisplay();
    }

    private void OnTransportFailure()
    {
        SetStatus(ConnectionStatus.Failed, "网络传输失败，请检查 IP 和端口");
        if (IpSetUI != null)
            IpSetUI.SetActive(true);
    }

    #endregion
    #region 状态与显示

    private void SetStatus(ConnectionStatus status, string message)
    {
        if (_status == status && _lastStatusMessage == message) return;

        _status = status;
        _lastStatusMessage = message;
        RefreshStatusDisplay();
        OnStatusChanged?.Invoke(status, message);
    }

    private string BuildStatusText()
    {
        var nm = NetworkManager.Singleton;
        if (nm == null) return _lastStatusMessage;

        if (_status == ConnectionStatus.HostRunning && nm.IsServer)
        {
            int count = nm.ConnectedClients?.Count ?? 0;
            return $"{_lastStatusMessage}\n当前人数: {count}/2";
        }

        if (_status == ConnectionStatus.ClientConnected && nm.IsClient)
        {
            return $"{_lastStatusMessage}\n你的 ID: {nm.LocalClientId}";
        }

        return _lastStatusMessage;
    }

    private void RefreshStatusDisplay()
    {
        string text = BuildStatusText();
        if (statusText != null)
            statusText.text = text;
    }

    private void UpdateHostIPHint(string ip)
    {
        if (hostIPHintText == null) return;
        hostIPHintText.text = string.IsNullOrEmpty(ip) ? "" : $"其他玩家请连接: {ip}";
    }

    /// <summary> 获取本机局域网 IP </summary>
    public static string GetLocalIPAddress()
    {
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip.ToString();
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"获取本机 IP 失败: {e.Message}");
        }
        return "未知";
    }

    #endregion
}
