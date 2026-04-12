using UnityEngine;

public class Unit : MonoBehaviour
{
    private static string s_MoveSpeedParam = "MoveSpeed";

    [SerializeField][Range(2f, 10f)] private float _maxSpeed = 5f;
    private float _moveSpeed;

    private Rigidbody _unitRigidBody;
    private Animator _unitAnimator;

    private Transform _target = null;
    private float _sqrDetectionRange = 0.01f;
    private float _currentMoveSpeed = 0f;

    #region Unity Message
    private void Awake()
    {
        _unitRigidBody = GetComponent<Rigidbody>();
        _unitAnimator = GetComponent<Animator>();
    }

    private void Start()
    {
        _moveSpeed = Random.Range(_maxSpeed / 2f, _maxSpeed);
    }

    private void FixedUpdate()
    {
        if (_target == null) return;
        Vector3 direction = _target.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > _sqrDetectionRange)
        {
            _currentMoveSpeed = Mathf.Lerp(_currentMoveSpeed, _moveSpeed, Time.fixedDeltaTime * 10f);

            Vector3 dirNormalized = direction.normalized;
            Quaternion targetRotation = Quaternion.LookRotation(dirNormalized);
            _unitRigidBody.MoveRotation(Quaternion.Slerp(_unitRigidBody.rotation, targetRotation, Time.fixedDeltaTime * 10f));

            Vector3 moveVector = dirNormalized * (_currentMoveSpeed * Time.fixedDeltaTime);
            _unitRigidBody.MovePosition(_unitRigidBody.position + moveVector);
        }
        else
        {
            _currentMoveSpeed = Mathf.Lerp(_currentMoveSpeed, 0f, Time.fixedDeltaTime * 10f);
            _unitRigidBody.angularVelocity = Vector3.zero;
        }
    }

    private void Update()
    {
        _unitAnimator.SetFloat(s_MoveSpeedParam, _currentMoveSpeed);
    }
    #endregion

    #region Getters
    public float MoveSpeed => _moveSpeed;
    #endregion

    #region Setters
    public void SetTarget(Transform t) => _target = t;
    public void SetTarget(GameObject t) => _target = t.transform;
    #endregion

    #region Reset
    public void ResetPhysics()
    {
        _target = null;
        _currentMoveSpeed = 0f;
        _unitRigidBody.linearVelocity  = Vector3.zero;
        _unitRigidBody.angularVelocity = Vector3.zero;
    }
    #endregion
}
