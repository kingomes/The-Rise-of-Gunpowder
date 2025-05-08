using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using System.Collections;

public class Enemy : MonoBehaviour
{
    private Vector3 acceleration = Vector3.zero;
    private Vector3 velocity = Vector3.zero;
    private float maxSpeed;
    private float mass;
    private float maxForce;

    private float walkIntervalDuration;
    private float walkIntervalTimer;

    private float turnIntervalDuration;
    private float turnIntervalTimer;

    private GameObject player;

    private float xOffset;
    private float yOffset;
    private float xIncrement;
    private float yIncrement;

    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private float timeBetweenAttacks;
    private bool alreadyAttacked;
    private bool isInCover;
    private bool isPeeking;
    private Transform currentCover;

    [SerializeField] private GameObject bullet;
    [SerializeField] private Transform spawnBulletPosition;

    [SerializeField] private float sightRange;
    [SerializeField] private float attackRange;
    [SerializeField] private float searchRadius;
    [SerializeField] private LayerMask whatIsPlayer;
    [SerializeField] private LayerMask whatIsAlly;
    [SerializeField] private LayerMask whatIsEnemy;

    private bool playerInSightRange;
    private bool playerInAttackRange;
    private bool playerInLineOfSight;

    private GameObject[] coverPoints;
    private string sceneName;

    private Collider collider;

    [SerializeField] private Enemy[] enemies;

    private float health;

    void Start()
    {
        this.transform.GetChild(2).GetChild(0).GetComponent<Renderer>().materials[0].color = Color.red;
        this.transform.GetChild(2).GetChild(0).GetComponent<Renderer>().materials[1].color = Color.red;
        this.transform.GetChild(2).GetChild(0).GetComponent<Renderer>().materials[2].color = Color.red;

        acceleration = Vector3.zero;
        velocity = Vector3.zero;
        maxSpeed = 2f;
        maxForce = 0.1f;
        mass = 1f;
        if (sceneName != "WorldMap")
            player = null;
        else
            player = GameObject.FindGameObjectWithTag("Player");
        agent = GetComponent<NavMeshAgent>();
        
        timeBetweenAttacks = 3f;
        sightRange = 500f;
        attackRange = 50f;

        playerInSightRange = false;
        playerInAttackRange = false;
        playerInLineOfSight = false;

        isInCover = false;
        isPeeking = false;

        xOffset = Random.Range(-1000, 1000);
        yOffset = Random.Range(-1000, 1000);

        Scene currentScene = SceneManager.GetActiveScene();
        sceneName = currentScene.name;

        health = 100f;

        searchRadius = 400f;

        coverPoints = GameObject.FindGameObjectsWithTag("CoverPoint");

        StartCoroutine(UpdateClosestPlayer());
        StartCoroutine(CheckPlayerRanges());
    }

    private IEnumerator CheckPlayerRanges()
    {
        while (true)
        {
            if (player != null)
            {
                Vector3 toPlayer = player.transform.position - this.transform.position;
                if (Physics.Raycast(this.transform.position + Vector3.up * 1f, toPlayer.normalized, out RaycastHit hit, toPlayer.magnitude, whatIsPlayer | whatIsAlly))
                {
                    playerInLineOfSight = true;
                }
                else
                {
                    playerInLineOfSight = false;
                }
                
                playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer | whatIsAlly);

                if (playerInLineOfSight)
                    playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer | whatIsAlly);
            }
            yield return new WaitForSeconds(0.2f); // Reduce physics checks
        }
    }

    private IEnumerator UpdateClosestPlayer()
    {
        while (true)
        {
            player = FindClosestPlayer();
            yield return new WaitForSeconds(0.5f);
        }
    }


    private void Update()
    {
        if (health <= 0)
        {
            Destroy(gameObject);
            BattleManager.Instance.ReduceNumEnemies();
        }

        if (player == null || sceneName == "WorldMap") return;

        if (playerInSightRange && !playerInAttackRange)
            ChasePlayer();
        else if (playerInSightRange && playerInAttackRange)
            AttackPlayer();

        if (isInCover)
           PeekAndShoot();

        if (agent.remainingDistance <= agent.stoppingDistance && currentCover != null)
            isInCover = true;
    }

    void FixedUpdate()
    {
        if (sceneName == "WorldMap") agent.enabled = false;
        else agent.enabled = true;

        if (sceneName != "WorldMap") return;

        ApplyBehaviors();
        WrapAround();
        velocity += acceleration;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        transform.position += velocity;
        transform.rotation = Quaternion.LookRotation(velocity, Vector3.up);
        acceleration = Vector3.zero;
    }

    private void ApplyForce(Vector3 force)
    {
        acceleration += force / mass;
    }

    private void ApplyBehaviors()
    {
        if (player == null || Vector3.Distance(player.transform.position, transform.position) > 500f)
        {
            Wander();
        }
        else
        {
            Vector3 seekForce = Seek();
            seekForce.y = 0;
            ApplyForce(seekForce);
        }

        collider = GetComponent<Collider>();
        Vector3 size = collider.bounds.size;

        Physics.Raycast(this.transform.position + new Vector3(0, size.y / 2, 0),
            transform.TransformDirection(Vector3.down), out RaycastHit raycastHit,
            float.MaxValue, MouseWorld.GetInstance().GetLayerMask());

        this.transform.position = new Vector3(this.transform.position.x, raycastHit.point.y, this.transform.position.z);
    }


    private Vector3 Seek()
    {
        if (player == null)
            return Vector3.zero;

        Vector3 direction = Vector3.zero;
        float neighborRadius = 500f;

        float distance = Vector3.Distance(player.transform.position, transform.position);
        if (distance < neighborRadius)
        {
            direction += player.transform.position - transform.position;
        }

        direction.Normalize();
        Vector3 desiredVelocity = direction * maxSpeed;
        Vector3 steering = desiredVelocity - velocity;
        return Vector3.ClampMagnitude(steering, maxForce);
    }

    private void Wander()
    {
        //reduce the time left on the timers
        walkIntervalTimer -= Time.deltaTime;
        turnIntervalTimer -= Time.deltaTime;

        // perlin noise setup
        float perlinX = Mathf.PerlinNoise(xOffset, 0);
        float perlinY = Mathf.PerlinNoise(yOffset, 0);
        float xVelocity = Unity.Mathematics.math.remap(0, 1, -this.maxSpeed, this.maxSpeed, perlinX);
        float zVelocity = Unity.Mathematics.math.remap(0, 1, -this.maxSpeed, this.maxSpeed, perlinY);
        
        // change the speed of movement
        if (walkIntervalTimer <= 0)
        {
            this.xIncrement = Random.Range(-0.01f, 0.01f);
            this.yIncrement = Random.Range(-0.01f, 0.01f);
            walkIntervalTimer = walkIntervalDuration;
        }

        // turn around
        if (turnIntervalTimer <= 0)
        {
            this.xOffset *= -1;
            this.yOffset *= -1;

            // Bias toward center
            Vector2 centerVelocity = Vector2.zero;
            Vector2 offset = new Vector2(xOffset, yOffset);
            offset = Vector2.SmoothDamp(offset, Vector2.zero, ref centerVelocity, 5f); // 5 sec to fully settle
            xOffset = offset.x;
            yOffset = offset.y;

            turnIntervalTimer = turnIntervalDuration;
        }

        this.xOffset += this.xIncrement;
        this.yOffset += this.yIncrement;
        velocity.x = xVelocity;
        velocity.z = zVelocity;
    }

    private void WrapAround()
    {
        if (this.transform.position.x > 1200)
        {
            this.transform.position = new Vector3(-1200, transform.position.y, transform.position.z);
        }
        if (this.transform.position.x < -1200)
        {
            this.transform.position = new Vector3(1200, transform.position.y, transform.position.z);
        }
        if (this.transform.position.z > 1200)
        {
            this.transform.position = new Vector3(transform.position.x, transform.position.y, -1200);
        }
        if (this.transform.position.z < -1200)
        {
            this.transform.position = new Vector3(transform.position.x, transform.position.y, 1200);
        }
    }

    private void ChasePlayer()
    {
        if (Vector3.Distance(agent.destination, player.transform.position) > 1f)
            agent.SetDestination(player.transform.position);
    }

    private void AttackPlayer()
    {
        if (!alreadyAttacked && !ShouldGuerilla())
        {
            agent.SetDestination(transform.position);
            transform.LookAt(player.transform);
            FireBullet();
        }
        else if (!alreadyAttacked && ShouldGuerilla())
        {
            FireAndRetreat();
        }
        else if (alreadyAttacked && ShouldGuerilla())
        {
            Relocate();
        }
    }

    private void FireBullet()
    {
        Vector3 targetPosition = player.GetComponent<Collider>().bounds.center;
        Vector3 aimDir = (targetPosition - spawnBulletPosition.position).normalized;
        Instantiate(bullet, spawnBulletPosition.position, Quaternion.LookRotation(aimDir, Vector3.up));
        alreadyAttacked = true;
        Invoke(nameof(ResetAttack), timeBetweenAttacks);
    }

    private Transform FindBestCover()
    {
        Transform bestCover = null;
        float bestScore = Mathf.NegativeInfinity;

        foreach (GameObject cover in coverPoints)
        {
            Vector3 coverPos = cover.transform.position;
            Vector3 toCover = coverPos - transform.position;
            Vector3 toPlayer = player.transform.position - coverPos;

            // Score this cover point
            float distanceScore = -toCover.magnitude;  // Closer to enemy
            float safetyScore = toPlayer.magnitude;    // Farther from player

            float totalScore = distanceScore + safetyScore;

            if (totalScore > bestScore)
            {
                bestScore = totalScore;
                bestCover = cover.transform;
            }
        }

        return bestCover;
    }


    private GameObject FindClosestPlayer()
    {
        Collider[] nearby = Physics.OverlapSphere(transform.position, searchRadius, whatIsPlayer | whatIsAlly);
        GameObject bestPlayer = null;
        float bestDist = Mathf.Infinity;

        foreach (Collider col in nearby)
        {
            if (!col.CompareTag("Player") && !col.CompareTag("Ally")) continue;

            float dist = Vector3.Distance(transform.position, col.transform.position);
            if (dist < bestDist)
            {
                bestPlayer = col.gameObject;
                bestDist = dist;
            }
        }

        return bestPlayer;
    }

    private void PeekAndShoot()
    {
        if (!isPeeking && !alreadyAttacked)
        {
            isPeeking = true;
            alreadyAttacked = true;
            Invoke(nameof(PeekOutAndShoot), 2f);
            Invoke(nameof(ResetPeek), timeBetweenAttacks + 2f);
        }
    }

    private void PeekOutAndShoot()
    {
        if (player == null) return;

        transform.LookAt(player.transform);
        Vector3 peekPosition = transform.position + transform.right * 1f;
        agent.SetDestination(peekPosition);
        FireBullet();
        isInCover = false;
    }

    private void FireAndRetreat()
    {
        FireBullet();

        Vector3 retreatDirection = (transform.position - player.transform.position).normalized;
        Vector3 retreatPosition = transform.position + retreatDirection * 10f;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(retreatPosition, out hit, 10f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            isInCover = false;
            currentCover = null;
        }
    }

    private void StartRelocationTimer()
    {
        Invoke(nameof(Relocate), Random.Range(5f, 10f));
    }

    private void Relocate()
    {
        currentCover = FindBestCover();
        if (currentCover != null)
        {
            // Move to the side of the cover opposite the player
            Vector3 toPlayer = (player.transform.position - currentCover.position).normalized;
            Vector3 behindCoverPos = currentCover.position - toPlayer * 2f;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(behindCoverPos, out hit, 2f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                isInCover = true;
            }
        }

        StartRelocationTimer();
    }


    private bool ShouldGuerilla()
    {
        int alliesNearby = Physics.OverlapSphere(transform.position, 10f, whatIsEnemy).Length;
        Debug.Log(alliesNearby);
        return health < 50f && alliesNearby < 20;
    }

    private void ResetPeek()
    {
        isPeeking = false;
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log(gameObject.name + "was hit for " + damage + " damge");
    }
}
