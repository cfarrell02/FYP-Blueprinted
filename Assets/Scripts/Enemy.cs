using System.Collections;
using UnityEngine;
using UnityEngine.AI;



public class Enemy : MonoBehaviour
{
    const float DETECT_DISTANCE = 8f;
    public float lookDistance = 30f;
    public int damage = 10;

    private GameObject player;
    private NavMeshAgent agent;
    private Renderer rend;
    private float timer = 0f;
    private Animator anim;
    private float fleeDistance = 5f;
    private LightingManager lightingManager;

    private BehaviourTree behaviourTree;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        agent.speed = 1f;
        rend = GetComponent<Renderer>();
        lightingManager = FindObjectOfType<LightingManager>();
        player = GameObject.FindWithTag("Player");
        if (rend == null)
        {
            rend = GetComponentInChildren<Renderer>();
        }

        // Initialize the behavior tree
        behaviourTree = new BehaviourTree(
            new Selector(
                new Sequence(
                    new Condition(IsPlayerInSight),
                    new Action(ChasePlayer)
                ),
                new Sequence(
                    new Condition(IsPlayerInRange),
                    new Action(AttackPlayer)
                ),
                new Sequence(
                    new Condition(ShouldFlee),
                    new Action(Flee)
                ),
                new Action(Wander)
            )
        );
    }

    void Update()
    {
        timer += Time.deltaTime;
        behaviourTree.Tick();
    }

    private bool IsPlayerInSight()
    {
        var direction = (player.transform.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, direction);
        return angle < 45f; // Check if player is within 45 degrees of the enemy's forward direction
    }

    private bool IsPlayerInRange()
    {
        float distance = Vector3.Distance(transform.position, player.transform.position);
        return distance < 2f;
    }

    private bool ShouldFlee()
    {
        return GetComponent<Health>().GetCurrentHealth() < 20;
    }

    private void Wander()
    {
        if (timer > 5f)
        {
            timer = 0f;
            var randomPosition = Random.insideUnitSphere * 10f;
            randomPosition += transform.position;
            NavMeshHit hit;
            NavMesh.SamplePosition(randomPosition, out hit, 10f, NavMesh.AllAreas);
            agent.SetDestination(hit.position);
        }
    }

    private void AttackPlayer()
    {
        if (timer > .8f)
        {
            timer = 0f;
            var health = player.GetComponent<Health>();
            int damageToDeal = damage + Random.Range(-5, 5);
            damageToDeal = lightingManager.isNight() ? damageToDeal * 2 : damageToDeal; // Double damage at night
            health.TakeDamage(damageToDeal);
        }
    }

    private void ChasePlayer()
    {
        print("Chasing");
        agent.SetDestination(player.transform.position);
    }

    private void Flee()
    {
        print("Fleeing");
        Vector3 towardsPlayer = (player.transform.position - transform.position).normalized;
        agent.SetDestination(transform.position - towardsPlayer * fleeDistance);
        agent.isStopped = false;
    }
}
