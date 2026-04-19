using System;
using UnityEngine;

public enum Phase
{
    Preparation,
    Battle
}

public class PhaseManager : MonoBehaviour
{
    // --- Inspector ---
    [SerializeField] private NavMeshBaker _baker;

    // --- Internal ---
    private Phase _current = Phase.Preparation;

    // --- Properties ---
    public Phase Current => _current;

    // --- Events ---
    public event Action OnPreparationStart;
    public event Action OnPreparationEnd;
    public event Action OnBattleStart;
    public event Action OnBattleEnd;

    // TODO: Phase 6에서 UI 버튼으로 교체
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (_current != Phase.Preparation)
            {
                StartPreparation();
            }
            else
            {
                StartBattle();
            }
        }
    }

    public void StartBattle()
    {
        if (_current != Phase.Preparation) return;

        _baker.BuildNow();
        _current = Phase.Battle;

        OnPreparationEnd?.Invoke();
        OnBattleStart?.Invoke();
        Debug.Log($"[{GetType().Name}] Battle started. NavMesh baked.");
    }

    public void StartPreparation()
    {
        if (_current != Phase.Battle) return;

        _current = Phase.Preparation;

        OnBattleEnd?.Invoke();
        OnPreparationStart?.Invoke();
        Debug.Log($"[{GetType().Name}] Battle ended.");
    }
}