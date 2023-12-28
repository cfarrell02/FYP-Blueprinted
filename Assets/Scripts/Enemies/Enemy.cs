using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Transform Playerpos;
    UnityEngine.AI.NavMeshAgent agent;
    public int health = 100;
    private int currentHealth;
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        currentHealth = health;
    }
    void Update()
    {

        agent.destination = Playerpos.position;

    }

    public void TakeDamage(int damage)
    {
        print("Enemy took " + damage + " damage");
        // Reduce the enemy's health by the damage amount.
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        print("Enemy died!");
        Destroy(gameObject);
    }
}