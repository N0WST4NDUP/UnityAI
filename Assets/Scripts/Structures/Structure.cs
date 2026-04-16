using UnityAI.Grid;
using UnityEngine;

namespace UnityAI.Structures
{
    public class Structure : MonoBehaviour
    {
        // --- Inspector ---
        [SerializeField] private int _maxDurability = 3;

        // --- Internal ---
        private int _currDurability;
        private GridField _gridField;
        private Vector2Int _gridPosition;

        // --- Properties ---
        public int MaxDurability => _maxDurability;
        public int CurrentDurability => _currDurability;
        public Vector2Int GridPosition => _gridPosition;

        public void Init(GridField gridField, Vector2Int gridPosition)
        {
            _currDurability = _maxDurability;
            _gridField = gridField;
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
            _gridField.SetCellState(_gridPosition.x, _gridPosition.y, CellState.Empty);
            Destroy(gameObject);
        }
    }
}