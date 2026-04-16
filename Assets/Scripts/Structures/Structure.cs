using UnityAI.Grid;
using UnityEngine;
using UnityEngine.AI;

namespace UnityAI.Structures
{
    [RequireComponent(typeof(NavMeshObstacle))]
    public class Structure : MonoBehaviour
    {
        // --- Inspector ---
        [SerializeField] private int _maxDurability = 3;

        // --- Internal ---
        private NavMeshObstacle _obstacle;
        private int _currDurability;
        private Vector2Int _gridPosition;

        // --- Properties ---
        public int MaxDurability => _maxDurability;
        public int CurrentDurability => _currDurability;
        public Vector2Int GridPosition => _gridPosition;

        // --- Unity Lifecycle ---
        private void Awake()
        {
            _obstacle = GetComponent<NavMeshObstacle>();
        }

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
}