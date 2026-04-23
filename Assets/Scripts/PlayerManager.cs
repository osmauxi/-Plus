using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using System.Linq;

//网络调试与展示中心，包含连接信息、延迟模拟、同步模式、踢人、帧同步参数、Desync测试等。
public class PlayerManager : NetworkBehaviour
{
    public static PlayerManager Instance;
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

    /// <summary>
    /// AI 专用的公有方法库：获取距离最近的玩家
    /// </summary>
    public Transform GetNearestPlayer(Vector3 searchPosition)
    {
        if (AllPlayers.Count == 0) return null;

        Transform nearestPlayer = null;
        float minDistance = float.MaxValue;

        foreach (var player in AllPlayers)
        {
            float dist = Vector3.SqrMagnitude(player.transform.position - searchPosition); // SqrMagnitude 比 Distance 快，因为它不计算平方根
            if (dist < minDistance)
            {
                minDistance = dist;
                nearestPlayer = player.transform;
            }
        }
        return nearestPlayer;
    }

}
