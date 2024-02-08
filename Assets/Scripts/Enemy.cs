using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    const float DETECT_DISTANCE = 8f;
    public Behaviour behaviour;
    public float lookDistance = 30f;
    public int damage = 10;
    public enum Behaviour
    {
        Idle,
        Wander,
        Chase,
        Flee
    }

    private GameObject player;
    private NavMeshAgent agent;
    private Renderer rend;
    private float timer = 0f;
    private Animator anim;
    private float fleeDistance = 5f;
    private LightingManager lightingManager;
    // Start is called before the first frame update
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
    }
    void Update()
    {
        timer += Time.deltaTime;
        switch (behaviour)
        {
            case Behaviour.Idle:
                //Do nothing
                break;
            case Behaviour.Wander:
                Wander();
                break;
            case Behaviour.Chase:
                ChasePlayer();
                break;
            case Behaviour.Flee:
                Flee();
                break;
        }
        
        Look();
        Listen();
        
        if (GetComponent<Health>().GetCurrentHealth() < 20)
        {
            behaviour = Behaviour.Flee;
            
        }
        //TODO Add in undoing flee when player is far enough away and increasing health
        
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
        float distance = Vector3.Distance(transform.position, player.transform.position);
        if (distance < 2f)
        {
            AttackPlayer();
            return;
        }
        if (distance > 20f)
        {
            behaviour = Behaviour.Wander;
            return;
        }

        agent.SetDestination(player.transform.position);
        
        
    }

    void Die()
    {
        print("Enemy died!");
        Destroy(gameObject);
    }
    
    // Observation methods

    void Look()
    {
        var direction = (player.transform.position - transform.position).normalized;
        bool isInFOV = Vector3.Dot(transform.forward.normalized, direction) > 0.7f;
        Debug.DrawRay(transform.position, direction * lookDistance, Color.green);
        Debug.DrawRay(transform.position, transform.forward * lookDistance, Color.blue);
        Debug.DrawRay(transform.position, transform.forward * lookDistance, Color.blue);
        Debug.DrawRay(transform.position, (transform.forward - transform.right) * lookDistance, Color.red);
        Debug.DrawRay(transform.position, (transform.forward + transform.right) * lookDistance, Color.red);

        Ray ray = new Ray();
        RaycastHit hit;
        ray.origin = transform.position + Vector3.up * .7f;
        string objectInSight = "";
        lookDistance = 20f;
        ray.direction = transform.forward * lookDistance;
        Debug.DrawRay(ray.origin, ray.direction, Color.yellow);
        
        if(Physics.Raycast(ray.origin, direction, out hit, lookDistance))
        {
            if (objectInSight == "Player" || isInFOV)
            {
                anim.SetBool("canSeePlayer", true);
                behaviour = Behaviour.Chase;
            }
            else
            {
                anim.SetBool("canSeePlayer", false);
               // behaviour = Behaviour.Wander;
            }
        }
        
    }
    
    private void Flee()
    {
        float distance = Vector3.Distance(player.transform.position, transform.position);
        if (distance < fleeDistance)
        {
            Vector3 towardsPlayer = (player.transform.position - transform.position).normalized;
            agent.SetDestination(transform.position - towardsPlayer * fleeDistance);
            agent.isStopped = false;
        }
    }
    
    void Listen()
    {
        float distance = Vector3.Distance(player.transform.position, transform.position);
        if (distance < DETECT_DISTANCE)
        {
            anim.SetBool("canHearPlayer", true);
            behaviour = Behaviour.Chase;
        }
        else
        {
            anim.SetBool("canHearPlayer", false);
            //behaviour = Behaviour.Wander;
        } 
        
    }
}