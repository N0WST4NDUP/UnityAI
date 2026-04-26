using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class SoldierAgent : Agent
{
    private Unit _unit;

    public void Init(Unit unit)
    {
        _unit = unit;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (TryGetComponent<UnitHealth>(out UnitHealth health))
        {
            sensor.AddObservation(health.CurrentHealth / health.MaxHealth);
        }
        else
        {
            sensor.AddObservation(0f);
        } // 1
        sensor.AddObservation(transform.position); // 3
        sensor.AddObservation(_unit.GroupId); // 1


    }
}