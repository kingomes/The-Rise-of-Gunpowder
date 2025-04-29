using UnityEngine;
using UnityEngine.SceneManagement;

public class Spawner : MonoBehaviour
{
    [Header("Enemy Spawn Settings")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float enemySpawnNumber;

    [Header("Ally Spawn Settings")]
    [SerializeField] private GameObject allyPrefab;
    [SerializeField] private float allySpawnNumber;

    [Header("Player Spawn Settings")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private float playerSpawnNumber;

    [Header("Raycast Settings")]
    [SerializeField] private float distanceBetweenChecks;
    [SerializeField] private float heightOfCheck;
    [SerializeField] private float rangeOfCheck;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private Vector2 negativePosition;
    [SerializeField] private Vector2 positivePosition;

    private string sceneName;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        sceneName = currentScene.name;

        heightOfCheck = 100f;

        if (sceneName == "WorldMap")
        {
            negativePosition = new Vector2(-1200f, -1200f);
            positivePosition = new Vector2(1200f, 1200f);
            enemyPrefab.transform.localScale = new Vector3(20f, 20f, 20f);
            playerPrefab.transform.localScale = new Vector3(20f, 20f, 20f);

            SpawnEnemies();
            SpawnPlayer();
        }
        else
        {
            negativePosition = new Vector2(-400f, -400f);
            positivePosition = new Vector2(400f, 400f);

            SpawnEnemies();
            SpawnPlayerAndAllies();
        }
    }

    void SpawnEnemies()
    {
        int enemySpawnCount = 0;
        float z = Random.Range(negativePosition.y, positivePosition.y);
        for (float x = negativePosition.x; x < positivePosition.x; x += distanceBetweenChecks)
        {
            while (enemySpawnCount < enemySpawnNumber)
            {
                RaycastHit raycastHit;
                if (Physics.Raycast(new Vector3(x, heightOfCheck, z), Vector3.down, out raycastHit, rangeOfCheck, layerMask))
                {
                    Instantiate(enemyPrefab, raycastHit.point, Quaternion.identity);
                    enemySpawnCount++;
                }
            }
        }
    }

    void SpawnPlayerAndAllies()
    {
        int allySpawnCount = 0;
        float x = Random.Range(negativePosition.x, positivePosition.x);
        float z = Random.Range(negativePosition.y, positivePosition.y);

        int attempts = 0;
        int maxAttempts = 100;

        RaycastHit playerRaycastHit;
        if (Physics.Raycast(new Vector3(x, heightOfCheck, z), Vector3.down, out playerRaycastHit, rangeOfCheck, layerMask))
        {
            Instantiate(playerPrefab, playerRaycastHit.point, Quaternion.identity);
        }

        float allyX = x - allySpawnNumber / 2f;
        while (allySpawnCount < allySpawnNumber && attempts < maxAttempts)
        {
            RaycastHit allyRaycastHit;
            if (Physics.Raycast(new Vector3(allyX, heightOfCheck, z - 5f), Vector3.down, out allyRaycastHit, rangeOfCheck, layerMask))
            {
                Instantiate(allyPrefab, allyRaycastHit.point, Quaternion.identity);
                allySpawnCount++;
                allyX += 2f;
            }
            attempts++;
        }
    }

    void SpawnPlayer()
    {
        float x = Random.Range(negativePosition.x, positivePosition.x);
        float z = Random.Range(negativePosition.y, positivePosition.y);

        RaycastHit raycastHit;
        if (Physics.Raycast(new Vector3(x, heightOfCheck, z), Vector3.down, out raycastHit, rangeOfCheck, layerMask))
        {
            Instantiate(playerPrefab, raycastHit.point, Quaternion.identity);
        }
    }
}
