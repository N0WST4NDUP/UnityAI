using UnityEngine;

public class EnemyAttackState : IState
{
    private EnemyAgent _enemy;
    private float _timer;

    public EnemyAttackState(EnemyAgent enemy)
    {
        _enemy = enemy;
    }

    public void OnEnter()
    {
        _enemy.Agent.isStopped = true;
        _enemy.Renderer.material.color = Color.red;
        _timer = 0;
    }

    public void OnExit() { }

    public void OnUpdate()
    {
        var target = _enemy.Target.position;
        var direction = (target - _enemy.transform.position).normalized;
        _enemy.transform.rotation = Quaternion.LookRotation(direction);

        var distance = Vector3.Distance(_enemy.transform.position, target);
        if (distance > _enemy.AttackRange)
        {
            _enemy.StateMachine.ChangeState(new EnemyChaseState(_enemy));
        }
        else
        {
            _timer += Time.deltaTime;
            if (_timer >= _enemy.AttackCooldown)
            {
                Debug.Log("Attack!!!");
                _timer = 0f;
            }
        }
    }
}