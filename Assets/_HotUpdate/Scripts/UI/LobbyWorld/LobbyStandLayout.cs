using System;
using UnityEngine;

namespace ProjectGame.HotFix.Lobby
{
    /// <summary>
    /// 大厅展位的唯一场景布局定义。所有 UI、模型和运镜都从这里取得展位引用。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LobbyStandLayout : MonoBehaviour
    {
        [SerializeField] private StandView[] _stands;

        public int Count => _stands.Length;

        /// <summary>
        /// 取得指定索引的展位视图。
        /// </summary>
        public StandView GetStand(int index)
        {
            if ((uint)index >= (uint)_stands.Length)
                throw new ArgumentOutOfRangeException(nameof(index), index, "展位索引越界");

            return _stands[index];
        }

        /// <summary>
        /// 取得指定展位的玩家模型生成点。
        /// </summary>
        public Transform GetPlayerSpawnPos(int index) => GetStand(index).PlayerSpawnPos;

        /// <summary>
        /// 取得指定展位的相机聚焦点。
        /// </summary>
        public Transform GetCameraFocusPos(int index) => GetStand(index).CameraFocusPos;
    }
}
