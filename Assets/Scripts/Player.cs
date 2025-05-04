using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    [SerializeField] private Vector3 acceleration;
    [SerializeField] private Vector3 velocity;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float mass;
    [SerializeField] private float maxForce;

    [SerializeField] private GameObject futurePoint;
    [SerializeField] private GameObject target;
    [SerializeField] private GameObject normalPoint;
    [SerializeField] private float lookAheadDistance;

    private Vector3 targetPosition;
    private bool shouldMove;

    private string sceneName;

    private Collider collider;

    [SerializeField] private MapGenerator mapGenerator;

    private float health;
    [SerializeField] private BattleManager battleManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        acceleration = Vector3.zero;
        velocity = Vector3.zero;
        maxSpeed = 2f;
        maxForce = 0.1f;
        mass = 1f;

        shouldMove = false;

        Scene currentScene = SceneManager.GetActiveScene();
        sceneName = currentScene.name;
        health = 100f;

        mapGenerator = GameObject.FindAnyObjectByType<MapGenerator>();

        battleManager = GameObject.FindAnyObjectByType<BattleManager>();
    }

    void Update()
    {
        if (health <= 0)
        {
            Destroy(gameObject);
            battleManager.reduceNumPlayers();
        }
        
        if (Input.GetMouseButtonDown(1) && sceneName == "WorldMap")
        {
            shouldMove = false;
            targetPosition = MouseWorld.GetPosition();

            Ray ray = Camera.main.ScreenPointToRay(targetPosition);
            Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, MouseWorld.GetInstance().GetLayerMask());

            Renderer rend = raycastHit.transform.GetComponent<Renderer>();
            MeshCollider meshCollider = raycastHit.collider as MeshCollider;

            if (rend == null || rend.sharedMaterial == null || rend.sharedMaterial.mainTexture == null || meshCollider == null)
                return;

            Texture2D tex = rend.material.mainTexture as Texture2D;

            Vector2 pixelUV = raycastHit.textureCoord;
            pixelUV.x *= tex.width;
            pixelUV.y *= tex.height;
            Color colorAtRayCast = tex.GetPixel((int) pixelUV.x, (int) pixelUV.y);

            float colorTolerance = 0.1f;
            foreach (TerrainType region in mapGenerator.regions)
            {
                if (region.name == "Grass" || region.name == "Grass 2" || region.name == "Sand")
                {
                    if (Mathf.Abs(colorAtRayCast.r - region.color.r) < colorTolerance && Mathf.Abs(colorAtRayCast.g - region.color.g) < colorTolerance && Mathf.Abs(colorAtRayCast.b - region.color.b) < colorTolerance)
                    {
                        shouldMove = true;
                        break;
                    }
                }
                else
                {
                    shouldMove = false;
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (sceneName == "WorldMap")
        {
            if (shouldMove)
            {
                ApplyBehaviors();
            }
            velocity += acceleration;
            velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
            this.transform.position += velocity;
            transform.rotation = Quaternion.LookRotation(velocity, Vector3.up);

            acceleration = Vector3.zero;
        }
    }

    private void ApplyForce(Vector3 force)
    {
        acceleration += force / mass;
    }

    private void ApplyBehaviors()
    {
        Vector3 seekForce = this.Arrive(targetPosition);

        seekForce.y = 0;
        seekForce *= 0.5f;

        collider = GetComponent<Collider>();
        Vector3 size = collider.bounds.size;

        Physics.Raycast(this.transform.position + new Vector3(0, size.y / 2, 0), transform.TransformDirection(Vector3.down), out RaycastHit raycastHit, float.MaxValue, MouseWorld.GetInstance().GetLayerMask());

        this.transform.position = new Vector3(this.transform.position.x, raycastHit.point.y, this.transform.position.z);

        this.ApplyForce(seekForce);
    }
    
    private Vector3 Arrive(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;
        float currentDistance = direction.magnitude;
        direction.Normalize();

        float slowDownRadius = 10f;
        float desiredSpeed = maxSpeed;
        
        if (currentDistance < slowDownRadius)
        {
            desiredSpeed = maxSpeed * (currentDistance / slowDownRadius);
        }

        Vector3 desiredVelocity = direction * desiredSpeed;

        Vector3 steeringForce = desiredVelocity - this.velocity;
        steeringForce = Vector3.ClampMagnitude(steeringForce, maxForce);

        return steeringForce;
    }

    // public void TakeDamage(float damage)
    // {
    //     health -= damage;
    // }
}
