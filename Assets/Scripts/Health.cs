
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Health : MonoBehaviour
{
    public int currentHealth;
    public int maxHealth = 100;
    private bool isPlayer;
    Vector3 previousPosition, currentPosition;
    public AudioClip damageSound;
    
    float time = 0, rehealPercentage = 0.05f;
    
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
        
        if (isPlayer)
        {
            AudioSource audioSource = GetComponent<AudioSource>();
            audioSource.PlayOneShot(damageSound);
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
            //GameManager.Instance.SaveGame(GameManager.Instance.GetSavePath() + "_dead.data");
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

    private void Update()
    {
        if (currentHealth < maxHealth)
        {
            time += Time.deltaTime;
            if (time > 5)
            {
                time = 0;
                Heal((int) (maxHealth * rehealPercentage));
            }
        }
        
    }

    public void ScaleBasedOnLevel(int currentLevel)
    {
        rehealPercentage = 0.05f - (currentLevel * 0.01f);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Projectile"))
        {
            string damage = other.gameObject.name.Split('_')[1];
            TakeDamage(int.Parse(damage));
        }
    }
}
