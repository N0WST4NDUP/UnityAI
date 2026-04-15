using UnityEngine;

namespace UnityAI.Grid
{
    public class GridField : MonoBehaviour
    {
        // --- Inspector ---
        [SerializeField] private int _width = 16;
        [SerializeField] private int _height = 16;
        [SerializeField] private float _cellSize = 1.0f;

        // --- Internal ---
        private CellState[,] _cells;

        // --- Properties ---
        public int Width => _width;
        public int Height => _height;
        public float CellSize => _cellSize;

        // --- Unity Lifecycle ---
        private void Awake()
        {
            _cells = new CellState[_width, _height];
        }

        // --- Cell State API ---
        public CellState GetCellState(int x, int y)
        {
            if (x < 0 || x >= _width || y < 0 || y >= _height)
                return CellState.Structure;

            return _cells[x, y];
        }

        public void SetCellState(int x, int y, CellState state)
        {
            if (x < 0 || x >= _width || y < 0 || y >= _height)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"SetCellState out of range: ({x},{y})");
#endif
                return;
            }

            _cells[x, y] = state;
        }

        // --- Coordinate Conversion ---
        public Vector2Int WorldToGrid(Vector3 world)
        {
            int gx = Mathf.FloorToInt(world.x / _cellSize);
            int gy = Mathf.FloorToInt(world.z / _cellSize);

            return new(gx, gy);
        }

        public Vector3 GridToWorld(Vector2Int grid)
        {
            float wx = (grid.x + 0.5f) * _cellSize;
            float wz = (grid.y + 0.5f) * _cellSize;

            return new(wx, 0f, wz);
        }

        #region Gizmos
        private void OnDrawGizmos()
        {
            if (_cells == null)
            {
                DrawEmptyFrame();
                return;
            }

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    DrawCell(x, y, _cells[x, y]);
                }
            }
        }

        private void DrawEmptyFrame()
        {
            Gizmos.color = new(0.5f, 0.5f, 0.5f, 1f);
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    var center = GridToWorld(new(x, y));
                    var size = new Vector3(_cellSize, 0.05f, _cellSize);
                    Gizmos.DrawWireCube(center, size);
                }
            }
        }

        private void DrawCell(int x, int y, CellState state)
        {
            var center = GridToWorld(new(x, y));
            var size = new Vector3(_cellSize, 0.05f, _cellSize);

            switch (state)
            {
                case CellState.Empty:
                    Gizmos.color = new(0.5f, 0.5f, 0.5f, 1f);
                    Gizmos.DrawWireCube(center, size);
                    break;

                case CellState.Structure:
                    Gizmos.color = new(0.55f, 0.27f, 0.07f);
                    Gizmos.DrawCube(center, size);
                    break;

                case CellState.Occupied:
                    Gizmos.color = Color.blue;
                    Gizmos.DrawCube(center, size);
                    break;

                case CellState.Danger:
                    Gizmos.color = Color.red;
                    Gizmos.DrawCube(center, size);
                    break;
            }
        }
        #endregion
    }
}