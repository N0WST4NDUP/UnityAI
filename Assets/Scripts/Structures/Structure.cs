using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshObstacle))]
public class Structure : MonoBehaviour, IGroupOwned, IDamageable
{
    // --- Inspector ---
    [SerializeField] private float _maxDurability = 100f;

    // --- Internal ---
    private int _groupId;
    private float _currDurability;
    private Vector3 _preparationPosition;

    // --- Properties ---
    public int GroupId => _groupId;
    public float MaxDurability => _maxDurability;
    public float CurrentDurability => _currDurability;
    public Vector3 PreparationPosition => _preparationPosition;

    public PhaseManager PhaseManager { get; private set; }

    private void Awake()
    {
        PhaseManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<PhaseManager>();
        PhaseManager.OnPreparationStart += ReturnTo;
    }

    public void Init(int groupId, Vector3 position)
    {
        _groupId = groupId;
        _currDurability = _maxDurability;
        _preparationPosition = position;
    }

    private void OnDestroy()
    {
        PhaseManager.OnPreparationStart -= ReturnTo;
    }

    public void OnDamaged(float damage)
    {
        if (damage <= 0) return;

        _currDurability -= damage;
        if (_currDurability <= 0) Die();
    }

    private void Die()
    {
        gameObject.SetActive(false);
    }

    protected virtual void ReturnTo()
    {
        gameObject.SetActive(true);
        transform.position = _preparationPosition;
        transform.rotation = Quaternion.identity;
        _currDurability = _maxDurability;
    }
}
