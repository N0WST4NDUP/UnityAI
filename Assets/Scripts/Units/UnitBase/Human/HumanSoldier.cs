using UnityEngine;

[RequireComponent(typeof(UnitHealth))]
[RequireComponent(typeof(UnitMovement))]
[RequireComponent(typeof(UnitAttack))]
[RequireComponent(typeof(UnitAnimator))]
public class HumanSoldier : Unit
{
    private UnitHealth _health;
    private UnitMovement _movement;
    private UnitAttack _attack;
    private UnitAnimator _animator;

    protected override void Awake()
    {
        base.Awake();

        _health = GetComponent<UnitHealth>();
        _movement = GetComponent<UnitMovement>();
        _attack = GetComponent<UnitAttack>();
        _animator = GetComponent<UnitAnimator>();
    }

    private void Start()
    {
        _health.Init(this);
        _movement.Init(this);
        _attack.Init(this);
    }

    private void Update()
    {
        _animator?.PlayMoveAnimation(_movement.GetNormalizedSpeed());
    }
}