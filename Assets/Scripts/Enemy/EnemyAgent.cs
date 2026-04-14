using UnityEngine;
using UnityEngine.AI;

public class EnemyAgent : MonoBehaviour
{
    // --- 인스펙터 설정 ---
    [SerializeField] private Transform _player;
    [SerializeField] private Renderer _renderer;
    [SerializeField] private float _maxHP;
    [SerializeField] private float _idleTime;
    [SerializeField] private float _detectionRange;
    [SerializeField] private float _attackDamage;
    [SerializeField] private float _attackRange;
    [SerializeField] private float _attackCooldown;
    [SerializeField] private Transform[] _waypoints;

    // --- 컴포넌트 참조 ---
    private NavMeshAgent _agent;

    // --- 내부 상태값 ---
    private float _currHP;
    private StateMachine _stateMachine;
    private int _patrolIndex;

    // --- 프로퍼티 ---
    public Renderer Renderer => _renderer;
    public Transform Target => _player;
    public NavMeshAgent Agent => _agent;
    public StateMachine StateMachine => _stateMachine;
    public float IdleTime => _idleTime;
    public float DetectionRange => _detectionRange;
    public float AttackDamage => _attackDamage;
    public float AttackRange => _attackRange;
    public float AttackCooldown => _attackCooldown;
    public Transform CurrentWaypoint => _waypoints[_patrolIndex];

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _stateMachine = new();
    }

    private void OnEnable()
    {
        _currHP = _maxHP;
        _stateMachine.ChangeState(new EnemyIdleState(this));
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
            _stateMachine.ChangeState(new EnemyDeadState(this));
        }
    }

    public void NextWaypoint()
    {
        _patrolIndex = (_patrolIndex + 1) % _waypoints.Length;
    }
}