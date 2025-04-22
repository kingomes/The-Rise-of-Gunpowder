using UnityEngine;

public class SpawnWorld : MonoBehaviour
{
    [Header("Enemy Spawn Settings")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float enemySpawnChance;
    [SerializeField] private float enemySpawnNumber;

    [Header("Player Spawn Settings")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private float playerSpawnChance;
    [SerializeField] private float playerSpawnNumber;

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
        SpawnEnemies();
        SpawnPlayer();
    }

    void SpawnEnemies()
    {
        int enemySpawnCount = 0;
        for (float x = negativePosition.x; x <= positivePosition.x; x += distanceBetweenChecks)
        {
            for (float z = negativePosition.y; z <= positivePosition.y; z += distanceBetweenChecks)
            {
                RaycastHit raycastHit;
                if (Physics.Raycast(new Vector3(x, heightOfCheck, z), Vector3.down, out raycastHit, rangeOfCheck, layerMask))
                {
                    if (enemySpawnChance > Random.Range(0, 101) && enemySpawnCount < enemySpawnNumber && x != 0 && z != 0)
                    {
                        Instantiate(enemyPrefab, raycastHit.point, Quaternion.identity, transform);
                        enemySpawnCount++;
                    }
                }
            }
        }
    }

    void SpawnPlayer()
    {
        int playerSpawnCount = 0;
        for (float x = positivePosition.x; x >= negativePosition.x; x -= distanceBetweenChecks)
        {
            for (float z = positivePosition.y; z >= negativePosition.y; z -= distanceBetweenChecks)
            {
                RaycastHit raycastHit;
                if (Physics.Raycast(new Vector3(x, heightOfCheck, z), Vector3.down, out raycastHit, rangeOfCheck, layerMask))
                {
                    if (playerSpawnChance > Random.Range(0, 101) && playerSpawnCount < playerSpawnNumber && x != 0 && z != 0)
                    {
                        Instantiate(playerPrefab, raycastHit.point, Quaternion.identity, transform);
                        playerSpawnCount++;
                    }
                }
            }
        }
    }
}
