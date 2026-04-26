using UnityEngine;

public class UnitPlacer : MonoBehaviour
{
    // --- Inspector ---
    [SerializeField] private PhaseManager _phaseManager;
    [SerializeField] private PlacementController _placementController;
    [SerializeField] private GridField _grid;
    [SerializeField] private TeamRegistry _teamRegistry;
    [SerializeField] private GameObject _unitPrefab;

    // --- Internal ---
    private Camera _camera;

    private void Awake()
    {
        _camera = Camera.main;
    }

    private void Update()
    {
        if (_phaseManager.Current != Phase.Preparation) return;
        if (_placementController.Mode != PlacementMode.Unit) return;
        if (!Input.GetMouseButtonDown(0)) return;

        TryPlaceUnit();
    }

    private void TryPlaceUnit()
    {
        var ray = _camera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;

        var gridPos = _grid.WorldToGrid(hit.point);
        if (_grid.GetCellState(gridPos) != CellState.Empty) return;

        _grid.SetCellState(gridPos, CellState.Occupied);
        var worldPos = _grid.GridToWorld(gridPos);
        var unit = Instantiate(
            _unitPrefab,
            worldPos,
            Quaternion.identity);

        int groupId = _placementController.ActiveGroupId;
        Team team = _teamRegistry[groupId];
        var soldier = unit.GetComponent<Unit>();
        soldier.Init(groupId, worldPos, _phaseManager);
        team.AddUnit(soldier);
    }
}
