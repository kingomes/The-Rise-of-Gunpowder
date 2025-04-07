using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour {

    [SerializeField] private Vector3 acceleration;
    [SerializeField] private Vector3 velocity;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float mass;
    [SerializeField] private float maxForce;

    [SerializeField] private GameObject futurePoint;
    [SerializeField] private GameObject target;
    [SerializeField] private GameObject normalPoint;
    [SerializeField] private float lookAheadDistance;

    [SerializeField] private float xOffset;
    [SerializeField] private float yOffset;

    [SerializeField] private Enemy[] enemies;
    [SerializeField] private GameObject player;

    void Start() 
    {
        acceleration = Vector3.zero;
        velocity = Vector3.zero;
        maxSpeed = 2f;
        maxForce = 0.1f;
        mass = 1f;
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void FixedUpdate() 
    {
        ApplyBehaviors();

        velocity += acceleration;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        this.transform.position += velocity;

        transform.rotation = Quaternion.LookRotation(velocity, Vector3.up);

        acceleration = Vector3.zero;
    }
    
    private void ApplyForce(Vector3 force)
    {
        acceleration += force / mass;
    }

    private void ApplyBehaviors()
    {
        Vector3 seekForce = this.Seek();

        seekForce.y = 0;
        seekForce *= 0.5f;

        this.ApplyForce(seekForce);
    }

    private void Wander() 
    {
        float perlinX = Mathf.PerlinNoise(xOffset, 0);
        float perlinY = Mathf.PerlinNoise(yOffset, 0);
        float xVelocity = Unity.Mathematics.math.remap(0, 1, -this.maxSpeed, this.maxSpeed, perlinX);
        float zVelocity = Unity.Mathematics.math.remap(0, 1, -this.maxSpeed, this.maxSpeed, perlinY);
        this.xOffset += 0.01f;
        this.yOffset += 0.01f;
        velocity.x = xVelocity;
        velocity.z = zVelocity;
    }

    private Vector3 Seek()
    {

        float maxNeighborDistance = 500f;
        float count = 0;
        Vector3 direction = Vector3.zero;

        float distance = Vector3.Distance(player.transform.position, this.transform.position);

        if (distance < maxNeighborDistance)
        {
            direction = player.transform.position - this.transform.position;
            count++;
        }

        foreach (Enemy boid in enemies)
        {
            distance = Vector3.Distance(boid.transform.position, this.transform.position);
            if (this != boid && distance < maxNeighborDistance)
            {
                direction = boid.transform.position - this.transform.position;
                count++;
            }
        }

        if (count > 0)
        {
            direction.Normalize();

            Vector3 desiredVelocity = direction * this.maxSpeed;

            Vector3 steeringForce = desiredVelocity - this.velocity;
            steeringForce = Vector3.ClampMagnitude(steeringForce, maxForce);

            return steeringForce;
        }
        else
        {
            Wander();
            return Vector3.zero;
        }
    }

    private void Flee(Vector3 targetPosition)
    {
        Vector3 direction = transform.position - targetPosition;
        direction.Normalize();

        Vector3 desiredVelocity = direction * this.maxSpeed;

        Vector3 steeringForce = desiredVelocity - this.velocity;
        steeringForce = Vector3.ClampMagnitude(steeringForce, maxForce);

        this.ApplyForce(steeringForce);
    }
}
