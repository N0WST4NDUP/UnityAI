using UnityEngine;

public enum PlacementMode
{
    Unit,
    Structure
}

public class PlacementController : MonoBehaviour
{
    private PlacementMode _mode = PlacementMode.Unit;
    private int _activeGroupId = 0;

    public PlacementMode Mode => _mode;
    public int ActiveGroupId => _activeGroupId;

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

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            _activeGroupId = (_activeGroupId + 1) % 2;
            Debug.Log($"[{GetType().Name}] GroupId Changed: {_activeGroupId}");
        }
    }
}