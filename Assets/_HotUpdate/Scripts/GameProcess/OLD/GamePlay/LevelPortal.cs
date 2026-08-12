using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LevelPortal : NetworkBehaviour
{
    int currentCount = 0;

    // ==========================================
    // 1. 传送门“出现”的音效
    // ==========================================
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        // 当传送门被刷出来的瞬间，所有客户端播放激活音效
        AudioManager.instance.PlaySFXByCategory(AudioCategory.SFX_Portal_Activate, 1f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;
        if (other.CompareTag("Player"))
        {
            currentCount++;
            CheckPortalActivation();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer) return;
        if (other.CompareTag("Player"))
        {
            currentCount--;
        }
    }

    private void CheckPortalActivation()
    {
        //int totalPlayers = PlayerManager.Instance.AllPlayers.Count;
        //Debug.Log($"[传送门] 站圈人数: {currentCount}/{totalPlayers}");

        //// 如果所有活着的玩家都站在圈里，触发跨层管线！
        //if (currentCount == totalPlayers)
        //{
        //    // 告诉所有客户端：我们要传送啦，放音效！
        //    PlayTeleportSFXClientRpc();

        //    // 执行场景切换
        //    MySceneManager.Instance.TransitionToNextLayer();
        //}
    }

    // ==========================================
    // 2. 传送门“传送全队”的音效
    // ==========================================
    [ClientRpc]
    private void PlayTeleportSFXClientRpc()
    {
        // 播放通关传送音效 (上面在映射时借用了 LevelUp 的声音，会非常清脆悦耳)
        AudioManager.instance.PlaySFXByCategory(AudioCategory.SFX_Portal_Teleport, 1f);
    }
}