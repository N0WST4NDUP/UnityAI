using UnityEngine;

public abstract class Unit : MonoBehaviour, IGroupOwned
{
    // --- Inspector ---
    [SerializeField] protected Tribe _tribe;
    [SerializeField] protected UnitType _type;
    [SerializeField] protected int _cost;

    // --- Internal ---
    protected int _groupId;
    protected int _rank = 1;
    protected Vector3 _preparationPosition;

    // --- External ---
    public int GroupId => _groupId;
    public Tribe Tribe => _tribe;
    public UnitType Type => _type;
    public int Rank => _rank;
    public int Cost => _cost;
    public Vector3 PreparationPosition => _preparationPosition;

    // --- Dependancies ---
    public PhaseManager PhaseManager { get; private set; }

    protected virtual void OnDestroy()
    {
        if (PhaseManager == null) return;

        PhaseManager.OnPreparationStart -= ReturnTo;
    }

    public virtual void Init(int groupId, Vector3 position, PhaseManager phaseManager)
    {
        _groupId = groupId;
        _preparationPosition = position;
        PhaseManager = phaseManager;

        PhaseManager.OnPreparationStart += ReturnTo;
    }

    protected virtual void ReturnTo()
    {
        gameObject.SetActive(true);
        transform.position = _preparationPosition;
        transform.rotation = Quaternion.identity;
    }
}