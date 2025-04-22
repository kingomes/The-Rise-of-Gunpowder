using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class Ally : MonoBehaviour
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

    [SerializeField] private float xOffset;
    [SerializeField] private float yOffset;

    [SerializeField] private Enemy[] enemies;
    [SerializeField] private GameObject enemy;

    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private float timeBetweenAttacks;
    private bool alreadyAttacked;

    [SerializeField] private float sightRange;
    [SerializeField] private float attackRange;
    [SerializeField] private bool playerInSightRange;
    [SerializeField] private bool playerInAttackRange;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private LayerMask whatIsPlayer;
    [SerializeField] private GameObject projectile;

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
        enemy = GameObject.FindGameObjectWithTag("Enemy");
        agent = GetComponent<NavMeshAgent>();

        Scene currentScene = SceneManager.GetActiveScene();
        sceneName = currentScene.name;

        numAllies = 50;
    }

    private void Update()
    {
        if (enemy == null)
        {
            return;
        }

        if (sceneName != "WorldMap")
        {
            playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
            playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

            if (playerInSightRange && !playerInAttackRange) ChasePlayer();
            if (playerInSightRange && playerInAttackRange) AttackPlayer();
        }
    }

    private void ChasePlayer()
    {
        agent.SetDestination(enemy.transform.position);
    }

    private void AttackPlayer()
    {
        agent.SetDestination(transform.position);
        transform.LookAt(enemy.transform);

        if (!alreadyAttacked)
        {
            Rigidbody rb = Instantiate(projectile, transform.position, Quaternion.identity).GetComponent<Rigidbody>();

            rb.AddForce(transform.forward * 32f, ForceMode.Impulse);
            rb.AddForce(transform.up * 8f, ForceMode.Impulse);

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
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
