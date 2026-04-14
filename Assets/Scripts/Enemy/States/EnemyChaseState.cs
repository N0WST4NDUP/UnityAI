using UnityEngine;

public class EnemyChaseState : IState
{
    private EnemyAgent _enemy;

    public EnemyChaseState(EnemyAgent enemy)
    {
        _enemy = enemy;
    }

    public void OnEnter()
    {
        _enemy.Agent.isStopped = false;
        _enemy.Renderer.material.color = Color.yellow;
    }

    public void OnExit() { }

    public void OnUpdate()
    {
        _enemy.Agent.SetDestination(_enemy.Target.position);

        var distance = Vector3.Distance(_enemy.transform.position, _enemy.Target.position);
        if (distance > _enemy.DetectionRange)
        {
            _enemy.StateMachine.ChangeState(new EnemyIdleState(_enemy));
        }
        else if (distance <= _enemy.AttackRange)
        {
            _enemy.StateMachine.ChangeState(new EnemyAttackState(_enemy));
        }
    }
}