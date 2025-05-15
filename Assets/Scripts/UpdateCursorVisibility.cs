using UnityEngine;
using UnityEngine.SceneManagement;

public class UpdateCursorVisibility : MonoBehaviour
{
    private string sceneName;

    void Start()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        sceneName = currentScene.name;
    }
    // Update is called once per frame
    void Update()
    {
        if (sceneName == "WorldMap" || sceneName == "GameOverScene" || sceneName == "WinScene")
        {
            if (Cursor.visible == false || Cursor.lockState != CursorLockMode.None)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }

        else
        {
            Cursor.visible = false;
        }
    }
}
