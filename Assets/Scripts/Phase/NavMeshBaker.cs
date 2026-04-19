using Unity.AI.Navigation;
using UnityEngine;

public class NavMeshBaker : MonoBehaviour
{
    [SerializeField] private NavMeshSurface _surface;

    public void BuildNow() => _surface.BuildNavMesh();
}