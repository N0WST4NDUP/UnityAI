using System.Collections.Generic;

public class Team
{
    public int GroupId { get; }

    private readonly List<Unit> _units = new();
    public IReadOnlyList<Unit> Units => _units;

    public Team(int groupId)
    {
        GroupId = groupId;
    }

    public void AddUnit(Unit unit)
    {
        _units.Add(unit);
    }

    public void RemoveUnit(Unit unit)
    {
        _units.Remove(unit);
    }
}