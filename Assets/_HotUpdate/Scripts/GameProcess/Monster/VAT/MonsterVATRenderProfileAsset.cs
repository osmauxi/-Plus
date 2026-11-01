using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    /// <summary>
    /// 保持Prefab和renderer的静态表现配置，切掉原来的运行时获取，方便在运行时直接使用配置数据
    /// 目的为彻底干掉怪物GameObject
    /// </summary>
    [CreateAssetMenu(menuName = "Monster/VAT Render Profile")]
    public sealed class MonsterVATRenderProfileAsset : ScriptableObject
    {
        public MonsterVATAsset Asset;
        public Material Material;
        public Material SourceMaterial;

        public Vector3 RootScale = Vector3.one;
        public Matrix4x4 RendererLocalToRoot = Matrix4x4.identity;

        public float CrossFadeDuration = 0.12f;
        public float MovementThreshold = 0.05f;
        public float HitFlashDuration = 0.08f;
        public Color HitFlashColor = Color.white;

        public ShadowCastingMode ShadowCastingMode = ShadowCastingMode.On;
        public bool ReceiveShadows = true;
        public int Layer;

        public MonsterVATRenderProfile ToRuntimeProfile()
        {
            return new MonsterVATRenderProfile(
                Asset,
                Material,
                SourceMaterial,
                RootScale,
                RendererLocalToRoot,
                CrossFadeDuration,
                MovementThreshold,
                HitFlashDuration,
                HitFlashColor,
                ShadowCastingMode,
                ReceiveShadows,
                Layer);
        }
    }
}