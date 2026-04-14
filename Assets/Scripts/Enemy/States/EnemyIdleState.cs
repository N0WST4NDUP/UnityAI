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
        _enemy.Renderer.material.color = Color.blue;
        _timer = 0f;
    }

    public void OnExit() { }

    public void OnUpdate()
    {
        if (Vector3.Distance(_enemy.transform.position, _enemy.Target.position) < _enemy.DetectionRange)
        {
            _enemy.StateMachine.ChangeState(new EnemyChaseState(_enemy));
        }
        else
        {
            _timer += Time.deltaTime;
            if (_timer >= _enemy.IdleTime)
            {
                _enemy.StateMachine.ChangeState(new EnemyPatrolState(_enemy));
            }
        }
    }
}