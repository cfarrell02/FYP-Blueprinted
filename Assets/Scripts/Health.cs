
using UnityEngine;
using UnityEngine.SceneManagement;

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
            GameManager.Instance.SaveGame(GameManager.Instance.currentSaveFile + "_dead.data");
            GameManager.Instance.ResetGame();
            
            // Get the current active scene index
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

            // Load the next scene by incrementing the current scene index
            int nextSceneIndex = (currentSceneIndex + 1) % SceneManager.sceneCountInBuildSettings;
    
            SceneManager.LoadScene(nextSceneIndex);
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
