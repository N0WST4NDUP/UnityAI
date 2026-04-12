using UnityEngine;

public class ActiveState : IState
{
    private StateMachine _stateMachine;
    private Renderer _renderer;
    private Color _color = new(1f, 0f, 0f);

    public ActiveState(StateMachine stateMachine, Renderer renderer)
    {
        _stateMachine = stateMachine;
        _renderer = renderer;
    }

    public void OnEnter()
    {
        _renderer.material.color = _color;
    }

    public void OnExit()
    {
    }

    public void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _stateMachine.ChangeState(new IdleState(_stateMachine, _renderer));
        }
    }
}