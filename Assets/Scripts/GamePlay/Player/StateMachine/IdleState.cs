public class IdleState : State
{
    public IdleState(StateMachine stateMachine, string animBoolName, PlayerController _player) : base(stateMachine, animBoolName, _player) { }

    public override void Update()
    {
        base.Update();
        player.CollectInput();
        player.HandleAimingCalculation();

        // 检测到输入，请求切入移动状态
        if (player.CurrentMoveInput.sqrMagnitude > 0.01f)
        {
            player.RequestStateChange(PlayerStateType.Moving);
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        // Idle 也要保持移动和旋转的计算，保证刹车滑行手感不中断
        player.HandleMovement();
        player.ApplyRotation();
    }
}