using UnityEngine;

public class StateMachine
{
    private IState _currentState;

    public void Tick()
    {
        _currentState?.OnUpdate();
    }

    public void ChangeState(IState newState)
    {
        _currentState?.OnExit();

        _currentState = newState;
        _currentState.OnEnter();
    }
}