using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class NavMeshBakeHelper : MonoBehaviour
{
    public NavMeshSurface surface;

    public void BakeNavMesh()
    {
        NavMeshObstacle[] obstacles = FindObjectsOfType<NavMeshObstacle>();

        foreach (var o in obstacles)
            o.enabled = false;

        surface.BuildNavMesh();

        foreach (var o in obstacles)
            o.enabled = true;

    }
}