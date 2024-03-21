using UnityEngine;

public class LevelManager : MonoBehaviour
{
    // Experience points required for each level
    public int[] xpThresholds;

    // Current player level and XP
    private int currentLevel = 1;
    [SerializeField, Range(0, 1000)]
    public int currentXP = 0;
    
    private float difficultyTimer = 0;
    public float difficultyCheckInterval = 10;

    
    //References to other scripts
    private WeatherManager weatherManager;
    private GameObject player;
    private BlockyTerrain terrain;
    
    
    
    // Method to gain XP
    public void GainXP(int xpAmount)
    {
        currentXP += xpAmount;
        
        // Check if the player leveled up
        while (currentLevel < xpThresholds.Length && currentXP >= xpThresholds[currentLevel - 1])
        {
            LevelUp();
        }
    }

    // Method to level up
    private void LevelUp()
    {
        currentLevel++;
        Debug.Log("Level up! You reached level " + currentLevel + "!");
    }

    // Method to retrieve current player level
    public int GetCurrentLevel()
    {
        return currentLevel;
    }

    // Method to retrieve current player XP
    public int GetCurrentXP()
    {
        return currentXP;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Initialize xpThresholds array if not provided in the Inspector
        if (xpThresholds == null || xpThresholds.Length == 0)
        {
            Debug.LogWarning("XP thresholds not set! Default thresholds will be used.");
            InitializeDefaultXPThresholds();
        }
        
        player = GameObject.FindGameObjectWithTag("Player");
        weatherManager = FindObjectOfType<WeatherManager>();
        terrain = FindObjectOfType<BlockyTerrain>();
    }

    // Initialize default XP thresholds (if not provided in the Inspector)
    private void InitializeDefaultXPThresholds()
    {
        // Default thresholds for levels 1 to 10
        xpThresholds = new int[] { 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000 };
    }
    
    void Update()
    {
        difficultyTimer += Time.deltaTime;
        if (difficultyTimer >= difficultyCheckInterval)
        {
            // Increase difficulty
            AdjustDynamicDifficulty();
            difficultyTimer = 0;
        }

    }

    private void AdjustDynamicDifficulty()
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var enemy in enemies)
        {
            var enemyScript = enemy.GetComponent<EnemyBehaviour>();
            enemyScript.ScaleBasedOnLevel(currentLevel);
        }
        
        weatherManager.ScaleBasedOnLevel(currentLevel);
        
        player.GetComponent<Health>().ScaleBasedOnLevel(currentLevel);
        
        terrain.ScaleBasedOnLevel(currentLevel);

    }
}