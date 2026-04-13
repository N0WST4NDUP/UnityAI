using UnityEngine;
using UnityEngine.AI;

public class EnemyAgent : MonoBehaviour
{
    // --- 인스펙터 설정 ---
    [SerializeField] private Transform _player;
    [SerializeField] private float _maxHP;
    [SerializeField] private float _idleTime;
    [SerializeField] private float _detectionRange;
    [SerializeField] private float _attackRange;
    [SerializeField] private Transform[] _wayPoints;

    // --- 컴포넌트 참조 ---
    private NavMeshAgent _agent;

    // --- 내부 상태값 ---
    private float _currHP;
    private StateMachine _stateMachine;

    // --- 프로퍼티 ---
    public Transform Target => _player;
    public NavMeshAgent Agent => _agent;
    public StateMachine StateMachine => _stateMachine;
    public float IdleTime => _idleTime;
    public float DetectionRange => _detectionRange;
    public float AttackRange => _attackRange;
    public int WayPointsLength => _wayPoints.Length;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _currHP = _maxHP;
        _stateMachine = new();
    }

    private void Update()
    {
        _stateMachine.Tick();
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"TakeDamage: 유효하지 않은 damage 값 {damage}");
#endif
            return;
        }

        _currHP -= damage;

        if (_currHP <= 0)
        {
            // TODO: _stateMachine.ChangeState(new DeadState(this));
        }
    }

    private Transform GetWayPoint(int idx) => _wayPoints[idx];
}