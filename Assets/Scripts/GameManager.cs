using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Singleton instance
    private static GameManager _instance;

    // Public property to access the instance
    public static GameManager Instance
    {
        get
        {
            // If the instance doesn't exist, create one
            if (_instance == null)
            {
                GameObject singletonObject = new GameObject("GameManager");
                _instance = singletonObject.AddComponent<GameManager>();
            }

            return _instance;
        }
    }

    // GameManager variables and methods can go below this line
    
    [Tooltip("All entities in the game.")]
    public Entity[] allEntities;
    
    public bool InputEnabled = true;
    public int NightsSurvived { get; set; } = 0;
    
    private string leaderboardFilePath;
    
    public Leaderboard leaderboard = new Leaderboard(new List<LeaderboardEntry>());


    // Awake is called when the script instance is being loaded
// Awake is called when the script instance is being loaded
    private void Awake()
    {
        // Ensure there is only one instance of the GameManager
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);

            // Initialize the leaderboard with an empty list
            leaderboard = new Leaderboard(new List<LeaderboardEntry>());
        }

        leaderboardFilePath = "leaderboard.json";
    }

    
    private void Start()
    {
        LoadLeaderboard();
    }
    
    public void IncreaseNightsSurvived()
    {
        NightsSurvived++;
    }
    
    public void ResetGame()
    {
        string DateTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        
        AddToLeaderboard($"Player {DateTime}", NightsSurvived); // Name is hardcoded for now
        NightsSurvived = 0;
    }
    
    public void AddToLeaderboard(string name, int score)
    {
        leaderboard.entries.Add(new LeaderboardEntry(name, score));
        leaderboard.entries.Sort((x, y) => y.score.CompareTo(x.score));
        SaveLeaderboard();
    }
    
    private void SaveLeaderboard()
    {
        string json = JsonUtility.ToJson(leaderboard);
        print(json);
        File.WriteAllText(leaderboardFilePath, json);
    }
    
    private void LoadLeaderboard()
    {
        if (File.Exists(leaderboardFilePath))
        {
            string json = File.ReadAllText(leaderboardFilePath);
            leaderboard = JsonUtility.FromJson<Leaderboard>(json);
        }
    }

}
[Serializable]
public struct Leaderboard
{
    public List<LeaderboardEntry> entries;
    
    public Leaderboard(List<LeaderboardEntry> entries)
    {
        this.entries = entries;
    }
}

[Serializable]
public struct LeaderboardEntry
{
    public string name;
    public int score;
    
    public LeaderboardEntry(string name, int score)
    {
        this.name = name;
        this.score = score;
    }
}
