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
    public float speed = 4f;
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
    
    
    private float flyTimeout = 0f;
    private float flyTimeoutDuration = 5f; // Adjust this value as needed

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
        
        ScaleBasedOnLevel();
        
    }
    
    
    void ScaleBasedOnLevel()
    {
        //Scale up health
        var health = GetComponent<Health>();
        health.SetHealth((int)(health.GetCurrentHealth() * (1+playerLevel * 0.1f)));
        
        //Scale up damage
        damage = (int)(damage * (1+playerLevel * 0.1f));
        
        //Scale up speed
        agent.speed = speed * (1+playerLevel * 0.1f);
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
        


        Leaf attackLeaf = new Leaf("Attack", AttackPlayer);
        Inverter attackInverter = new Inverter("Invert");
        attackInverter.AddChild(attackLeaf);
        
        attackPlayer.AddChild(chaseLeaf);
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
        float distance = Vector3.Distance(transform.position - Vector3.up*.4f, player.transform.position);
        return distance < 1.2f;
    }

    private bool ShouldFlee()
    {
        return GetComponent<Health>().GetCurrentHealth() < 30;
    }

    // Actions
    private Node.Status Wander()
    {
        agent.isStopped = false;
        Debug.Log("Wandering");
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
        agent.isStopped = true;
        agent.SetDestination(transform.position);
        Debug.Log("Attacking player");
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

    private void BreakBlockInFront()
    {
        // agent.SetDestination(player.transform.position);
        // // Check if the agent is close to the player or if the player is in range
        // if ((agent.remainingDistance > 1f || IsPlayerInRange()) && flyTimeout < flyTimeoutDuration)
        // {
        //     state = ActionState.IDLE;
        //     flyTimeout = 0f;
        //     return;
        //     //The player is out of reach but not out of sight
        // }
        //
        flyTimeout += Time.deltaTime;
        if(flyTimeout < flyTimeoutDuration)
        {
            state = ActionState.IDLE;
            return;
        }
        agent.enabled = false;
        


    
        // Move towards the player (fly)
        transform.LookAt(player.transform);
        transform.Translate(Vector3.forward * Time.deltaTime * 2f);

        // Check if the enemy has reached the player
        if (Vector3.Distance(transform.position, player.transform.position) < .5f)
        {
            agent.enabled = true;
            flyTimeout = 0f;
            state = ActionState.IDLE;
        }
        
    }

    

    private Node.Status ChasePlayer()
    {
        Debug.Log($"Chasing the player, path partial? {agent.pathStatus} and distance to destination: {agent.remainingDistance}");
        agent.isStopped = false;
        agent.SetDestination(player.transform.position);

        // Reset the chase timeout timer if player is seen or heard
        if (IsPlayerInSight() || CanHearPlayer())
        {
            chaseTimeout = 0f;
            
        }
        else
        {
            // Increment the timeout timer
            chaseTimeout += Time.deltaTime;
        
            // If timed out, stop chasing
            if (chaseTimeout >= chaseTimeoutDuration)
            {
                chaseTimeout = 0f;
                state = ActionState.IDLE;
                return Node.Status.FAILURE;
            }
        }

        //Check if the agent's path is blocked or if the player is out of reach but not out of sight
        if (agent.remainingDistance < 1f && !IsPlayerInRange() && Vector3.Distance(agent.pathEndPosition, player.transform.position) > 1f)
        {
            flyTimeout += Time.deltaTime;
            if(flyTimeout > flyTimeoutDuration)
            {
                flyTimeout = 0f;
                transform.position = player.transform.position + Vector3.up;
                return Node.Status.RUNNING;
            }
            return Node.Status.RUNNING;
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

        // Continue chasing
        state = ActionState.WORKING;
        return Node.Status.RUNNING;
    }


    private Node.Status Flee()
    {
        Debug.Log("Fleeing");
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
