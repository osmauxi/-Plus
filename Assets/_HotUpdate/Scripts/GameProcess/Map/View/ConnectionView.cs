using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Map.View
{
    /// <summary>
    /// 两个房间之间连接表现的静态资源描述 
    /// 可以表示短通道、走廊、桥梁或传送通道 
    /// 一条 MapConnectionDefinition 只创建一个 ConnectionView 
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ConnectionView : MonoBehaviour
    {
        [Header("模型设置")]
        [Tooltip("通道模型根物体")]
        [SerializeField] private Transform _stretchRoot;

        [Tooltip("通道模型在缩放为 1 时的基础长度，沿本地 Z 轴计算 ")]
        [SerializeField, Min(0.1f)] private float _baseLength = 1f;

        [Header("阻挡门")]
        [Tooltip("战斗期间用于阻挡玩家的唯一门 ")]
        [SerializeField] private GameObject _battleGateRoot;

        [Header("判定设置")]
        [Tooltip("靠近 Room A 一侧的入口判定平面 forward 应朝向 Room A 内部 ")]
        [SerializeField] private Transform _portalA;

        [Tooltip("靠近 Room B 一侧的入口判定平面 forward 应朝向 Room B 内部 ")]
        [SerializeField] private Transform _portalB;

        public Transform StretchRoot => _stretchRoot;
        public float BaseLength => _baseLength;
        public GameObject BattleGateRoot => _battleGateRoot;
        public Transform PortalA => _portalA;
        public Transform PortalB => _portalB;
    }
}