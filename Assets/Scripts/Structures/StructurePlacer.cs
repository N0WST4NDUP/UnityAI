using UnityEngine;

public class StructurePlacer : MonoBehaviour
{
    // --- Inspector ---
    [SerializeField] private PhaseManager _phaseManager;
    [SerializeField] private PlacementController _placementController;
    [SerializeField] private GridField _grid;
    [SerializeField] private GameObject _structurePrefab;

    // --- Internal ---
    private Camera _camera;

    private void Awake()
    {
        _camera = Camera.main;
    }

    private void Update()
    {
        if (_phaseManager.Current != Phase.Preparation) return;
        if (_placementController.Mode != PlacementMode.Structure) return;
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
        var worldPos = _grid.GridToWorld(gridPos);
        var structure = Instantiate(
            _structurePrefab,
            worldPos,
            Quaternion.identity);

        int groupId = _placementController.ActiveGroupId;
        structure.GetComponent<Structure>().Init(groupId, worldPos, _phaseManager);
    }
}
