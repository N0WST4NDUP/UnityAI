using UnityEngine;

public abstract class UnitStatsSO : ScriptableObject
{
    public float Damage;
    public float Range;
    public float Cooldown;

    public abstract void ExecuteAttack(int groupId, Transform origin, Collider[] validTargets);
}