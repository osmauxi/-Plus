using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LevelPortal : NetworkBehaviour
{
    int currentCount = 0;
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
        int totalPlayers = PlayerManager.Instance.AllPlayers.Count;
        // 如果所有活着的玩家都站在圈里，触发跨层管线！
        Debug.Log(totalPlayers);
        if (currentCount == totalPlayers)
        {
            SceneManager.Instance.TransitionToNextLayer();
        }
    }
}