public enum PlayerStateType 
{
    Idle,
    Moving,
    dead
}

public class State
{

    protected StateMachine stateMachine;
    protected bool triggerCalled;
    protected string animBoolName;
    protected PlayerController player;

    public State(StateMachine stateMachine, string animBoolName, PlayerController _player)
    //作为所有状态的父类，这个状态机是基于SetBool来的，所有状态都由Animator中的布尔值来进行转换，状态需要指定状态机，状态对象和Animator中对应的布尔值
    {
        this.stateMachine = stateMachine;
        this.animBoolName = animBoolName;
        player = _player;
    }

    public virtual void Update()
    {

    }
    public virtual void FixedUpdate() 
    { 
    
    }
    //进入状态是改变对应的bool为ture，Animator中进入此状态，退出后设为false，Animator进入Exit，这样就不用在Animator里拉蜘蛛网
    public virtual void Enter()
    {
        player.Anim.SetBool(animBoolName, true);
    }

    public virtual void Exit()
    {
        player.Anim.SetBool(animBoolName, false);
    }

}
