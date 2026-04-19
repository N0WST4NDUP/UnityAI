using UnityEngine;

public enum PlacementMode
{
    Unit,
    Structure
}

public class PlacementController : MonoBehaviour
{
    private PlacementMode _mode = PlacementMode.Unit;

    public PlacementMode Mode => _mode;

    // TODO: Phase 6에서 UI 토글로 교체
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            _mode = PlacementMode.Unit;
            Debug.Log($"[{GetType().Name}] Mode: {_mode}");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            _mode = PlacementMode.Structure;
            Debug.Log($"[{GetType().Name}] Mode: {_mode}");
        }
    }
}