using Unity.Netcode;
using UnityEngine;

public class TreasureChest : NetworkBehaviour, IInteractable
{
    public enum ChestType { Standard, Mutation, ChaosAltar }
    public ChestType currentChestType = ChestType.Standard;
    [SerializeField] private GameObject chest_Open;
    [SerializeField] private GameObject chest_Close;

    private NetworkVariable<bool> isOpened = new NetworkVariable<bool>(false);

    public bool IsInteractable => !isOpened.Value;

    public string InteractPrompt
    {
        get
        {
            switch (currentChestType)
            {
                case ChestType.Mutation: return "按 [F] 提取异变核心";
                case ChestType.ChaosAltar: return "按 [F] 献祭 (失去30%生命)";
                default: return "按 [F] 开启武装箱";
            }
        }
    }

    private void OnEnable()
    {
        chest_Close.SetActive(true);
        chest_Open.SetActive(false);
    }

    // ==========================================
    // 1. 客户端发起请求
    // ==========================================
    public void OnInteract(GameObject interactor)
    {
        if (isOpened.Value) return;

        var netObj = interactor.GetComponentInParent<NetworkObject>();
        if (netObj == null || !netObj.IsOwner) return;

        // 把互动的玩家ID传给服务器，让服务器去裁决扣血和开箱！
        RequestOpenChestServerRpc(netObj.NetworkObjectId);
    }

    // ==========================================
    // 2. 服务器权威裁决 (扣血、锁状态)
    // ==========================================
    [ServerRpc(RequireOwnership = false)]
    private void RequestOpenChestServerRpc(ulong interactorId)
    {
        if (isOpened.Value) return;

        // 祭坛特殊逻辑：服务器负责真实扣血！
        if (currentChestType == ChestType.ChaosAltar)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(interactorId, out NetworkObject playerObj))
            {
                Health playerHealth = playerObj.GetComponent<Health>();
                if (playerHealth != null)
                {
                    float damageAmount = playerHealth.maxHealth.Value * 0.3f;
                    // 使用真实伤害(isTrueDamage = true)无视护甲，hitWeight传0无硬直
                    playerHealth.TakeDamage(damageAmount, transform.position, Vector3.zero, 0f, null, true);
                }
            }
        }

        isOpened.Value = true; // 全网锁死

        // 通知全宇宙所有客户端：开箱啦，发奖励啦！
        OpenChestAndShowUIClientRpc(currentChestType);
    }

    // ==========================================
    // 3. 客户端各自弹 UI
    // ==========================================
    [ClientRpc]
    private void OpenChestAndShowUIClientRpc(ChestType type)
    {
        // 1. 播放表现
        PlayOpenVisuals();

        // 2. 极其关键：让每个客户端只给【自己的本地玩家】弹 UI！
        foreach (var player in PlayerManager.Instance.AllPlayers)
        {
            if (player.IsOwner)
            {
                PlayerModifierHandler handler = player.GetComponent<PlayerModifierHandler>();
                if (handler != null)
                {
                    switch (type)
                    {
                        case ChestType.Standard: handler.OpenStandardChest(); break;
                        case ChestType.Mutation: handler.OpenMutationChest(); break;
                        case ChestType.ChaosAltar: handler.OpenChaosChest(); break;
                    }
                }
                break; // 找到了自己的玩家，弹完就跳出
            }
        }
    }

    private void PlayOpenVisuals()
    {
        chest_Open.SetActive(true);
        chest_Close.SetActive(false);
        Debug.Log("宝箱开启！");
    }
}