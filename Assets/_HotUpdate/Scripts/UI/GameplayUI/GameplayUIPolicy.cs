using System;
using UnityEngine;

namespace ProjectGame.HotFix.UI.Gameplay
{
    public enum GameplayUILayer : byte
    {
        /// <summary>
        /// 局内常驻信息层，和游戏场景并行，不阻断输入
        /// </summary>
        Hud,
        /// <summary>
        /// 独占全屏页面，同时只能存在一个
        /// </summary>
        Screen,
        /// <summary>
        /// 盖在最顶层，强制拦截输入，栈式叠加
        /// </summary>
        Modal,
    }

    /// <summary>
    /// P层默认持有一个，根据自定义性质实现切换时的对应操作，比如有些不用隐藏，有些不阻断操作等
    /// </summary>
    [Serializable]
    public struct GameplayUIPolicy
    {
        [SerializeField] private GameplayUILayer _layer;
        [SerializeField] private bool _blocksGameplayInput;
        [SerializeField] private bool _canCloseOnBack;
        [SerializeField] private bool _hidesHud;

        public GameplayUILayer Layer => _layer;
        public bool BlocksGameplayInput => _blocksGameplayInput;
        public bool CanCloseOnBack => _canCloseOnBack;
        public bool HidesHud => _hidesHud;

        public GameplayUIPolicy(GameplayUILayer layer, bool blocksGameplayInput,
            bool canCloseOnBack, bool hidesHud = false)
        {
            _layer = layer;
            _blocksGameplayInput = blocksGameplayInput;
            _canCloseOnBack = canCloseOnBack;
            _hidesHud = hidesHud;
        }

        public static GameplayUIPolicy For(GameplayUIId id)
        {
            switch (id)
            {
                case GameplayUIId.GameplayHUD:
                    return new GameplayUIPolicy(GameplayUILayer.Hud, false, false);
                case GameplayUIId.EffectRoll:
                    return new GameplayUIPolicy(GameplayUILayer.Screen, true, false, true);
                case GameplayUIId.PlayerStatus:
                    return new GameplayUIPolicy(GameplayUILayer.Screen, true, true);
                case GameplayUIId.Settings:
                    return new GameplayUIPolicy(GameplayUILayer.Modal, true, true);
                default:
                    throw new ArgumentOutOfRangeException(nameof(id), id, "UI 未定义默认策略");
            }
        }
        /// <summary>
        /// 检查可能的逻辑矛盾
        /// </summary>
        public void Validate()
        {
            if (!Enum.IsDefined(typeof(GameplayUILayer), _layer))
                throw new InvalidOperationException("UI 层级无效");
            if (_layer == GameplayUILayer.Hud && (_blocksGameplayInput || _hidesHud))
                throw new InvalidOperationException("HUD 不能阻塞 Gameplay 输入或隐藏自身层级");
            if (_layer == GameplayUILayer.Modal && !_blocksGameplayInput)
                throw new InvalidOperationException("Modal 必须阻塞 Gameplay 输入");
        }
    }
}
