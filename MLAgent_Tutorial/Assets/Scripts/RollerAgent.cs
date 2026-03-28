using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class RollerAgent : Agent
{
    Rigidbody rb;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public GameObject Target;
    public override void OnEpisodeBegin()
    { // 에피소드 시작시
        // 만약 에이전트가 떨어졌다면, 기본 위치로 세팅
        if (this.transform.localPosition.y < 0)
        {
            rb.angularVelocity = Vector3.zero;
            rb.linearVelocity = Vector3.zero;
            this.transform.localPosition = new(0f, 0.5f, 0f);
        }

        // 타겟은 매번 새로운 위치에 생성
        Target.transform.localPosition =
            new(Random.value * 8 - 4, 0.5f, Random.value * 8 - 4);
    }

    public override void CollectObservations(VectorSensor sensor)
    { // State 정의하는 부분인듯? ML-Agents에선 환경 관찰로 명시.
        // 타겟과 에이전트의 위치
        sensor.AddObservation(Target.transform.localPosition);
        sensor.AddObservation(this.transform.localPosition);

        // 에이전트 속도
        sensor.AddObservation(rb.linearVelocity.x);
        sensor.AddObservation(rb.linearVelocity.z);
    }

    public float ForceMultiplier = 10f;
    public override void OnActionReceived(ActionBuffers actions)
    { // 액션과 보상을 정의하는 곳
        // Actions, size = 2
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actions.ContinuousActions[0];
        controlSignal.z = actions.ContinuousActions[1];
        rb.AddForce(controlSignal * ForceMultiplier);

        // Rewards
        float distanceToTarget = Vector3.Distance(
            this.transform.localPosition, Target.transform.localPosition);
        // Reached Target
        if (distanceToTarget < 1.42f)
        {
            SetReward(1.0f);
            EndEpisode();
        }
        // Fell off Platform
        else if (this.transform.localPosition.y < 0)
        {
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }
}
