using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;
    public int numAlliesWorld;
    public int numEnemiesWorld;
    public int numPlayersWorld;

    public int numAlliesBattle;
    public int numEnemiesBattle;
    public int numPlayersBattle;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        numEnemiesWorld = 3;
        numAlliesWorld = 0;
        numPlayersWorld = 1;

        numEnemiesBattle = 50;
        numAlliesBattle = 50;
        numPlayersBattle = 1;
    }

    void Update()
    {
        if (numEnemiesBattle <= 0)
        {
            Instance.numEnemiesWorld--;
            ResetCharacters();
            SceneManager.LoadScene("WorldMap");
        }
        else if (numPlayersBattle <= 0)
        {
            SceneManager.LoadScene("GameOverScene");
        }

        if (numEnemiesWorld <= 0)
        {
            SceneManager.LoadScene("WinScene");
        }
    }

    // reduce the number of each when one dies to keep track of who's winning
    public void ReduceNumEnemies()
    {
        numEnemiesBattle--;
    }

    public void ReduceNumAllies()
    {
        numAlliesBattle--;
    }

    public void ReduceNumPlayers()
    {
        numPlayersBattle--;
    }

    public void ResetCharacters()
    {
        numEnemiesBattle = 50;
        numAlliesBattle = 50;
        numPlayersBattle = 1;
    }
}
