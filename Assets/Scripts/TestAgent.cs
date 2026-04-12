using UnityEngine;

public class TestAgent : MonoBehaviour
{
    private StateMachine _stateMachine = new();

    private void Awake()
    {
        IState state = new IdleState(
            _stateMachine,
            GetComponent<Renderer>()
            );

        _stateMachine.ChangeState(state);
    }

    private void Update()
    {
        _stateMachine.Tick();
    }
}