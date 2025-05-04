using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleManager : MonoBehaviour
{
    private int numAllies;
    private int numEnemies;
    private int numPlayers;

    [SerializeField] private Spawner spawner;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        numEnemies = spawner.getNumEnemies();
        numAllies = spawner.getNumAllies();
        numPlayers = spawner.getNumPlayers();
    }

    void Update()
    {
        if (numEnemies <= 0)
        {
            SceneManager.LoadScene("WorldMap");
            spawner.reduceNumEnemies();
        }
        else if (numPlayers <= 0 && numAllies <= 0)
        {
            Debug.Log("Game over!");
        }
    }

    // reduce the number of each when one dies to keep track of who's winning
    public void reduceNumEnemies()
    {
        numEnemies--;
    }

    public void reduceNumAllies()
    {
        numAllies--;
    }

    public void reduceNumPlayers()
    {
        numPlayers--;
    }
}
