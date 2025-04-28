using UnityEngine;

public class SpawnTrees : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject treePrefab;
    [SerializeField] private float treeSpawnChance;

    [Header("Raycast Settings")]
    [SerializeField] private float distanceBetweenChecks;
    [SerializeField] private float heightOfCheck;
    [SerializeField] private float rangeOfCheck;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private Vector2 negativePosition;
    [SerializeField] private Vector2 positivePosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        heightOfCheck = 100f;
        SpawnTreesInMap();
    }

    void SpawnTreesInMap()
    {
        for (float x = negativePosition.x; x <= positivePosition.x; x += distanceBetweenChecks)
        {
            for (float z = negativePosition.y; z <= positivePosition.y; z += distanceBetweenChecks)
            {
                RaycastHit raycastHit;
                if (Physics.Raycast(new Vector3(x, heightOfCheck, z), Vector3.down, out raycastHit, rangeOfCheck, layerMask))
                {
                    if (treeSpawnChance > Random.Range(0, 101))
                    {
                        GameObject treeInstance = Instantiate(treePrefab, raycastHit.point, Quaternion.identity);
                        CapsuleCollider capsuleCollider = treeInstance.AddComponent<CapsuleCollider>();
                        capsuleCollider.radius = 1f;
                        capsuleCollider.center = new Vector3(0.5f, 5, -0.6f);

                        GameObject cover = new GameObject();
                        cover.transform.position = treeInstance.transform.position;
                        cover.tag = "CoverPoint";
                    }
                }
            }
        }
    }
}