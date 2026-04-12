using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    [Header("Unit 관련")]
    public GameObject UnitPrefab;
    public GameObject Group;

    [Header("생성 관련")]
    [SerializeField][Range(1, 10)] private int _capability = 5;
    public int Capability => _capability;
    public int MaxCapability => 10;

    [Header("스폰 설정")]
    [SerializeField] private Collider _groundCollider;

    private readonly List<Unit> _units = new();

    private float GetSpawnY()
    {
        if (_groundCollider != null) return _groundCollider.bounds.max.y;
        return 0f;
    }

    // ETA 기준 정렬, GoalZone 안에 이미 있는 유닛은 제외
    public List<Unit> GetFastestToArrive(Vector3 destination, int count, GoalZone zone)
    {
        var candidates = new List<Unit>();
        foreach (var u in _units)
        {
            if (u != null && !zone.IsUnitInside(u))
                candidates.Add(u);
        }
        candidates.Sort((a, b) => GetETA(a, destination).CompareTo(GetETA(b, destination)));
        return candidates.GetRange(0, Mathf.Min(count, candidates.Count));
    }

    // 에이전트 관찰용: 전체 유닛 리스트 반환
    public List<Unit> GetAllUnits() => new(_units);

    // 에피소드 시작마다 타겟 + 관성만 초기화 (위치는 유지)
    public void ResetPhysicsAll()
    {
        foreach (var unit in _units)
        {
            if (unit != null) unit.ResetPhysics();
        }
    }

    private static float GetETA(Unit unit, Vector3 destination)
    {
        float dist = Vector3.Distance(unit.transform.position, destination);
        float speed = Mathf.Max(unit.MoveSpeed, 0.01f);
        return dist / speed;
    }

    private void Update()
    {
        if (_units.Count < _capability)
        {
            float spawnY = GetSpawnY();
            Vector3 spawnPos = new(
                transform.position.x + Random.Range(-2f, 2f),
                spawnY,
                transform.position.z + Random.Range(-2f, 2f));

            var go = Instantiate(UnitPrefab, spawnPos, Quaternion.identity, Group.transform);
            var unit = go.GetComponent<Unit>();
            go.GetComponent<Rigidbody>().AddForce(
                new(Random.Range(-0.1f, 0.1f), 0f, Random.Range(-0.1f, 0.1f)),
                ForceMode.Impulse);

            if (unit != null) _units.Add(unit);
        }
        else if (_units.Count > _capability)
        {
            Destroy(_units[0].gameObject);
            _units.RemoveAt(0);
        }
    }
}
