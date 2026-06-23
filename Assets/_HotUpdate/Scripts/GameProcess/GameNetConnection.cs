using DG.Tweening; // 引入 DOTween
using System;
using System.Net;
using System.Net.Sockets;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class GameNetConnection : NetworkBehaviour
{
    private const ushort DefaultPort = 7777;

    [Header("创建房间 UI (Host)")]
    public CanvasGroup hostPanelGroup;
    public TextMeshProUGUI hostStatusText;
    [Tooltip("预留接口：用于后续展示多人联机时的玩家模型")]
    public Transform multiplayerModelShowcaseAnchor;

    [Header("加入房间 UI (Client)")]
    public CanvasGroup joinPanelGroup;
    public TextMeshProUGUI joinStatusText; // 实时状态反馈

    private bool isSolo = false;

    private void Awake()
    {
        // 初始隐藏两个面板
        if (hostPanelGroup != null) hostPanelGroup.gameObject.SetActive(false);
        if (joinPanelGroup != null) joinPanelGroup.gameObject.SetActive(false);
    }

    private void Start()
    {
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
        NetworkManager.Singleton.OnServerStopped += OnServerStopped;
        NetworkManager.Singleton.OnClientStarted += OnClientStarted;
        NetworkManager.Singleton.OnClientStopped += OnClientStopped;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnTransportFailure += OnTransportFailure;
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
            NetworkManager.Singleton.OnServerStopped -= OnServerStopped;
            NetworkManager.Singleton.OnClientStarted -= OnClientStarted;
            NetworkManager.Singleton.OnClientStopped -= OnClientStopped;
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnTransportFailure -= OnTransportFailure;
        }
    }

    #region 按钮交互逻辑

    /// <summary> 点击：单人游戏 </summary>
    public void OnStartSoloClicked()
    {
        isSolo = true;
        StartNetwork(true, "127.0.0.1");
    }

    /// <summary> 点击：创建房间 </summary>
    public void OnCreateRoomClicked()
    {
        isSolo = false;
        OpenPanelWithAnim(hostPanelGroup);
        hostStatusText.text = "正在启动房间...";
        StartNetwork(true, "0.0.0.0");
    }

    /// <summary> 点击：打开加入房间面板 </summary>
    public void OnOpenJoinPanelClicked()
    {
        OpenPanelWithAnim(joinPanelGroup);
        joinStatusText.text = "请输入主机 IP 地址";
    }

    /// <summary> 点击：确认加入房间 (在 Join 面板内) </summary>
    public void OnInputConfirm(string inputIP)
    {
        string cleanIP = inputIP.Trim().Replace("\u200B", "");
        if (string.IsNullOrWhiteSpace(cleanIP))
        {
            joinStatusText.text = "<color=red>IP 不能为空！</color>";
            return;
        }

        joinStatusText.text = $"正在连接 {cleanIP} ...";
        StartNetwork(false, cleanIP);
    }

    /// <summary> 点击：取消创建房间 (在 Host 面板内) </summary>
    public void OnCancelHostClicked()
    {
        ClosePanelWithAnim(hostPanelGroup);
        NetworkManager.Singleton?.Shutdown();
    }

    /// <summary> 点击：取消加入房间 (在 Join 面板内) </summary>
    public void OnCancelJoinClicked()
    {
        ClosePanelWithAnim(joinPanelGroup);
        NetworkManager.Singleton?.Shutdown();
    }

    #endregion

    #region 核心网络启动逻辑

    private void StartNetwork(bool isHost, string ip)
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(ip, DefaultPort);

        if (isHost)
        {
            if (isSolo) GameStateController.instance.isSolo.Value = true;
            NetworkManager.Singleton.StartHost();
        }
        else
        {
            NetworkManager.Singleton.StartClient();
        }
    }

    #endregion

    #region 网络事件回调

    private void OnServerStarted()
    {
        if (isSolo || GameStateController.instance.isSolo.Value)
        {
            // 单人模式，直接转场
            SceneManager.Instance.TransitionToGameScene();
        }
        else
        {
            // 多人模式：更新 UI，等待玩家加入
            string localIP = GetLocalIPAddress();
            hostStatusText.text = $"房间已创建\n你的IP: <color=green>{localIP}</color>\n等待队友加入 (1/2)...";
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            // 只要人数达到2，直接发车加载场景
            if (NetworkManager.Singleton.ConnectedClientsList.Count == 2)
            {
                hostStatusText.text = "队友已就位，正在进入游戏...";
                SceneManager.Instance.TransitionToGameScene();
            }
        }
        else if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            // 我是客户端，且我成功连上了
            joinStatusText.text = "<color=green>连接成功！等待主机开始游戏...</color>";
        }
    }

    private void OnClientStarted()
    {
        if (!NetworkManager.Singleton.IsServer)
            joinStatusText.text = "正在尝试连接...";
    }

    private void OnClientStopped(bool wasHost)
    {
        var reason = NetworkManager.Singleton.DisconnectReason;
        string errorMsg = string.IsNullOrEmpty(reason) ? "连接被断开" : reason;

        if (wasHost && hostPanelGroup.gameObject.activeSelf)
        {
            hostStatusText.text = $"房间已关闭: {errorMsg}";
        }
        else if (!wasHost && joinPanelGroup.gameObject.activeSelf)
        {
            joinStatusText.text = $"<color=red>连接失败: {errorMsg}</color>";
        }
    }

    private void OnServerStopped(bool _)
    {
        if (hostPanelGroup.gameObject.activeSelf)
            hostStatusText.text = "服务器已关闭";
    }

    private void OnTransportFailure()
    {
        if (joinPanelGroup.gameObject.activeSelf)
            joinStatusText.text = "<color=red>网络错误，请检查IP或端口</color>";
    }

    #endregion

    #region DOTween UI 动效封装

    private void OpenPanelWithAnim(CanvasGroup cg)
    {
        if (cg == null) return;

        // 停掉可能正在播放的动画
        cg.DOKill();
        cg.transform.DOKill();

        cg.gameObject.SetActive(true);
        cg.alpha = 0f;
        cg.transform.localScale = Vector3.one * 0.8f; // 从 80% 大小开始放大

        // 渐隐渐现 + 果冻弹出效果
        cg.DOFade(1f, 0.3f);
        cg.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
    }

    private void ClosePanelWithAnim(CanvasGroup cg)
    {
        if (cg == null) return;

        cg.DOKill();
        cg.transform.DOKill();

        // 缩小并变透明，完成后关闭节点
        cg.DOFade(0f, 0.2f);
        cg.transform.DOScale(0.8f, 0.2f).SetEase(Ease.InBack).OnComplete(() =>
        {
            cg.gameObject.SetActive(false);
        });
    }

    #endregion

    private string GetLocalIPAddress()
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
        catch { }
        return "未知";
    }
}