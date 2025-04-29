using Unity.Cinemachine;
using UnityEngine;

public class UpdateVirtualCamera : MonoBehaviour
{
    private CinemachineVirtualCamera virtualCamera;
    private GameObject player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
        virtualCamera.Follow = player.transform.GetChild(0).transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
