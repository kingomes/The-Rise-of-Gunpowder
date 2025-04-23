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
        int maxAttempts = 1000;
        int attempts = 0;

        while (enemySpawnCount < enemySpawnNumber && attempts < maxAttempts)
        {
            attempts++;

            float x = Random.Range(negativePosition.x, positivePosition.x);
            float z = Random.Range(negativePosition.y, positivePosition.y);

            RaycastHit raycastHit;
            if (Physics.Raycast(new Vector3(x, heightOfCheck, z), Vector3.down, out raycastHit, Mathf.Infinity, layerMask))
            {
                Instantiate(enemyPrefab, raycastHit.point, Quaternion.identity);
                enemySpawnCount++;
            }
        }

        if (enemySpawnCount < enemySpawnNumber)
        {
            Debug.LogWarning($"Only spawned {enemySpawnCount} out of {enemySpawnNumber} enemies after {attempts} attempts.");
        }
    }


    void SpawnAllies()
    {
        int allySpawnCount = 0;
        int maxAttempts = 1000;
        int attempts = 0;
        while (allySpawnCount < allySpawnNumber && attempts < maxAttempts)
        {
            attempts++;

            float x = Random.Range(negativePosition.x, positivePosition.x);
            float z = Random.Range(negativePosition.y, positivePosition.y);

            RaycastHit raycastHit;
            if (Physics.Raycast(new Vector3(x, heightOfCheck, z), Vector3.down, out raycastHit, Mathf.Infinity, layerMask))
            {
                Debug.Log($"Raycast hit {raycastHit.collider.gameObject.name} at {raycastHit.point}");
                Instantiate(allyPrefab, raycastHit.point, Quaternion.identity);
                allySpawnCount++;
            }
        }

        if (allySpawnCount < enemySpawnNumber)
        {
            Debug.LogWarning($"Only spawned {allySpawnCount} out of {enemySpawnNumber} enemies after {attempts} attempts.");
        }
    }

    void SpawnPlayer()
    {
        float x = Random.Range(negativePosition.x, positivePosition.x);
        float z = Random.Range(negativePosition.y, positivePosition.y);

        RaycastHit raycastHit;
        if (Physics.Raycast(new Vector3(x, heightOfCheck, z), Vector3.down, out raycastHit, layerMask))
        {
            Instantiate(playerPrefab, raycastHit.point, Quaternion.identity);
        }
    }
}
