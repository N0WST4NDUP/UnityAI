using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Unit : MonoBehaviour, IDamageable
{
    // --- Inspector ---
    [SerializeField] private Tribe _tribe;
    [SerializeField] private UnitType _type;
    [SerializeField] private float _maxHealth = 100f;

    // --- Internal ---
    private int _groupId;
    private float _currentHealth;
    private Vector2Int _startPosition;

    // --- Properties ---
    public int GroupId => _groupId;
    public Tribe Tribe => _tribe;
    public UnitType Type => _type;
    public float MaxHealth => _maxHealth;
    public float CurrentHealth => _currentHealth;
    public bool IsDead => _currentHealth <= 0;
    public Vector2Int StartPosition => _startPosition;

    private void OnEnable()
    {
        _currentHealth = _maxHealth;
    }

    public void Init(Vector2Int gridPosition, int groupId)
    {
        _startPosition = gridPosition;
        _groupId = groupId;
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