using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

[RequireComponent(typeof(Unity.MLAgents.Policies.BehaviorParameters))]
public class CommanderAgent : Agent
{
    [Header("레퍼런스")]
    [SerializeField] private UnitManager _unitManager;
    [SerializeField] private GoalZone _goalZone;
    [SerializeField] private Collider _groundCollider;

    [Header("에피소드 설정")]
    [SerializeField] private float _episodeTimeLimit = 30f;

    private float _timer;

    // ───────────────────────────────────────────
    // ML-Agents 생명주기
    // ───────────────────────────────────────────

    public override void Initialize()
    {
        var bp = GetComponent<Unity.MLAgents.Policies.BehaviorParameters>();
        if (TrainManager.Instance != null)
            TrainManager.Instance.RegisterAgent(bp != null ? bp.BehaviorName : name, MaxStep);
    }

    public override void OnEpisodeBegin()
    {
        _timer = 0f;

        Bounds ground = _groundCollider.bounds;

        // 타겟 + 관성 초기화 (위치는 유지)
        _unitManager.ResetPhysicsAll();

        // GoalZone 랜덤 초기화
        int requiredCount = Random.Range(1, _unitManager.Capability + 1);
        float zoneSize = Random.Range(0.5f, 1.0f);
        float halfZone = zoneSize * 0.5f;

        float x = Mathf.Clamp(Random.Range(ground.min.x + halfZone, ground.max.x - halfZone),
                                ground.min.x + halfZone, ground.max.x - halfZone);
        float z = Mathf.Clamp(Random.Range(ground.min.z + halfZone, ground.max.z - halfZone),
                                ground.min.z + halfZone, ground.max.z - halfZone);
        Vector3 zonePos = new(x, ground.max.y + 0.05f, z);
        Vector3 zoneScale = new(zoneSize, 0.05f, zoneSize);

        _goalZone.Initialize(requiredCount, zonePos, zoneScale);
    }

    // 맵 대각선 길이 (정규화 기준) — Ground scale 5짜리 Plane = 50×50 월드
    private const float MaxDist = 72f; // sqrt(50^2 + 50^2) ≈ 70.7
    private const float MaxSpeed = 10f; // Unit._maxSpeed 상한

    public override void CollectObservations(VectorSensor sensor)
    {
        // 1. 목표 인원 수 (0~1)
        sensor.AddObservation((float)_goalZone.UnitsNeeded / _unitManager.Capability);

        // 2. 유닛별 (정규화 거리, 정규화 속도) — Capability 슬롯 고정, 없는 슬롯은 0 패딩
        var units = _unitManager.GetAllUnits();
        for (int i = 0; i < _unitManager.MaxCapability; i++)
        {
            if (i < units.Count)
            {
                float dist = Vector3.Distance(units[i].transform.position, _goalZone.Center);
                sensor.AddObservation(dist / MaxDist);
                sensor.AddObservation(units[i].MoveSpeed / MaxSpeed);
            }
            else
            {
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
            }
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        _timer += Time.fixedDeltaTime;

        int needed = _goalZone.UnitsNeeded;
        if (needed > 0)
        {
            // Branch 0: 몇 명에게 명령할지, UnitsNeeded 이하로 제한
            int commandCount = Mathf.Min(actions.DiscreteActions[0] + 1, needed);

            // GoalZone 밖 유닛 중 ETA 가장 짧은 n명에게만 명령
            var units = _unitManager.GetFastestToArrive(_goalZone.Center, commandCount, _goalZone);
            foreach (var unit in units)
                unit.SetTarget(_goalZone.transform);
        }

        // 매 스텝 보상: 정규화 (최대 0.001/step → 30초 1500step 기준 최대 +1.5)
        float stepReward = (float)_goalZone.UnitsInside / _unitManager.Capability * 0.001f;
        AddReward(stepReward);
        if (TrainManager.Instance != null)
            TrainManager.Instance.AddStep(stepReward);

        // 성공
        if (_goalZone.IsCompleted)
        {
            AddReward(1.0f);
            if (TrainManager.Instance != null)
                TrainManager.Instance.OnEpisodeEnd(GetCumulativeReward(), StepCount, success: true);
            EndEpisode();
            return;
        }

        // 시간 초과
        if (_timer >= _episodeTimeLimit)
        {
            AddReward(-0.5f);
            if (TrainManager.Instance != null)
                TrainManager.Instance.OnEpisodeEnd(GetCumulativeReward(), StepCount, success: false);
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // 테스트용: 항상 전체 유닛에게 명령
        actionsOut.DiscreteActions.Array[0] = _unitManager.Capability - 1;
    }
}
