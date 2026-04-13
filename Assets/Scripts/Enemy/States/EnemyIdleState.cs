using UnityEngine;

public class EnemyIdleState : IState
{
    private EnemyAgent _enemy;
    private float _timer;

    public EnemyIdleState(EnemyAgent enemy)
    {
        _enemy = enemy;
    }

    public void OnEnter()
    {
        _enemy.Agent.isStopped = true;
        _timer = 0f;
    }

    public void OnExit() { }

    public void OnUpdate()
    {
        if (Vector3.Distance(_enemy.transform.position, _enemy.Target.position) < _enemy.DetectionRange)
        {
            // TODO: _enemy.StateMachine.ChangeState(new ChaseState);
        }
        else
        {
            _timer += Time.deltaTime;
            if (_timer >= _enemy.IdleTime)
            {
                // TODO: _enemy.StateMachine.ChangeState(new PatrolState);
            }
        }
    }
}