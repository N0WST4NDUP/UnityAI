using UnityEngine;

public class TeamRegistry : MonoBehaviour
{
    private const int k_TEAM_COUNT = 2;

    private readonly Team[] _teams = new Team[k_TEAM_COUNT];

    public Team this[int index] => _teams[index];

    private void Awake()
    {
        for (int i = 0; i < _teams.Length; i++)
        {
            _teams[i] = new(i);
        }
    }
}