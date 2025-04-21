using Unity.AI.Navigation;
using UnityEngine;

public class BakeNavMesh : MonoBehaviour
{
    [SerializeField] private NavMeshSurface navMeshSurface;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        navMeshSurface.BuildNavMesh();
    }
}
