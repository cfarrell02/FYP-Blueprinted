using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBehaviour : MonoBehaviour
{

    // Enemy properties
    public float lookDistance = 30f;
    public float hearingDistance = 10f;
    public int damage = 10; 
    public float speed = 10f;
    public float fleeDistance = 5f;

    // Components
    private GameObject player;
    private NavMeshAgent agent;
    private Renderer rend;
    private Animator anim;
    private float timer = 0f;
    private LightingManager lightingManager;
    private int playerLevel = 1;
    
    // Timer for timeout on chasing
    private float chaseTimeout = 0f;
    private float chaseTimeoutDuration = 5f; // Adjust this value as needed

    // Behaviour tree stuff
    BehaviourTree tree;
    enum ActionState { IDLE, WORKING };
    ActionState state = ActionState.IDLE;

    void Start()
    {
        playerLevel = FindObjectOfType<LevelManager>().GetCurrentLevel();

        
        // Component initialization
        agent = GetComponent<NavMeshAgent>();
        agent.speed = speed;
        anim = GetComponent<Animator>();
        rend = GetComponent<Renderer>();
        lightingManager = FindObjectOfType<LightingManager>();
        player = GameObject.FindGameObjectWithTag("Player");
        if (rend == null)
            rend = GetComponentInChildren<Renderer>();

        // Behaviour tree initialization
        InitializeBehaviourTree();
        
        //ScaleBasedOnLevel();
        
    }
    
    
    void ScaleBasedOnLevel()
    {
        //Scale up health
        var health = GetComponent<Health>();
        health.SetHealth((int)(health.GetCurrentHealth() * (playerLevel * 0.1f)));
        
        //Scale up damage
        damage = (int)(damage * (playerLevel * 0.1f));
        
        //Scale up speed
        agent.speed = speed * (playerLevel * 0.1f);
    }

    void Update()
    {
        timer += Time.deltaTime;
        tree.Process();
    }

    void InitializeBehaviourTree()
    {
        // Behaviour tree construction
        tree = new BehaviourTree();

        // Sequence for attacking the player
        Sequence attackPlayer = new Sequence("Attack Player");

        Leaf chaseLeaf = new Leaf("Chase", ChasePlayer);
        Leaf breakBlockLeaf = new Leaf("Break Block", BreakBlockInFront);
        
        Selector chaseOrBreak = new Selector("Chase or Break");
        chaseOrBreak.AddChild(chaseLeaf);
        chaseOrBreak.AddChild(breakBlockLeaf);

        Leaf attackLeaf = new Leaf("Attack", AttackPlayer);
        Inverter attackInverter = new Inverter("Invert");
        attackInverter.AddChild(attackLeaf);
        
        attackPlayer.AddChild(chaseOrBreak);
        attackPlayer.AddChild(attackInverter);
        

        // Sequence for fleeing
        Sequence flee = new Sequence("Flee");

        Leaf fleeLeaf = new Leaf("Flee", Flee);
        flee.AddChild(fleeLeaf);

        Leaf wanderLeaf = new Leaf("Wander", Wander);
        flee.AddChild(wanderLeaf);

        // Root selector
        Selector root = new Selector("Root");

        root.AddChild(attackPlayer);
        root.AddChild(flee);

        tree.AddChild(root);
    }


    // Conditions
    private bool IsPlayerInSight()
    {
        Vector3 direction = player.transform.position - transform.position;
        float distanceToPlayer = direction.magnitude;

        // Check if player is within the look distance
        if (distanceToPlayer <= lookDistance)
        {
            // Perform raycast to check for obstacles
            RaycastHit hit;
            if (Physics.Raycast(transform.position, direction, out hit, lookDistance))
            {
                if (hit.collider.gameObject.CompareTag("Player"))
                {
                    // Player is in sight if no obstacles are blocking the view
                    return true;
                }
            }
        }

        return false;
    }


    private bool CanHearPlayer()
    {
        return Vector3.Distance(transform.position, player.transform.position) < hearingDistance;
    }

    private bool IsPlayerInRange()
    {
        float distance = Vector3.Distance(transform.position, player.transform.position);
        return distance < 2f;
    }

    private bool ShouldFlee()
    {
        return GetComponent<Health>().GetCurrentHealth() < 30;
    }

    // Actions
    private Node.Status Wander()
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

        if (CanHearPlayer() || IsPlayerInSight())
        {
            state = ActionState.WORKING;
            return Node.Status.SUCCESS;
        }

        return Node.Status.RUNNING;
    }

    private Node.Status AttackPlayer()
    {
        if (timer > .8f)
        {
            timer = 0f;
            var health = player.GetComponent<Health>();
            int damageToDeal = damage + Random.Range(5, 10);

            if (!IsPlayerInRange())
                return Node.Status.FAILURE;
            
            
            damageToDeal = lightingManager.isNight() ? damageToDeal * 2 : damageToDeal; // Double damage at night
            health.TakeDamage(damageToDeal);

            if (health.GetCurrentHealth() <= 0)
            {
                state = ActionState.IDLE;
                return Node.Status.SUCCESS;
            }
        }

        if (!IsPlayerInRange())
        {
            state = ActionState.IDLE;
            return Node.Status.FAILURE;
        }

        return Node.Status.RUNNING;
    }

    private Node.Status BreakBlockInFront()
    {
        Vector3 blockPosition = transform.position + transform.forward;
        var generator = FindObjectOfType<BlockyTerrain>();
        if (generator.RemoveBlock(blockPosition))
        {
            state = ActionState.IDLE;
            return Node.Status.SUCCESS;
        }
        
        return Node.Status.FAILURE;
    }
    
    

    private Node.Status ChasePlayer()
    {
        agent.SetDestination(player.transform.position);

        // Check if the agent cannot find a complete path to the player or should flee
        if (agent.pathStatus == NavMeshPathStatus.PathPartial || ShouldFlee())
        {
            state = ActionState.IDLE;
            return Node.Status.FAILURE;
        }

        // If player is not in sight and not heard, start the timeout timer
        if (!IsPlayerInSight() && !CanHearPlayer())
        {
            chaseTimeout += Time.deltaTime;
            if (chaseTimeout >= chaseTimeoutDuration)
            {
                state = ActionState.IDLE;
                chaseTimeout = 0f; // Reset the timer
                return Node.Status.FAILURE;
            }
        }
        else
        {
            // If player is seen or heard, reset the timeout timer
            chaseTimeout = 0f;
        }

        // If player is in range, consider chase successful
        if (IsPlayerInRange())
        {
            state = ActionState.IDLE;
            return Node.Status.SUCCESS;
        }

        state = ActionState.WORKING;
        return Node.Status.RUNNING;
    }

    private Node.Status Flee()
    {
        Vector3 towardsPlayer = (player.transform.position - transform.position).normalized;
        agent.SetDestination(transform.position - towardsPlayer * fleeDistance);
        agent.isStopped = false;
        
        if (Vector3.Distance(transform.position, player.transform.position) > 10f)
        {
            state = ActionState.IDLE;
            return Node.Status.SUCCESS;
        }

        if (ShouldFlee())
        {
            state = ActionState.WORKING;
            return Node.Status.RUNNING;
        }
        
        return Node.Status.FAILURE;
    }
}
