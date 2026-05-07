using UnityEngine;

public class DeadState : State
{
    private float realReviveProgress = 0f;

    public DeadState(StateMachine stateMachine, string animBoolName, PlayerController _player) : base(stateMachine, animBoolName, _player) { }

    public override void Enter()
    {
        base.Enter();
        realReviveProgress = 0f;

        if (!GameStateController.instance.isSolo.Value)
        {
            player.reviveUI.ShowUI();
        }

        // 只有死者自己负责向服务器汇报团灭
        if (player.IsOwner)
        {
            PlayerManager.Instance.CheckTeamWipeServerRpc();
        }
    }

    public override void Update()
    {
        base.Update();

        if (!player.IsOwner || GameStateController.instance.isSolo.Value) return;

        if (player.isBeingRevived.Value)
        {
            realReviveProgress += Time.deltaTime;
            if (realReviveProgress >= player.maxReviveTime)
            {
                player.RequestRevive();
            }
        }
        else
        {
            realReviveProgress = Mathf.Max(0, realReviveProgress - Time.deltaTime * 0.5f);
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        player.ExecuteStop();
    }

    public override void Exit()
    {
        base.Exit();
        player.reviveUI.HideUI();
        if (player.IsOwner)
        {
            player.SetRevivingServerRpc(false);
        }
    }
}