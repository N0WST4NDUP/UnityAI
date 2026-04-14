using UnityEngine;

public class EnemyDeadState : IState
{
    private EnemyAgent _enemy;

    public EnemyDeadState(EnemyAgent enemy)
    {
        _enemy = enemy;
    }

    public void OnEnter()
    {
        _enemy.Agent.isStopped = true;
        _enemy.Renderer.material.color = Color.gray;
        _enemy.GetComponent<Collider>().enabled = false;
        _enemy.enabled = false;
    }

    public void OnExit() { }

    public void OnUpdate() { }
}