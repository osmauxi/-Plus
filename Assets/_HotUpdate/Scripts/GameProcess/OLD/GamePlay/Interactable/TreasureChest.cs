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
                    playerHealth.TakeDamage(damageAmount, transform.position, Vector3.zero, 0f, null, true);
                }
            }
        }

        // ==========================================
        // 【新增 1】：全队恢复 20% 生命值
        // ==========================================
        //foreach (var player in PlayerManager.Instance.AllPlayers)
        //{
        //    if (player != null)
        //    {
        //        Health hp = player.GetComponent<Health>();
        //        if (hp != null && !hp.isDead)
        //        {
        //            float healAmount = hp.maxHealth.Value * 0.2f;
        //            // 直接在服务器修改血量，自动同步给所有客户端
        //            hp.currentHealth.Value = Mathf.Clamp(hp.currentHealth.Value + healAmount, 0f, hp.maxHealth.Value);
        //        }
        //    }
        //}

        isOpened.Value = true; // 全网锁死

        // ==========================================
        // 【新增 2】：15% 概率获得“无冲突随机宝箱”升级
        // ==========================================
        bool isChaosUpgrade = false;
        if (currentChestType == ChestType.Standard || currentChestType == ChestType.Mutation)
        {
            // 只有普通箱和异变箱有概率升级（混沌祭坛本身就是混沌池了，不用升）
            isChaosUpgrade = UnityEngine.Random.value <= 0.15f;
        }

        // 通知全宇宙所有客户端：开箱啦，发奖励啦！带上是否升级的标志位
        OpenChestAndShowUIClientRpc(currentChestType, isChaosUpgrade);
    }

    // ==========================================
    // 3. 客户端各自弹 UI
    // ==========================================
    [ClientRpc]
    private void OpenChestAndShowUIClientRpc(ChestType type, bool isChaosUpgrade)
    {
        if (TryGetComponent<TargetableIndicator>(out var indicator))
        {
            indicator.Unregister();
        }

        PlayOpenVisuals();

        // 极其关键：让每个客户端只给【自己的本地玩家】弹 UI！
        //foreach (var player in PlayerManager.Instance.AllPlayers)
        //{
        //    if (player.IsOwner)
        //    {
        //        PlayerModifierHandler handler = player.GetComponent<PlayerModifierHandler>();
        //        if (handler != null)
        //        {
        //            // 【修改】：使用统一的新接口，并将升级标志传过去
        //            handler.OpenChestFromTrigger(type, isChaosUpgrade);
        //        }
        //        break;
        //    }
        //}
    }
    private void PlayOpenVisuals()
    {
        chest_Open.SetActive(true);
        chest_Close.SetActive(false);
        Debug.Log("宝箱开启！");
        AudioManager.instance.PlaySFXByCategory(AudioCategory.SFX_Chest_Open, 1f);
    }
}