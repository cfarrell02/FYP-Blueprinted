using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Transform Playerpos;
    UnityEngine.AI.NavMeshAgent agent;
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
    }
    void Update()
    {

        agent.destination = Playerpos.position;

    }

    public void TakeDamage(int damage)
    {
        print("Enemy took " + damage + " damage");
        // Reduce the enemy's health by the damage amount.
        //health -= damage;

        // If the enemy has lost all it's health and the death flag hasn't been set yet...
        //if (health <= 0 && !isDead)
        //{
        //    // ... it should die.
        //    Death();
        //}
    }
}