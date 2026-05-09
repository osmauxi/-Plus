using Unity.Netcode;
using UnityEngine;

public class TreasureChest : NetworkBehaviour, IInteractable
{
   // [Header("宝箱类型")]
    public enum ChestType { Standard, Mutation, ChaosAltar }
    public ChestType currentChestType = ChestType.Standard;
    [SerializeField] private GameObject chest_Open;
    [SerializeField] private GameObject chest_Close;

    // 网络变量：同步箱子是否已经被开启，防止网络延迟导致两个人同时开一个箱子
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
    public void OnInteract(GameObject interactor)
    {
        // 1. 防御性检查：已经被开了，或者不是本地玩家按的，直接 return
        if (isOpened.Value) return;

        // 找到操作者的本地处理句柄
        PlayerModifierHandler modifierHandler = interactor.GetComponentInParent<PlayerModifierHandler>();
        if (!modifierHandler.IsOwner) return;

        // 2. 献祭祭坛的特殊逻辑：先扣血！
        if (currentChestType == ChestType.ChaosAltar)
        {
            Health playerHealth = interactor.GetComponent<Health>();
            if (playerHealth != null)
            {
                // 假设你的 Health 脚本有对应方法，这里扣除 30% 最大生命值
                float damageAmount = playerHealth.maxHealth.Value * 0.3f;
                playerHealth.TakeDamage(damageAmount, transform.position, Vector3.zero);
            }
        }

        // 3. 通知服务器：这个箱子我开了！(锁住状态，让别人点不了)
        RequestOpenChestServerRpc();

        // 4. 在本地立刻弹出对应的词条抽取 UI！
        switch (currentChestType)
        {
            case ChestType.Standard:
                Debug.Log(414145);
                modifierHandler.OpenStandardChest();
                break;
            case ChestType.Mutation:
                modifierHandler.OpenMutationChest();
                break;
            case ChestType.ChaosAltar:
                modifierHandler.OpenChaosChest();
                break;
        }

        // 可选：本地先播放一次开箱粒子/动画掩盖网络延迟
        PlayOpenVisuals();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestOpenChestServerRpc()
    {
        if (isOpened.Value) return;

        isOpened.Value = true; // 真正锁死状态
        OpenChestVisualsClientRpc();
    }

    [ClientRpc]
    private void OpenChestVisualsClientRpc()
    {
        // 所有的客户端都会执行这里：播放箱子盖子打开的动画、喷金光等
        PlayOpenVisuals();
    }

    private void PlayOpenVisuals()
    {
        chest_Open.SetActive(true);
        chest_Close.SetActive(false);
        // TODO: 播放 Animator 动画，或者直接换材质/模型
        Debug.Log("宝箱开启！");

        // 可选：开完后过几秒把自己还给对象池
        // if(IsServer) StartCoroutine(RecycleRoutine());
    }
}