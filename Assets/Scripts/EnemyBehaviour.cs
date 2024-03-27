using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static Utils.Utils;

public class EnemyBehaviour : MonoBehaviour
{
    public enum Type
    {
        Melee, Ranged
    }
    

    // Enemy properties
    public float lookDistance = 30f;
    public float hearingDistance = 10f;
    public int damage = 10; 
    public float speed = 4f;
    public float fleeDistance = 5f;
    public float fieldOfView = 90f;

    // Components
    private GameObject player;
    private NavMeshAgent agent;
    private Renderer rend;
    private float timer = 0f;
    private LightingManager lightingManager;
    
    // Timer for timeout on chasing
    private float chaseTimeout = 0f;
    private float chaseTimeoutDuration = 5f; // Adjust this value as needed
    
    
    private float flyTimeout = 0f;
    private float flyTimeoutDuration = 5f; // Adjust this value as needed
    
    public Type type;
    private bool shot = false;
    public float shootInterval = 1f;
    public GameObject projectilePrefab, projectileSpawnPoint;
    [Range(0, 100)]
    public float projectileSpeed = 40f;
    

    // Behaviour tree stuff
    BehaviourTree tree;
    enum ActionState { IDLE, WORKING };
    ActionState state = ActionState.IDLE;


    private Animator animator;
    private AnimatorStateInfo info;
    private Health health;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        health = GetComponent<Health>();
        
        // Component initialization
        agent = GetComponent<NavMeshAgent>();
        agent.speed = speed;
        rend = GetComponent<Renderer>();
        lightingManager = FindObjectOfType<LightingManager>();
        player = GameObject.FindGameObjectWithTag("Player");
        if (rend == null)
            rend = GetComponentInChildren<Renderer>();

        // Behaviour tree initialization
        InitializeBehaviourTree();
        
     //   ScaleBasedOnLevel();
     
     
        if(type == Type.Ranged)
        {
            animator.SetFloat("throwSpeed", shootInterval);
        }
        
    }
    
    
    public void ScaleBasedOnLevel(int level)
    {
        //Scale up health
        var health = GetComponent<Health>();
        health.SetHealth((int)(health.GetCurrentHealth() * (1+level * 0.1f)));
        
        //Scale up damage
        damage = (int)(damage * (1+level * 0.1f));
        
        //Scale up speed
        agent.speed = speed * (1+level * 0.1f);
    }


    void Update()
    {
        if(animator)
            info = animator.GetCurrentAnimatorStateInfo(0);
        
        timer += Time.deltaTime;
        tree.Process();
        
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 1f, LayerMask.GetMask("Navmesh")))
        {
            if (hit.collider.CompareTag("Snow"))
            {
                agent.speed = speed / 2;
            }
            else
            {
                agent.speed = speed;
            }
        }
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
        Sequence wander = new Sequence("Flee");

        Leaf fleeLeaf = new Leaf("Flee", Flee);
        wander.AddChild(fleeLeaf);

        Leaf wanderLeaf = new Leaf("Wander", Wander);
        wander.AddChild(wanderLeaf);

        // Root selector
        Selector root = new Selector("Root");

        root.AddChild(attackPlayer);
        root.AddChild(wander);

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
            // Calculate the angle between the direction to the player and the forward direction of the object
            float angleToPlayer = Vector3.Angle(direction, transform.forward);

            // Check if the angle is within the field of view
            if (angleToPlayer <= fieldOfView * 0.5f)
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
        }

        return false;
    }



    private bool CanHearPlayer()
    {
        return Vector3.Distance(transform.position, player.transform.position) < hearingDistance;
    }

    private bool IsPlayerInRange()
    {
        //Check if player is touching the damage box
        var damageBox = transform.GetChild(0).GetComponent<Collider>();
        
        return damageBox.bounds.Intersects(player.GetComponent<Collider>().bounds);
    }
    
    private bool IsPlayerInRangedAttackRange()
    {
        float distance = Vector3.Distance(transform.position, player.transform.position);
        return distance < 15f && distance > 5f;
    }

    private bool ShouldFlee()
    {        
        bool tooCloseToRanged = type == Type.Ranged && Vector3.Distance(transform.position, player.transform.position) < 6f;

        return GetComponent<Health>().GetCurrentHealth() < 30 || tooCloseToRanged;
    }

    // Actions
    private Node.Status Wander()
    {
        
        print(Vector3.Distance(agent.destination, transform.position));
        if(Vector3.Distance(agent.destination, transform.position) < 2f)
        {
            animator.SetBool("isWalking", false);

        }
        else
        {
            animator.SetBool("isWalking", true);

        }
        agent.isStopped = false;
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
        animator.SetBool("isWalking", false);
        animator.SetBool("isAttacking", true);
        var playerHealth = player.GetComponent<Health>();
        if (type == Type.Melee)
        {

            agent.isStopped = true;
            agent.SetDestination(transform.position);
            if (timer > .8f)
            {
                timer = 0f;
                int damageToDeal = damage + Random.Range(5, 10);

                if (!IsPlayerInRange())
                    return Node.Status.FAILURE;


                damageToDeal = lightingManager.isNight() ? damageToDeal * 2 : damageToDeal; // Double damage at night
                playerHealth.TakeDamage(damageToDeal);

                if (playerHealth.GetCurrentHealth() <= 0)
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
        else
        {
            agent.isStopped = true;
            if (!IsPlayerInRangedAttackRange())
            {
                state = ActionState.IDLE;
                return Node.Status.FAILURE;
            }
            
            
            // Look at player but only on the y axis
            transform.LookAt(new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z));
            float progress = info.normalizedTime % shootInterval;
            if (progress > .9f && !shot)
            {
                shot = true;
                var projectile = Instantiate(projectilePrefab, projectileSpawnPoint.transform.position + transform.forward, Quaternion.identity);
                var target = player.transform.GetChild(4).transform.position;
                projectile.name = $"Projectile_{damage}";
                projectile.tag = "Projectile";
                projectile.transform.LookAt(target);
                projectile.GetComponent<Rigidbody>().AddForce((target - transform.position).normalized * projectileSpeed, ForceMode.Impulse);
                Destroy(projectile, 5f);
            }
            if (progress < .5f)
            {
                shot = false;
            }
            
            
            
            if (playerHealth.GetCurrentHealth() <= 0)
            {
                state = ActionState.IDLE;
                return Node.Status.SUCCESS;
            }
            
            return Node.Status.RUNNING;
            
        }
        
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
        animator.SetBool("isAttacking", false);
        animator.SetBool("isWalking", true);
        //Debug.Log($"Chasing the player, path partial? {agent.pathStatus} and distance to destination: {agent.remainingDistance}");
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
            if (chaseTimeout >= chaseTimeoutDuration )
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
                agent.enabled = false;
                transform.position = player.transform.position + Camera.main.transform.forward * 2f + Vector3.up * 2f;
                agent.enabled = true;
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
        bool inRange = type == Type.Melee ? IsPlayerInRange() : IsPlayerInRangedAttackRange();
        
        if (inRange)
        {
            state = ActionState.IDLE;
            return Node.Status.SUCCESS;
        }
        
        bool tooCloseToRanged = type == Type.Ranged && Vector3.Distance(transform.position, player.transform.position) < 6f;
        
        if(health.GetCurrentHealth() < 30 || tooCloseToRanged)
        {
            state = ActionState.IDLE;
            return Node.Status.FAILURE;
        }

        // Continue chasing
        state = ActionState.WORKING;
        return Node.Status.RUNNING;
    }


    private Node.Status Flee()
    {
        animator.SetBool("isWalking", true);
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
    
    public void TakeKnockback(Vector3 direction, float force=10)
    {
        agent.enabled = false;
        var rb = transform.GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.AddForce(direction * force , ForceMode.Impulse);
        StartCoroutine(PerformFunctionAfterDelay(1f, () =>
        {
            transform.GetComponent<Rigidbody>().isKinematic = true;
            agent.enabled = true;
        }));
        agent.enabled = true;
    }
}
