using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnWorld : MonoBehaviour
{
    [Header("Enemy Spawn Settings")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float enemySpawnChance;
    [SerializeField] private float enemySpawnNumber;

    [Header("Ally Spawn Settings")]
    [SerializeField] private GameObject allyPrefab;
    [SerializeField] private float allySpawnNumber;

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

            SpawnEnemies();
            SpawnPlayer();
        }
        else
        {
            negativePosition = new Vector2(-1200f, -1200f);
            positivePosition = new Vector2(1200f, 1200f);

            SpawnEnemies();
            SpawnAllies();
            SpawnPlayer();
        }
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
                    if (enemySpawnChance > Random.Range(0, 101) && enemySpawnCount < enemySpawnNumber)
                    {
                        Instantiate(enemyPrefab, raycastHit.point, Quaternion.identity);
                        enemySpawnCount++;
                    }
                }
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
            if (Physics.Raycast(new Vector3(x, heightOfCheck, z), Vector3.down, out raycastHit, Mathf.Infinity, layerMask))
            {
                Instantiate(allyPrefab, raycastHit.point, Quaternion.identity);
                allySpawnCount++;
            }
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
