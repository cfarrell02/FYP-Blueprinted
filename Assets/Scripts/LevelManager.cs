using UnityEngine;

public class LevelManager : MonoBehaviour
{
    // Experience points required for each level
    public int[] xpThresholds;

    // Current player level and XP
    private int currentLevel = 1;
    [SerializeField, Range(0, 1000)]
    public int currentXP = 0;

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
    }

    // Initialize default XP thresholds (if not provided in the Inspector)
    private void InitializeDefaultXPThresholds()
    {
        // Default thresholds for levels 1 to 10
        xpThresholds = new int[] { 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000 };
    }
    
    void Update()
    {
        // if (Input.GetKeyDown(KeyCode.Space))
        // {
        //     GainXP(50);
        // }
    }
}