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

    private bool enemyInSightRange;
    private bool enemyInAttackRange;

    private bool isInCover;
    private bool isPeeking;

    private float health;
    [SerializeField] private BattleManager battleManager;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        coverPoints = GameObject.FindGameObjectsWithTag("CoverPoint");

        enemy = null;
        
        timeBetweenAttacks = 3f;
        sightRange = 500f;
        attackRange = 100f;

        isInCover = false;
        isPeeking = false;

        health = 100f;

        searchRadius = 400f;

        StartCoroutine(CheckEnemyRanges());
    }

    private IEnumerator CheckEnemyRanges()
    {
        while (true)
        {
            if (enemy != null)
            {
                enemyInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsEnemy);
                enemyInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsEnemy);
            }
            yield return new WaitForSeconds(0.2f);
        }
    }

    private void Update()
    {
        if (health <= 0)
        {
            Destroy(gameObject);
            BattleManager.Instance.ReduceNumAllies();
        }

        enemy = FindClosestEnemy();
        if (enemy == null) return;

        if (enemyInSightRange && !enemyInAttackRange)
            ChaseEnemy();
        else if (enemyInSightRange && enemyInAttackRange)
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
        agent.SetDestination(transform.position);
        transform.LookAt(enemy.transform);

        if (!alreadyAttacked)
        {
            FireBullet();
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
        else
        {
            if (currentCover == null)
                currentCover = FindBestCover();

            if (currentCover != null)
                agent.SetDestination(currentCover.position);
        }
    }

    private void FireBullet()
    {
        Vector3 aimDir = (enemy.transform.position - spawnBulletPosition.position).normalized;
        Instantiate(bullet, spawnBulletPosition.position, Quaternion.LookRotation(aimDir, Vector3.up));
        // Consider switching to object pooling here
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

            // skip if player can see this cover directly
            if (Physics.Raycast(coverPos + Vector3.up * 1f, toEnemy.normalized, out RaycastHit hit, toEnemy.magnitude))
            {
                if (hit.collider.CompareTag("Enemy"))
                    continue;
            }

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
            Invoke(nameof(PeekOutAndShoot), 2f);
        }
    }

    private void PeekOutAndShoot()
    {
        if (enemy == null) return;

        transform.LookAt(enemy.transform);
        Vector3 peekPos = transform.position + transform.right * 1f;
        agent.SetDestination(peekPos);

        FireBullet();
        Invoke(nameof(ReturnToCover), 1.5f);
    }

    private void ReturnToCover()
    {
        if (currentCover != null)
            agent.SetDestination(currentCover.position);

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

    public void TakeDamage(int damage)
    {
        health -= damage;
    }
}
