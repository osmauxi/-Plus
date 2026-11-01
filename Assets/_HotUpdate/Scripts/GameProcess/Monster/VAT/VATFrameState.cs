namespace ProjectGame.HotFix.Gameplay.Monsters
{
    /// <summary>
    /// VAT 当前采样帧、目标采样帧以及两者之间的插值权重。
    /// </summary>
    public readonly struct VATFrameState
    {
        public readonly float CurrentFrame;
        public readonly float NextFrame;
        public readonly float Blend;

        public VATFrameState(float currentFrame, float nextFrame, float blend)
        {
            CurrentFrame = currentFrame;
            NextFrame = nextFrame;
            Blend = blend;
        }
    }
}
