using UnityEngine;

namespace UnityAI.Grid
{
    public class GridTester : MonoBehaviour
    {
        [SerializeField] private GridField _grid;

        private void Start()
        {
            if (_grid == null)
            {
                Debug.LogError("GridTester: _grid is not assigned.");
                return;
            }

            // 테스트 1: 특정 칸을 Structure 로 (갈색 큐브)
            _grid.SetCellState(3, 3, CellState.Structure);
            _grid.SetCellState(4, 3, CellState.Structure);
            _grid.SetCellState(5, 3, CellState.Structure);

            // 테스트 2: 유닛 점유 1개 (파랑)
            _grid.SetCellState(8, 8, CellState.Occupied);

            // 테스트 3: 위험 셀 2개 (빨강)
            _grid.SetCellState(10, 5, CellState.Danger);
            _grid.SetCellState(10, 6, CellState.Danger);

            // 테스트 4: 좌표 변환 왕복 확인
            Vector3 world = _grid.GridToWorld(new Vector2Int(5, 7));
            Vector2Int back = _grid.WorldToGrid(world);
            Debug.Log($"GridToWorld(5,7) = {world}, WorldToGrid(그거) = {back}");

            // 테스트 5: 경계 밖 질의 → Structure 반환 확인
            CellState oob = _grid.GetCellState(-1, 0);
            Debug.Log($"GetCellState(-1, 0) = {oob} (Structure 여야 함)");
        }
    }
}
