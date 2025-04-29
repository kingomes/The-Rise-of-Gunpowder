using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

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

    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private float timeBetweenAttacks;
    private bool alreadyAttacked;

    [SerializeField] private float sightRange;
    [SerializeField] private float attackRange;
    [SerializeField] private bool playerInSightRange;
    [SerializeField] private bool playerInAttackRange;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private LayerMask whatIsPlayer;
    [SerializeField] private GameObject bullet;
    [SerializeField] private Transform spawnBulletPosition;

    private bool isInCover;
    private bool isPeeking;
    private Transform currentCover;

    private string sceneName;

    private float health;

    [SerializeField] private float numAllies;
    void Start() 
    {
        acceleration = Vector3.zero;
        velocity = Vector3.zero;
        maxSpeed = 2f;
        maxForce = 0.1f;
        mass = 1f;
        player = GameObject.FindGameObjectWithTag("Player");
        agent = GetComponent<NavMeshAgent>();
        
        timeBetweenAttacks = 10f;
        sightRange = 500f;
        attackRange = 100f;

        isInCover = false;
        isPeeking = false;

        Scene currentScene = SceneManager.GetActiveScene();
        sceneName = currentScene.name;

        numAllies = 50;
    }

    private void Update()
    {
        if (player == null)
        {
            return;
        }

        if (sceneName != "WorldMap")
        {
            playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
            playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

            if (playerInSightRange && !playerInAttackRange) ChasePlayer();
            if (playerInSightRange && playerInAttackRange) AttackPlayer();
            if (isInCover) PeekAndShoot();

            if (agent.remainingDistance <= agent.stoppingDistance && currentCover != null)
            {
                isInCover = true;
            }
        }
    }

    void FixedUpdate() 
    {
        if (sceneName == "WorldMap")
        {
            if (player == null)
            {
                return;
            }
            
            ApplyBehaviors();

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

    private void ChasePlayer()
    {
        agent.SetDestination(player.transform.position);
    }

    private void AttackPlayer()
    {
        agent.SetDestination(transform.position);
        transform.LookAt(player.transform);

        if (!alreadyAttacked)
        {
            Vector3 aimDir = (player.transform.position - spawnBulletPosition.position).normalized;
            Instantiate(bullet, spawnBulletPosition.position, Quaternion.LookRotation(aimDir, Vector3.up));

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
        else
        {
            Transform cover = FindClosestCover();
            if (cover != null)
            {
                agent.SetDestination(cover.transform.position);
                currentCover = cover;
            }
        }
    }

    private Transform FindClosestCover()
    {
        GameObject[] coverPoints = GameObject.FindGameObjectsWithTag("CoverPoint");
        Transform bestCover = null;
        float bestDistance = Mathf.Infinity;

        foreach (GameObject cover in coverPoints)
        {
            float distanceToCover = Vector3.Distance(transform.position, cover.transform.position);
            Vector3 directionToPlayer = player.transform.position - cover.transform.position;

            // Check if cover is between enemy and player (optional: use Raycast for more realism)
            if (Physics.Raycast(cover.transform.position, directionToPlayer.normalized, out RaycastHit hit))
            {
                if (hit.collider.gameObject.CompareTag("Player"))
                {
                    // Not good cover if player is directly visible
                    continue;
                }
            }

            if (distanceToCover < bestDistance)
            {
                bestCover = cover.transform;
                bestDistance = distanceToCover;
            }
        }

        return bestCover;
    }

    private void PeekAndShoot()
    {
        if (!isPeeking && !alreadyAttacked)
        {
            isPeeking = true;

            Invoke(nameof(PeekOutAndShoot), 2f);
        }
    }

    private void PeekOutAndShoot()
    {
        if (player == null)
            return;

        
        transform.LookAt(player.transform);

        
        Vector3 peekPosition = transform.position + transform.right * 1f;
        agent.SetDestination(peekPosition);

        
        Vector3 aimDir = (player.transform.position - spawnBulletPosition.position).normalized;
        Instantiate(bullet, spawnBulletPosition.position, Quaternion.LookRotation(aimDir, Vector3.up));

        
        Invoke(nameof(ReturnToCover), 1.5f);
    }

    private void ReturnToCover()
    {
        if (currentCover != null)
        {
            agent.SetDestination(currentCover.transform.position);
        }

        Invoke(nameof(ResetPeek), 3f);
    }

    private void ResetPeek()
    {
        isPeeking = false;
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    private void TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0) Invoke(nameof(DestroyEnemy), 0.5f);
    }

    private void DestroyEnemy()
    {
        Destroy(gameObject);
    }
}
