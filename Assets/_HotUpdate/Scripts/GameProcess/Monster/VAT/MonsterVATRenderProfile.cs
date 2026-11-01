using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    /// <summary>
    /// 描述了怪物的VAT渲染配置文件，包括材质、缩放、阴影设置等信息
    /// 信息对应一种怪物的渲染配置，所有怪物实例共享同一个配置文件
    /// </summary>
    public readonly struct MonsterVATRenderProfile
    {
        public readonly MonsterVATAsset Asset;
        //挂载了VATShader以及对应顶点法线纹理的材质
        public readonly Material Material;
        //原始怪物挂载材质，用于从旧的普通蒙皮材质拷贝基础纹理和颜色到VAT材质上
        public readonly Material SourceMaterial;
        //绘制时的缩放，通常是怪物模型的缩放
        public readonly Vector3 RootScale;
        //Renderer的世界坐标系到根节点的局部坐标系的变换矩阵
        //用于将Renderer的顶点位置转换到根节点的局部坐标系
        public readonly Matrix4x4 RendererLocalToRoot;
        //顶点动画的淡入淡出时间，单位秒
        public readonly float CrossFadeDuration;
        //怪物移动阈值，用于进行移动判断
        public readonly float MovementThreshold;
        public readonly float HitFlashDuration;
        public readonly Color HitFlashColor;
        //阴影投射模式
        public readonly ShadowCastingMode ShadowCastingMode;
        //怪物是否接受场景光照的阴影投射
        public readonly bool ReceiveShadows;
        public readonly int Layer;

        public MonsterVATRenderProfile(
            MonsterVATAsset asset,
            Material material,
            Material sourceMaterial,
            Vector3 rootScale,
            Matrix4x4 rendererLocalToRoot,
            float crossFadeDuration,
            float movementThreshold,
            float hitFlashDuration,
            Color hitFlashColor,
            ShadowCastingMode shadowCastingMode,
            bool receiveShadows,
            int layer)
        {
            Asset = asset;
            Material = material;
            SourceMaterial = sourceMaterial;
            RootScale = rootScale;
            RendererLocalToRoot = rendererLocalToRoot;
            CrossFadeDuration = crossFadeDuration;
            MovementThreshold = movementThreshold;
            HitFlashDuration = hitFlashDuration;
            HitFlashColor = hitFlashColor;
            ShadowCastingMode = shadowCastingMode;
            ReceiveShadows = receiveShadows;
            Layer = layer;
        }
    }
}