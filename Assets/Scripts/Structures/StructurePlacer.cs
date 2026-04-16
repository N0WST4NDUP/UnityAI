using UnityAI.Grid;
using UnityEngine;

namespace UnityAI.Structures
{
    public class StructurePlacer : MonoBehaviour
    {
        // --- Inspector ---
        [SerializeField] private GridField _grid;
        [SerializeField] private GameObject _structurePrefab;
        [SerializeField] private bool _isPreparationPhase = true;

        // --- Internal ---
        private Camera _camera;

        private void Awake()
        {
            _camera = Camera.main;
        }

        private void Update()
        {
            if (!_isPreparationPhase) return;
            if (!Input.GetMouseButtonDown(0)) return;

            TryPlaceStructure();
        }

        private void TryPlaceStructure()
        {
            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit)) return;

            var gridPos = _grid.WorldToGrid(hit.point);
            if (_grid.GetCellState(gridPos) != CellState.Empty) return;

            _grid.SetCellState(gridPos, CellState.Structure);
            var structure = Instantiate(_structurePrefab, _grid.GridToWorld(gridPos), Quaternion.identity);
            structure.GetComponent<Structure>().Init(_grid, gridPos);
        }
    }
}