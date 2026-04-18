using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using System.Linq;

//网络调试与展示中心，包含连接信息、延迟模拟、同步模式、踢人、帧同步参数、Desync测试等。
public class GameDebugManager : NetworkBehaviour
{
    public static GameDebugManager Instance;
    public List<PlayerController> AllPlayers = new List<PlayerController>();

    public float GlobalSimulatedLatency { get; private set; } = 0f;//全局延迟变量
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void RegisterPlayer(PlayerController pc)
    {
        if (!AllPlayers.Contains(pc))
        {
            AllPlayers.Add(pc);
            AllPlayers.Sort((a, b) => a.OwnerClientId.CompareTo(b.OwnerClientId));
            Debug.Log($"[玩家注册] ID:{pc.OwnerClientId}, 当前总数:{AllPlayers.Count}");
        }
    }

    public void UnregisterPlayer(PlayerController pc)
    {
        if (AllPlayers.Contains(pc))
        {
            AllPlayers.Remove(pc);
            AllPlayers.Sort((a, b) => a.OwnerClientId.CompareTo(b.OwnerClientId));
            Debug.Log($"[玩家移除] ID:{pc.OwnerClientId}, 当前总数:{AllPlayers.Count}");
        }
    }

    public override void OnNetworkSpawn()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
    }

    public override void OnNetworkDespawn()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
    }

    private void OnSpawnObject(NetworkObject obj)
    {
        if (obj.TryGetComponent<PlayerController>(out var pc))
        {
            if (!AllPlayers.Contains(pc))
            {
                AllPlayers.Add(pc);
                AllPlayers.Sort((a, b) => a.OwnerClientId.CompareTo(b.OwnerClientId));
                Debug.Log($"玩家加入列表: {pc.OwnerClientId}, 当前总数: {AllPlayers.Count}");
            }
        }
    }

    private void OnClientDisconnect(ulong clientId)
    {
        var playerToRemove = AllPlayers.FirstOrDefault(p => p.OwnerClientId == clientId);
        if (playerToRemove != null)
        {
            AllPlayers.Remove(playerToRemove);
            Debug.Log($"玩家移除列表: {clientId}");
        }
    }

}
