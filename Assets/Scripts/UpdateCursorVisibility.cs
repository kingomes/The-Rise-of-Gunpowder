using UnityEngine;

public class UpdateCursorVisibility : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (Cursor.visible == false || Cursor.lockState != CursorLockMode.None)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
