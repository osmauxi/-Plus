using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Map.View
{
    /// <summary>
    /// 房间连接插槽的静态资源描述，作为挂载脚本，只标明锚点
    /// 所有运行时操作由MapVisualBuilder统一完成。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RoomConnectorSlot : MonoBehaviour
    {
        [Header("ID")]
        [Tooltip("同一个房间预制体内必须唯一，例如 North、Entrance_A。")]
        [SerializeField] private string _slotId;

        [Tooltip("GridGraph 使用标准方向；自由走廊插槽使用 None。")]
        [SerializeField] private ConnectorDirection _direction;

        [Header("模型")]
        [Tooltip("该方向没有连接时显示的完整墙体。")]
        [SerializeField] private GameObject _closedWallRoot;

        [Tooltip("该方向存在连接时显示的门框或开口墙体。")]
        [SerializeField] private GameObject _openFrameRoot;

        [Tooltip("可选。Grid 无 ConnectionView 时，由该节点承担战斗封门显示。")]
        [SerializeField] private GameObject _battleGateRoot;

        [Header("判定范围")]
        [Tooltip("用于玩家进入房间判定的入口宽度。")]
        [SerializeField, Min(0.1f)] private float _portalWidth = 4f;

        [Tooltip("用于玩家进入房间判定的入口高度。")]
        [SerializeField, Min(0.1f)] private float _portalHeight = 3f;

        public string SlotId => _slotId;
        public ConnectorDirection Direction => _direction;

        // 当前组件所在的 Transform 就是连接锚点，不额外保存一层 Transform 引用。
        public Transform Anchor => transform;

        public GameObject ClosedWallRoot => _closedWallRoot;
        public GameObject OpenFrameRoot => _openFrameRoot;
        public GameObject BattleGateRoot => _battleGateRoot;

        public float PortalWidth => _portalWidth;
        public float PortalHeight => _portalHeight;

        private void OnDrawGizmosSelected()
        {
            Matrix4x4 previousMatrix = Gizmos.matrix;
            Color previousColor = Gizmos.color;

            /*
             * 使用当前 Connector 的本地坐标：
             * X 表示 Portal 宽度；
             * Y 表示 Portal 高度；
             * Z 表示 Portal 平面的法线方向。
             */
            Gizmos.matrix = transform.localToWorldMatrix;

            // 绘制数学入口平面。
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(_portalWidth, _portalHeight, 0.05f));

            // 绘制 forward，要求朝向房间外部。
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(Vector3.zero, Vector3.forward * 2f);
            Gizmos.DrawSphere(Vector3.forward * 2f, 0.08f);

            Gizmos.matrix = previousMatrix;
            Gizmos.color = previousColor;
        }
    }
}
