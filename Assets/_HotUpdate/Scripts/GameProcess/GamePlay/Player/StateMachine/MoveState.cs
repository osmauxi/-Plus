public class MoveState : State
{
    public MoveState(StateMachine stateMachine, string animBoolName, PlayerController _player) : base(stateMachine, animBoolName, _player) { }

    public override void Update()
    {
        base.Update();
        player.CollectInput();
        player.HandleAimingCalculation();

        // 输入归零，请求切回站立
        if (player.CurrentMoveInput.sqrMagnitude < 0.01f)
        {
            player.RequestStateChange(PlayerStateType.Idle);
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        player.HandleMovement();
        player.ApplyRotation();
    }
}