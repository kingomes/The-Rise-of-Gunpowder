using UnityEngine;

public class SpawnBattle : MonoBehaviour
{
    [Header("Enemy Spawn Settings")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float enemySpawnNumber;

    [Header("Ally Spawn Settings")]
    [SerializeField] private GameObject allyPrefab;
    [SerializeField] private float allySpawnNumber;

    [Header("Player Spawn Settings")]
    [SerializeField] private GameObject playerPrefab;

    [Header("Raycast Settings")]
    [SerializeField] private float heightOfCheck;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private Vector2 negativePosition;
    [SerializeField] private Vector2 positivePosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        heightOfCheck = 100f;
        SpawnEnemies();
        SpawnAllies();
        SpawnPlayer();
    }

    void SpawnEnemies()
    {
        int enemySpawnCount = 0;
        while (enemySpawnCount < enemySpawnNumber)
        {
            float x = Random.Range(negativePosition.x, positivePosition.x);
            float z = Random.Range(negativePosition.y, positivePosition.y);

            RaycastHit raycastHit;
            if (Physics.Raycast(new Vector3(x, heightOfCheck, z), Vector3.down, out raycastHit, layerMask))
            {
                Instantiate(enemyPrefab, raycastHit.point, Quaternion.identity, transform);
                enemySpawnCount++;
            }
        }
    }

    void SpawnAllies()
    {
        int allySpawnCount = 0;
        while (allySpawnCount < allySpawnNumber)
        {
            float x = Random.Range(negativePosition.x, positivePosition.x);
            float z = Random.Range(negativePosition.y, positivePosition.y);

            RaycastHit raycastHit;
            if (Physics.Raycast(new Vector3(x, heightOfCheck, z), Vector3.down, out raycastHit, layerMask))
            {
                Instantiate(allyPrefab, raycastHit.point, Quaternion.identity, transform);
                allySpawnCount++;
            }
        }
    }

    void SpawnPlayer()
    {
        float x = Random.Range(negativePosition.x, positivePosition.x);
        float z = Random.Range(negativePosition.y, positivePosition.y);

        RaycastHit raycastHit;
        if (Physics.Raycast(new Vector3(x, heightOfCheck, z), Vector3.down, out raycastHit, layerMask))
        {
            Instantiate(playerPrefab, raycastHit.point, Quaternion.identity, transform);
        }
    }
}
