namespace ProjectGame.HotFix.Gameplay.CameraSystem
{
    public enum CameraShakeDirectionMode
    {
        Random = 0,

        /// <summary>把调用方提供的世界方向解释为射击方向，镜头向反方向产生后坐。</summary>
        Recoil,

        /// <summary>直接使用调用方提供的世界方向。</summary>
        WorldDirection
    }
}