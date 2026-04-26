using UnityEngine;

public class BattleTestInput : MonoBehaviour
{
    [SerializeField] private PhaseManager _phaseManager;
    private Camera _camera;

    private void Awake()
    {
        _camera = Camera.main;
    }

    private void Update()
    {
        if (_phaseManager.Current != Phase.Battle) return;
        if (!Input.GetMouseButtonDown(1)) return;

        var ray = _camera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;

        var movements = FindObjectsByType<UnitMovement>(FindObjectsSortMode.None);
        foreach (var m in movements)
        {
            if (m.GetComponent<Unit>().GroupId != 0) continue;

            m.MoveTo(hit.point);
            Debug.Log($"[{GetType().Name}] OnClicked to {hit.point}");
        }
    }
}