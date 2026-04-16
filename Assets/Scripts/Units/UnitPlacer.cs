using UnityEngine;

public class UnitPlacer : MonoBehaviour
{
    // --- Inspector ---
    [SerializeField] private GridField _grid;
    [SerializeField] private GameObject _unitPrefab;
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

        TryPlaceUnit(0); // TODO: Phase 6에서 채움
    }

    private void TryPlaceUnit(int groupId)
    {
        var ray = _camera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;

        var gridPos = _grid.WorldToGrid(hit.point);
        if (_grid.GetCellState(gridPos) != CellState.Empty) return;

        _grid.SetCellState(gridPos, CellState.Occupied);
        var unit = Instantiate(_unitPrefab, _grid.GridToWorld(gridPos), Quaternion.identity);
        unit.GetComponent<Unit>().Init(gridPos, groupId);
    }
}
