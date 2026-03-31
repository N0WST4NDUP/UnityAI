using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class BallControlAgent : Agent
{
    public float Speed = 1.2f;
    public float SuccessTime = 10f; // 이 시간(초) 버티면 성공
    public GameObject Target;

    private Rigidbody _rb;
    private Rigidbody _ballRb;
    private float _timer;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _ballRb = Target.GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        _timer = 0f;

        // 에이전트 초기화
        _rb.angularVelocity = Vector3.zero;
        _rb.linearVelocity = Vector3.zero;
        this.transform.localRotation = Quaternion.identity;

        // 공 초기화: 머리 위 랜덤 위치에 생성
        _ballRb.angularVelocity = Vector3.zero;
        _ballRb.linearVelocity = Vector3.zero;
        Target.transform.localPosition = new Vector3(
            Random.Range(-0.5f, 0.5f),
            4f,
            Random.Range(-0.5f, 0.5f)
        );
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(Target.transform.localPosition); // 3
        sensor.AddObservation(_ballRb.linearVelocity);         // 3
        sensor.AddObservation(_rb.rotation.x);                 // 1
        sensor.AddObservation(_rb.rotation.z);                 // 1
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // 행동 적용 (Continuous: [0]=앞뒤, [1]=좌우)
        float upDown = actions.ContinuousActions[0];
        float leftRight = actions.ContinuousActions[1];
        var rotate = Quaternion.Euler(upDown * Speed, 0f, -leftRight * Speed);
        _rb.MoveRotation(_rb.rotation * rotate);

        // 공이 머리 위에 유지되면 매 스텝 보상
        if (Target.transform.localPosition.y > 0f)
        {
            _timer += Time.fixedDeltaTime;
            AddReward(0.1f);

            // 목표 시간 버티면 최고 보상 후 에피소드 종료
            if (_timer >= SuccessTime)
            {
                SetReward(1f);
                EndEpisode();
            }
        }
        else
        {
            // 공이 떨어지면 패널티 후 에피소드 종료
            SetReward(-1f);
            EndEpisode();
        }
    }

    // 수동 테스트용 (Behavior Type: Heuristic Only)
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var actions = actionsOut.ContinuousActions;
        actions[0] = Input.GetAxis("Vertical");
        actions[1] = Input.GetAxis("Horizontal");
    }
}
