using UnityEngine;

public class UnitHealth : MonoBehaviour, IDamageable
{
    // --- Inspector ---
    [SerializeField] private float _maxHealth = 100f;

    // --- Internal ---
    private float _currentHealth;

    // --- External ---
    private Unit _unit;
    public float MaxHealth => _maxHealth;
    public float CurrentHealth => _currentHealth;
    public bool IsDead => _currentHealth <= 0;

    public void Init(Unit unit)
    {
        _unit = unit;
        _currentHealth = _maxHealth;
    }

    public void OnDamaged(float damage)
    {
        if (damage <= 0) return;

        _currentHealth -= damage;
        if (_currentHealth <= 0) Die();
    }

    //
    private void Die()
    {
        gameObject.SetActive(false);
    }
}