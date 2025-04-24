using Unity.VisualScripting;
using UnityEngine;

public class UpdateMeshCollider : MonoBehaviour
{
    void Start()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        MeshCollider meshCollider = this.AddComponent<MeshCollider>();

        meshCollider.sharedMesh = null; 
        meshCollider.sharedMesh = meshFilter.sharedMesh;
    }
}
