using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class UnitMovement : MonoBehaviour, IMovable
{
    // --- Internal ---
    private Unit _unit;
    private NavMeshAgent _agent;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    public void Init(Unit unit)
    {
        _unit = unit;
        _unit.PhaseManager.OnBattleStart += EnableMovement;
        _unit.PhaseManager.OnBattleEnd += DisableMovement;
    }

    private void OnDestroy()
    {
        _unit.PhaseManager.OnBattleStart -= EnableMovement;
        _unit.PhaseManager.OnBattleEnd -= DisableMovement;
    }

    public void MoveTo(Vector3 position)
    {
        if (!_agent.enabled) return;

        _agent.SetDestination(position);
    }

    public float GetNormalizedSpeed()
    {
        return _agent.velocity.magnitude / _agent.speed;
    }

    private void EnableMovement()
    {
        _agent.enabled = true;
        _agent.ResetPath();
    }

    private void DisableMovement()
    {
        _agent.enabled = false;
    }
}