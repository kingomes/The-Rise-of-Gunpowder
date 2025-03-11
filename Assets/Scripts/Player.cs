using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Vehicle : MonoBehaviour
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

    [SerializeField] private Vehicle[] vehicles;

    private Vector3 targetPosition;
    private bool shouldMove = false;

    private string sceneName;

    private float yOffset = 0.5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        acceleration = Vector3.zero;
        velocity = Vector3.zero;
        maxSpeed = 2f;
        maxForce = 0.1f;
        mass = 1f;
        vehicles = GameObject.FindObjectsOfType<Vehicle>();

        Scene currentScene = SceneManager.GetActiveScene();
        sceneName = currentScene.name;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1) && sceneName == "WorldMap")
        {
            targetPosition = MouseWorld.GetPosition();
            shouldMove = true;
        }
    }

    void FixedUpdate()
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

    private void ApplyForce(Vector3 force)
    {
        acceleration += force / mass;
    }

    private void ApplyBehaviors()
    {
        Vector3 seekForce = this.Seek(targetPosition);

        seekForce.y = 0;
        seekForce *= 0.5f;

        Physics.Raycast(this.transform.position + new Vector3(0, 5f, 0), transform.TransformDirection(Vector3.down), out RaycastHit raycastHit, float.MaxValue, MouseWorld.GetInstance().GetLayerMask());
        Debug.Log(this.transform.position);
        this.transform.position = new Vector3(this.transform.position.x, raycastHit.point.y, this.transform.position.z);

        this.ApplyForce(seekForce);
    }

    private void Flock()
    {
        Vector3 separationForce = this.Separate();
        Vector3 alignmentForce = this.Align();
        Vector3 cohesionForce = this.Cohere();

        separationForce.y = 0;
        alignmentForce.y = 0;
        cohesionForce.y = 0;

        separationForce *= 1.5f;
        alignmentForce *= 1.0f;
        cohesionForce *= 1.0f;

        this.ApplyForce(separationForce);
        this.ApplyForce(alignmentForce);
        this.ApplyForce(separationForce);
    }

    private Vector3 Seek(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;
        direction.Normalize();

        Vector3 desiredVelocity = direction * this.maxSpeed;

        Vector3 steeringForce = desiredVelocity - this.velocity;
        steeringForce = Vector3.ClampMagnitude(steeringForce, maxForce);

        return steeringForce;
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

    private Vector3 Separate()
    {
        float desiredSeparation = 2f;
        Vector3 separationVector = Vector3.zero;
        int count = 0;
        foreach (Vehicle other in vehicles)
        {
            if (other == this)
            {
                continue;
            }

            float distance = Vector3.Distance(other.transform.position, this.transform.position);
            if (distance < desiredSeparation)
            {
                Vector3 diffVector = this.transform.position - other.transform.position;
                diffVector.Normalize();
                diffVector *= 1 / distance;
                separationVector += diffVector;
                count++;
            }
        }

        if (count > 0)
        {
            separationVector.Normalize();
            separationVector *= maxSpeed;
            Vector3 steeringForce = separationVector - this.velocity;
            steeringForce = Vector3.ClampMagnitude(steeringForce, maxForce);
            return steeringForce;
        }
        else
        {
            return Vector3.zero;
        }
    }

    private Vector3 Align()
    {
        float maxNeighborDistance = 10f;
        float count = 0;

        Vector3 averageVelocity = Vector3.zero;

        foreach (Vehicle boid in vehicles)
        {
            float distance = Vector3.Distance(boid.transform.position, this.transform.position);
            if (this != boid && distance < maxNeighborDistance)
            {
                averageVelocity += boid.velocity;
                count++;
            }
        }

        if (count > 0)
        {
            averageVelocity /= vehicles.Length;
            averageVelocity.Normalize();
            averageVelocity *= maxSpeed;

            Vector3 steeringForce = averageVelocity - this.velocity;
            steeringForce = Vector3.ClampMagnitude(steeringForce, maxForce);
            return steeringForce;
        }
        else
        {
            return Vector3.zero;
        }
    }

    private Vector3 Cohere()
    {
        float maxNeighborDistance = 10f;
        float count = 0;

        Vector3 averagePosition = Vector3.zero;

        foreach (Vehicle boid in vehicles)
        {
            float distance = Vector3.Distance(boid.transform.position, this.transform.position);
            if (this != boid && distance < maxNeighborDistance)
            {
                averagePosition += boid.transform.position;
                count++;
            }
        }

        if (count > 0)
        {
            averagePosition /= count;
            return Seek(averagePosition);
        }
        else
        {
            return Vector3.zero;
        }
    }

    private Vector3 GetNormalPoint(Vector3 futurePoint, Vector3 pathStart, Vector3 pathEnd)
    {
        Vector3 A = futurePoint - pathStart;
        Vector3 B = pathEnd - pathStart;

        B.Normalize();
        float dotProduct = Vector3.Dot(A, B);

        B *= dotProduct;

        Vector3 normalPoint = pathStart + B;
        return normalPoint;
    }
}
