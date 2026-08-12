using Unity.Netcode;
using UnityEngine;

public class PlayerDebugTools : NetworkBehaviour
{
    private PlayerModifierHandler modifierHandler;

    private void Awake()
    {
        modifierHandler = GetComponent<PlayerModifierHandler>();
    }

    private void Update()
    {
        // 必须是本地玩家才能按键测试，防止影响到联机房间里的其他玩家
        if (!IsOwner) return;

        // 【K 键】：测试普通武器箱 (带流派倾向和互斥)
        if (Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("<color=green>[Debug] 强制触发：常规武装箱</color>");
            modifierHandler.OpenChestFromTrigger(TreasureChest.ChestType.Standard, false);
        }

        // 【I 键】：测试无冲突普通武器箱 (爆金光版 15%概率)
        if (Input.GetKeyDown(KeyCode.I))
        {
            Debug.Log("<color=yellow>[Debug] 强制触发：✨混沌武装赐福✨ (无冲突普通箱)</color>");
            modifierHandler.OpenChestFromTrigger(TreasureChest.ChestType.Standard, true);
        }

        // 【L 键】：测试异变宝箱 (测试你的机制特效是否能正常挂载)
        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log("<color=magenta>[Debug] 强制触发：异变核心提取</color>");
            modifierHandler.OpenChestFromTrigger(TreasureChest.ChestType.Mutation, false);
        }

        // 【O 键】：测试无冲突异变宝箱 (爆金光版 15%概率)
        if (Input.GetKeyDown(KeyCode.O))
        {
            Debug.Log("<color=orange>[Debug] 强制触发：✨混沌异变核心✨ (无冲突异变箱)</color>");
            modifierHandler.OpenChestFromTrigger(TreasureChest.ChestType.Mutation, true);
        }

        // 【J 键】：测试混沌祭坛 (纯随机抽卡，测试底线限制)
        if (Input.GetKeyDown(KeyCode.J))
        {
            Debug.Log("<color=red>[Debug] 强制触发：混沌祭坛 (不扣血版)</color>");
            modifierHandler.OpenChestFromTrigger(TreasureChest.ChestType.ChaosAltar, false);
        }
    }
}