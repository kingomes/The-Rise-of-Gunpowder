using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class MouseWorld : MonoBehaviour
{
    public static MouseWorld instance;

    [SerializeField] private LayerMask mousePlanetLayerMask;
    
    private void Awake()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = MouseWorld.GetPosition();
    }

    public static MouseWorld GetInstance()
    {
        return instance;
    }

    public LayerMask GetLayerMask()
    {
        return mousePlanetLayerMask;
    }

    public static Vector3 GetPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, instance.mousePlanetLayerMask);

        return raycastHit.point;
    }
}
