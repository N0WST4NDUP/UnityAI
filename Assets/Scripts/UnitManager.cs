using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    [Header("Unit 관련")]
    public GameObject UnitPrefab;
    public GameObject Group;

    [Header("생성 관련")]
    [SerializeField][Range(1, 10)] private int _capability = 5;

    private List<GameObject> _units = new();

    private void Update()
    {
        if (_units.Count < _capability)
        {
            GameObject unit = Instantiate(UnitPrefab, transform);
            unit.transform.SetParent(Group.transform);
            unit.GetComponent<Rigidbody>().AddForce(
                new(
                    Random.Range(-0.1f, 0.1f),
                    0f,
                    Random.Range(-0.1f, 0.1f)),
                    ForceMode.Impulse);

            _units.Add(unit);
        }
        else if (_units.Count > _capability)
        {
            Destroy(_units[0]);
            _units.RemoveAt(0);
        }
    }
}
