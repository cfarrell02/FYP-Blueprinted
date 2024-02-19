
using UnityEngine;

public class Health : MonoBehaviour
{
    public int currentHealth;
    public int maxHealth = 100;
    private bool isPlayer;
    
    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
        isPlayer = gameObject.CompareTag("Player");
    }
    
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
        
    }
    
    public void SetHealth(int health)
    {
        currentHealth = health;
        
        if(currentHealth > maxHealth)
        {
            maxHealth = currentHealth;
        }
    }
    
    void Die()
    {
        if (isPlayer)
        {
            Debug.Log("You died"); //This is handled elsewhere
        }
        else
        {
            LevelManager levelManager = FindObjectOfType<LevelManager>();
            levelManager.GainXP(10);
            Destroy(gameObject);
        }
    }
    
    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }
    
    public int GetCurrentHealth()
    {
        return currentHealth;
    }
    
    
}
