using UnityEngine;
using UnityEngine.AI;

public class EnemyPatrolState : IState
{
    private const float k_ARRIVAL_THRESHOLD = 0.5f;
    private EnemyAgent _enemy;

    public EnemyPatrolState(EnemyAgent enemy)
    {
        _enemy = enemy;
    }

    public void OnEnter()
    {
        _enemy.Agent.isStopped = false;
        _enemy.Renderer.material.color = Color.green;
        _enemy.Agent.SetDestination(_enemy.CurrentWaypoint.position);
    }

    public void OnExit() { }

    public void OnUpdate()
    {
        if (Vector3.Distance(_enemy.transform.position, _enemy.Target.position) < _enemy.DetectionRange)
        {
            _enemy.StateMachine.ChangeState(new EnemyChaseState(_enemy));
        }
        else if (!_enemy.Agent.pathPending && _enemy.Agent.remainingDistance < k_ARRIVAL_THRESHOLD)
        {
            _enemy.NextWaypoint();
            _enemy.StateMachine.ChangeState(new EnemyIdleState(_enemy));
        }

    }
}