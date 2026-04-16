using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshObstacle))]
public class Structure : MonoBehaviour
{
    // --- Inspector ---
    [SerializeField] private int _maxDurability = 3;

    // --- Internal ---
    private int _currDurability;
    private Vector2Int _gridPosition;

    // --- Properties ---
    public int MaxDurability => _maxDurability;
    public int CurrentDurability => _currDurability;
    public Vector2Int GridPosition => _gridPosition;

    public void Init(Vector2Int gridPosition)
    {
        _currDurability = _maxDurability;
        _gridPosition = gridPosition;
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;

        _currDurability -= amount;
        if (_currDurability <= 0) Die();
    }

    private void Die()
    {
        gameObject.SetActive(false);
    }
}
