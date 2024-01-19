using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Transform Playerpos;
    UnityEngine.AI.NavMeshAgent agent;
    public int health = 100;
    private int currentHealth;
    private Renderer rend;
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        agent.speed = 1f;
        currentHealth = health;
        rend = GetComponent<Renderer>();
        if (rend == null)
        {
            rend = GetComponentInChildren<Renderer>();
        }
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
        StartCoroutine(TurnCapsuleRed());
    }

    //coroutine method
    IEnumerator TurnCapsuleRed()
    {
        rend.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        rend.material.color = Color.white;
    }

    void Die()
    {
        print("Enemy died!");
        Destroy(gameObject);
    }
}