using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class RegistryManager : MonoBehaviour
{
    public readonly List<Unit>[] GroupedUnits = new List<Unit>[8];

    private void Awake()
    {
        for (int i = 0; i < GroupedUnits.Length; i++)
        {
            GroupedUnits[i] = new();
        }
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Alpha3)) return;

        GroupTest.groupId = (GroupTest.groupId + 1) % 2;
        Debug.Log($"[{GetType().Name}] GroupId Changed: {GroupTest.groupId}");
    }

    public void Register(Unit unit)
    {
        GroupedUnits[unit.GroupId].Add(unit);
        Debug.Log($"[{GetType().Name}] Unit registered. GroupId: {unit.GroupId}");
    }

    public void Unregister(Unit unit)
    {
        if (!GroupedUnits[unit.GroupId].Contains(unit)) return;

        GroupedUnits[unit.GroupId].Remove(unit);
        Debug.Log($"[{GetType().Name}] Unit unregistered. GroupId: {unit.GroupId}");
    }
}