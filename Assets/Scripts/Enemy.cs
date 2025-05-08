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

    private GameObject player;

    private float xOffset;
    private float yOffset;

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
        player = null;
        agent = GetComponent<NavMeshAgent>();
        
        timeBetweenAttacks = 3f;
        sightRange = 500f;
        attackRange = 50f;

        isInCover = false;
        isPeeking = false;

        Scene currentScene = SceneManager.GetActiveScene();
        sceneName = currentScene.name;

        health = 100f;

        searchRadius = 400f;

        coverPoints = GameObject.FindGameObjectsWithTag("CoverPoint");

        StartCoroutine(CheckPlayerRanges());
    }

    private IEnumerator CheckPlayerRanges()
    {
        while (true)
        {
            if (player != null)
            {
                playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer | whatIsAlly);
                playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer | whatIsAlly);

                Vector3 toPlayer = player.transform.position - this.transform.position;
                if (Physics.Raycast(this.transform.position + Vector3.up * 1f, toPlayer.normalized, out RaycastHit hit, toPlayer.magnitude))
                {
                    if (hit.collider.CompareTag("Player") || hit.collider.CompareTag("Ally"))
                        playerInLineOfSight = true;
                    else
                        playerInLineOfSight = false;
                }
            }
            yield return new WaitForSeconds(0.2f); // Reduce physics checks
        }
    }

    private void Update()
    {
        if (health <= 0)
        {
            Destroy(gameObject);
            BattleManager.Instance.ReduceNumEnemies();
        }

        player = FindClosestPlayer();
        if (player == null || sceneName == "WorldMap") return;

        if (playerInSightRange && !playerInAttackRange)
            ChasePlayer();
        else if (playerInSightRange && playerInAttackRange && playerInLineOfSight)
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

        if (sceneName != "WorldMap" || player == null) return;

        ApplyBehaviors();
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
        Vector3 seekForce = Seek();
        seekForce.y = 0;
        seekForce *= 0.5f;

        collider = GetComponent<Collider>();
        Vector3 size = collider.bounds.size;

        Physics.Raycast(this.transform.position + new Vector3(0, size.y / 2, 0), transform.TransformDirection(Vector3.down), out RaycastHit raycastHit, float.MaxValue, MouseWorld.GetInstance().GetLayerMask());

        this.transform.position = new Vector3(this.transform.position.x, raycastHit.point.y, this.transform.position.z);

        this.ApplyForce(seekForce);

        ApplyForce(seekForce);
    }

    private Vector3 Seek()
    {
        Vector3 direction = Vector3.zero;
        int count = 0;
        float neighborRadius = 500f;

        float distance = Vector3.Distance(player.transform.position, transform.position);
        if (distance < neighborRadius)
        {
            direction += (player.transform.position - transform.position);
            count++;
        }

        foreach (Enemy boid in enemies)
        {
            if (boid == this) continue;
            distance = Vector3.Distance(boid.transform.position, transform.position);
            if (distance < neighborRadius)
            {
                direction += (boid.transform.position - transform.position);
                count++;
            }
        }

        if (count > 0)
        {
            direction.Normalize();
            Vector3 desiredVelocity = direction * maxSpeed;
            Vector3 steering = desiredVelocity - velocity;
            return Vector3.ClampMagnitude(steering, maxForce);
        }

        Wander();
        return Vector3.zero;
    }

    private void Wander()
    {
        float perlinX = Mathf.PerlinNoise(xOffset, 0);
        float perlinY = Mathf.PerlinNoise(yOffset, 0);
        float x = Mathf.Lerp(-maxSpeed, maxSpeed, perlinX);
        float z = Mathf.Lerp(-maxSpeed, maxSpeed, perlinY);
        xOffset += 0.01f;
        yOffset += 0.01f;
        velocity.x = x;
        velocity.z = z;
    }

    private void ChasePlayer()
    {
        if (Vector3.Distance(agent.destination, player.transform.position) > 1f)
            agent.SetDestination(GetFlankPosition());
    }

    private void AttackPlayer()
    {
        agent.SetDestination(transform.position);
        transform.LookAt(player.transform);

        if (!alreadyAttacked && !ShouldGuerilla())
        {
            FireBullet();
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
        else if (!alreadyAttacked && ShouldGuerilla())
        {
            FireAndRetreat();
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
        else
        {
            Relocate();
        }
    }

    private void FireBullet()
    {
        Vector3 aimDir = (player.transform.position - spawnBulletPosition.position).normalized;
        Instantiate(bullet, spawnBulletPosition.position, Quaternion.LookRotation(aimDir, Vector3.up));
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

            // skip if player can see this cover directly
            if (Physics.Raycast(coverPos + Vector3.up * 1f, toPlayer.normalized, out RaycastHit hit, toPlayer.magnitude))
            {
                if (hit.collider.CompareTag("Player") || hit.collider.CompareTag("Ally"))
                    continue;
            }

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
            Invoke(nameof(PeekOutAndShoot), 2f);
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

    private Vector3 GetFlankPosition()
    {
        Vector3 toPlayer = (player.transform.position - transform.position).normalized;
        Vector3 flankDirection = Vector3.Cross(toPlayer, Vector3.up);
        return player.transform.position + flankDirection * 5f;
    }

    private void StartRelocationTimer()
    {
        Invoke(nameof(Relocate), Random.Range(5f, 10f));
    }

    private void Relocate()
    {
        currentCover = FindBestCover();
        if (currentCover != null)
            agent.SetDestination(currentCover.position);
            isInCover = true;
        StartRelocationTimer();
    }

    private bool ShouldGuerilla()
    {
        int alliesNearby = Physics.OverlapSphere(transform.position, 10f, whatIsEnemy).Length;
        return health < 30f && alliesNearby < 2;
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
    }
}
