using UnityEngine;

public class UnitAttack : MonoBehaviour
{
    [SerializeField] private UnitStatsSO _stats;
    [SerializeField] private LayerMask _hitMask;

    private Unit _unit;
    private float _cooldownTimer;
    private bool _isActive = false;

    public void Init(Unit unit)
    {
        _unit = unit;
        _unit.PhaseManager.OnBattleStart += Enable;
        _unit.PhaseManager.OnBattleEnd += Disable;
    }

    private void OnDestroy()
    {
        _unit.PhaseManager.OnBattleStart -= Enable;
        _unit.PhaseManager.OnBattleEnd -= Disable;
    }

    private void Update()
    {
        if (!_isActive) return;

        _cooldownTimer -= Time.deltaTime;
        if (_cooldownTimer > 0) return;

        var validTargets = Physics.OverlapSphere(
            _unit.transform.position,
            _stats.Range,
            _hitMask);
        _stats.ExecuteAttack(_unit.GroupId, transform, validTargets);
        _cooldownTimer = _stats.Cooldown;
    }

    private void Enable()
    {
        _isActive = true;
    }
    private void Disable()
    {
        _isActive = false;
    }
}