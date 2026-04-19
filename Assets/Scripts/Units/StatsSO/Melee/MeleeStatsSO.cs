using UnityEngine;

[CreateAssetMenu(menuName = "Units/Stats/Melee")]
public class MeleeStatsSO : UnitStatsSO
{
    public override void ExecuteAttack(int groupId, Transform origin, Collider[] validTargets)
    {
        IDamageable nearest = null;
        float minDistance = float.MaxValue;

        foreach (var target in validTargets)
        {
            if (target.TryGetComponent(out IDamageable unit) &&
                target.TryGetComponent(out IGroupOwned group))
            {
                if (group.GroupId == groupId) continue;

                float dist = Vector3.Distance(origin.position, target.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    nearest = unit;
                }
            }
        }

        if (nearest != null)
        {
            nearest.OnDamaged(Damage);
            Debug.Log($"[{GetType().Name}] Attack successful. Damage: {Damage}");
        }
    }
}