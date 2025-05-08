using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using System.Collections;

public class Ally : MonoBehaviour
{
    private GameObject enemy;

    [SerializeField] private NavMeshAgent agent;
    private Transform currentCover;
    private GameObject[] coverPoints;

    [SerializeField] private GameObject bullet;
    [SerializeField] private Transform spawnBulletPosition;
    [SerializeField] private float timeBetweenAttacks;
    private bool alreadyAttacked;

    [SerializeField] private float sightRange;
    [SerializeField] private float attackRange;
    [SerializeField] private float searchRadius;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private LayerMask whatIsEnemy;
    [SerializeField] private LayerMask whatIsAlly;

    private bool enemyInSightRange;
    private bool enemyInAttackRange;
    private bool enemyInLineOfSight;

    private bool isInCover;
    private bool isPeeking;

    private float health;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        coverPoints = GameObject.FindGameObjectsWithTag("CoverPoint");

        enemy = null;
        
        timeBetweenAttacks = 3f;
        sightRange = 500f;
        attackRange = 50f;

        enemyInSightRange = false;
        enemyInAttackRange = false;
        enemyInLineOfSight = false;

        isInCover = false;
        isPeeking = false;

        health = 100f;

        searchRadius = 400f;

        StartCoroutine(UpdateClosestEnemy());
        StartCoroutine(CheckEnemyRanges());
    }

    private IEnumerator CheckEnemyRanges()
    {
        while (true)
        {
            if (enemy != null)
            {
                Vector3 toEnemy = enemy.transform.position - this.transform.position;
                if (Physics.Raycast(this.transform.position + Vector3.up * 1f, toEnemy.normalized, out RaycastHit hit, toEnemy.magnitude, whatIsEnemy))
                {
                    enemyInLineOfSight = true;
                }
                else
                {
                    enemyInLineOfSight = false;
                }

                enemyInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsEnemy);
                
                if (enemyInLineOfSight)
                    enemyInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsEnemy);
            }
            yield return new WaitForSeconds(0.2f); // Reduce physics checks
        }
    }

    private IEnumerator UpdateClosestEnemy()
    {
        while (true)
        {
            enemy = FindClosestEnemy();
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void Update()
    {
        if (health <= 0)
        {
            Destroy(gameObject);
            BattleManager.Instance.ReduceNumAllies();
        }

        if (enemy == null) return;

        if (enemyInSightRange && !enemyInAttackRange)
            ChaseEnemy();
        else if (enemyInSightRange && enemyInAttackRange && enemyInLineOfSight)
            AttackEnemy();

        if (isInCover)
            PeekAndShoot();

        if (agent.remainingDistance <= agent.stoppingDistance && currentCover != null)
            isInCover = true;
    }

    private void ChaseEnemy()
    {
        if (Vector3.Distance(agent.destination, enemy.transform.position) > 1f)
            agent.SetDestination(enemy.transform.position);
    }

    private void AttackEnemy()
    {
        if (!alreadyAttacked && !ShouldGuerilla())
        {
            agent.SetDestination(transform.position);
            transform.LookAt(enemy.transform);
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
        else
        {
        }
    }

    private void FireBullet()
    {
        Vector3 targetPosition = enemy.GetComponent<Collider>().bounds.center;
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
            Vector3 toEnemy = enemy.transform.position - coverPos;

            // Score this cover point
            float distanceScore = -toCover.magnitude;  // Closer to ally
            float safetyScore = toEnemy.magnitude;    // Farther from enemy

            float totalScore = distanceScore + safetyScore;

            if (totalScore > bestScore)
            {
                bestScore = totalScore;
                bestCover = cover.transform;
            }
        }

        return bestCover;
    }


    private GameObject FindClosestEnemy()
    {
        Collider[] nearby = Physics.OverlapSphere(transform.position, searchRadius, whatIsEnemy);
        GameObject bestEnemy = null;
        float bestDist = Mathf.Infinity;

        foreach (Collider col in nearby)
        {
            if (!col.CompareTag("Enemy")) continue;

            float dist = Vector3.Distance(transform.position, col.transform.position);
            if (dist < bestDist)
            {
                bestEnemy = col.gameObject;
                bestDist = dist;
            }
        }

        return bestEnemy;
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
        if (enemy == null) return;

        transform.LookAt(enemy.transform);
        Vector3 peekPosition = transform.position + transform.right * 1f;
        agent.SetDestination(peekPosition);
        FireBullet();
        isInCover = false;
    }

    private void FireAndRetreat()
    {
        FireBullet();
        Vector3 retreatDirection = (transform.position - enemy.transform.position).normalized;
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
            // Move to the side of the cover opposite the enemy
            Vector3 toEnemy = (enemy.transform.position - currentCover.position).normalized;
            Vector3 behindCoverPos = currentCover.position - toEnemy * 2f;

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
        int alliesNearby = Physics.OverlapSphere(transform.position, 10f, whatIsAlly).Length;
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
