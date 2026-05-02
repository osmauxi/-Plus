using Unity.Netcode;
using UnityEngine;

public class PlayerInteractor : NetworkBehaviour
{
    [Header("交互设置")]
    public float interactRadius = 2.0f;          // 拾取/交互范围
    public LayerMask interactableLayer;          // 专门给宝箱、控制台建一个 Layer，比如叫 "Interactable"

    // 0 GC 物理扫描数组
    private Collider[] overlapResults = new Collider[5];

    public override void OnNetworkSpawn()
    {
        // 只有本地玩家才需要处理输入和探测
        if (!IsOwner) this.enabled = false;
    }

    private void Update()
    {
        // 1. 如果在看 UI，不允许交互
        if (InputManager.Instance.CurrentState != InputState.Gameplay) return;

        // 2. 只有在按下 F 键的这一帧，才进行高代价的物理扫描！
        if (InputManager.Instance.InteractPressed)
        {
            TryInteract();
        }
    }

    private void TryInteract()
    {
        // 不产生内存碎片的球形扫描
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, interactRadius, overlapResults, interactableLayer);

        IInteractable nearestInteractable = null;
        float minDistance = float.MaxValue;

        // 遍历扫到的所有物体，找出离玩家最近的那一个（防止周围有三个箱子时不知道开哪个）
        for (int i = 0; i < hitCount; i++)
        {
            // 使用 GetComponentInParent 是为了防止碰撞体挂在子节点上
            IInteractable interactable = overlapResults[i].GetComponentInParent<IInteractable>();

            if (interactable != null && interactable.IsInteractable)
            {
                float dist = (transform.position - overlapResults[i].transform.position).sqrMagnitude;
                if (dist < minDistance)
                {
                    minDistance = dist;
                    nearestInteractable = interactable;
                }
            }
        }

        // 如果找到了合法的可交互物，直接调用它的接口！
        if (nearestInteractable != null)
        {
            nearestInteractable.OnInteract(this.gameObject);
        }
    }
}