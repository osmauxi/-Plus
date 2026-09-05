using System;

public class StateMachine//PlayerStateMachine负责管理所有的状态的切换
{
    public Action OnValueChanged;
    private State _currentState;
    public State CurrentState
    {
        get => _currentState;
        set
        {
            _currentState = value;
            OnValueChanged?.Invoke();
        }
    }//意为一个值在访问它的时候时public状态，想改变它时时private状态，即只读状态

    public void Initialize(State _startState)//Initialize为构造函数名
    {
        CurrentState = _startState;
        CurrentState.Enter();//enter是playerstate中的enter函数，下同
    }

    public void ChangeState(State _newState)
    {//退出现在的状态，改变现在的状态，进入新的状态
        CurrentState.Exit();
        CurrentState = _newState;
        CurrentState.Enter();
    }
}
