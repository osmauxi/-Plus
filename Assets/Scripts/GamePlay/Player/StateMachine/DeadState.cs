using UnityEngine;

public class DeadState : State
{
    private float reviveProgress = 0f;

    public DeadState(StateMachine stateMachine, string animBoolName, PlayerController _player) : base(stateMachine, animBoolName, _player) { }

    public override void Enter()
    {
        base.Enter();
        reviveProgress = 0f;
        if (player.IsOwner)
        {
            if (GameStateController.instance.isSolo.Value)
            {
                PlayerManager.Instance.CheckTeamWipeServerRpc();
            }
            else
            {
                player.reviveUI.ShowUI();
                PlayerManager.Instance.CheckTeamWipeServerRpc();
            }
        }
    }

    public override void Update()
    {
        base.Update();

        if (!player.IsOwner || GameStateController.instance.isSolo.Value) return;

        // 逻辑变得极其简单：只看网络桥梁变量
        if (player.isBeingRevived.Value)
        {
            // 有人在按 F 救我！进度暴涨！
            reviveProgress += Time.deltaTime;

            if (reviveProgress >= player.maxReviveTime)
            {
                player.RequestRevive();
            }
        }
        else
        {
            // 没人救我（或中途打断），进度缓慢掉落
            reviveProgress = Mathf.Max(0, reviveProgress - Time.deltaTime * 0.5f);
        }

        // 把进度和是否涌动的高级表现全部甩给 UI 组件
        if (player.reviveUI != null)
        {
            player.reviveUI.UpdateProgress(reviveProgress / player.maxReviveTime, player.isBeingRevived.Value);
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
        if (player.IsOwner)
        {
            player.reviveUI.HideUI();
            player.SetRevivingServerRpc(false); // 兜底：防止复活瞬间卡死变量
        }
    }
}