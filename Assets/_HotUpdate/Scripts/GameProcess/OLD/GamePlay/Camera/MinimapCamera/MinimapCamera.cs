using System.Collections;
using UnityEngine;

public class MinimapCamera : MonoBehaviour
{
    public static MinimapCamera Instance;

    [Header("平滑设置")]
    public float smoothSpeed = 5f; // 注意：由于使用了 Time.deltaTime，这里的速度建议设为 5-10 左右
    public float stopThreshold = 0.01f;

    private Coroutine moveCoroutine; // 用于缓存当前正在执行的协程，防止冲突

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // 自动订阅 RoomManager 的房间切换事件
        // 只要有人（或系统）改了这个值，摄像机就会自动跟过去
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.CurrentActiveRoom.OnValueChanged += OnRoomChanged;
        }
    }

    private void OnDestroy()
    {
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.CurrentActiveRoom.OnValueChanged -= OnRoomChanged;
        }
    }

    // 网络变量改变时的回调
    private void OnRoomChanged(Vector2Int oldRoom, Vector2Int newRoom)
    {
        ChangeCameraPos(newRoom);
    }

    public void ChangeCameraPos(Vector2Int gridPos)
    {
        // 关键修复：必须传入正在运行的协程引用才能真正停掉它！
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }
        moveCoroutine = StartCoroutine(SmoothMoveToNode(gridPos));
    }

    // 【新增功能】瞬间定位：用于刚进游戏或重置关卡时，摄像机立刻就位
    public void SnapToRoom(Vector2Int gridPos)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }
        transform.position = new Vector3(gridPos.x, gridPos.y, transform.position.z);
    }

    private IEnumerator SmoothMoveToNode(Vector2Int targetGrid)
    {
        // 纯数学推算：小地图节点的生成规律就是 1格=1个世界单位
        Vector3 targetCameraPos = new Vector3(targetGrid.x, targetGrid.y, transform.position.z);

        while (Vector3.Distance(transform.position, targetCameraPos) > stopThreshold)
        {
            Vector3 smoothedPos = Vector3.Lerp(transform.position, targetCameraPos, smoothSpeed * Time.deltaTime);
            transform.position = smoothedPos;
            yield return null;
        }

        // 确保最终精确到达
        transform.position = targetCameraPos;
        moveCoroutine = null;
    }
}