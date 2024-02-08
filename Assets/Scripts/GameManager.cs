using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using Utils;

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
    public string savePath = "data/saves/";
    public string currentSaveFile;
    private BlockyTerrain generator;
    
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

        leaderboardFilePath = "data/leaderboard/leaderboard.json";
    }
    
    
    private void Start()
    {
        LoadLeaderboard();
    }

    private void Update()
    {
        if (!generator && IsMainScene() )
        {
            print(IsMainScene());
            generator = GameObject.Find("Generator").GetComponent<BlockyTerrain>();
        }
        
        //Temp load and save, will be moved to pause menu
        if (Input.GetKeyDown(KeyCode.O))
        {
            print("Saving terrain");
            SaveGame(currentSaveFile + ".data");
        }
        
        if (Input.GetKeyDown(KeyCode.P))
        {
            print("Loading terrain");
            LoadGame(currentSaveFile + ".data");
        }
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
    
    public void SaveGame(string path = "SaveGame.data")
    {
        if (!IsMainScene()) return;
        print("Saving game to " + savePath + path);
        var pickups = GameObject.FindGameObjectsWithTag("Pickup");
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        var pickupsAndEnemies = pickups.Concat(enemies).ToArray();
        var playerTransform = GameObject.FindWithTag("Player").transform;
        var coordsToHeight = generator.GetHeightMap();

        Vector3 playerPos = playerTransform.position;
        Quaternion playerRot = playerTransform.rotation;
        var playerInventory = playerTransform.GetComponent<PlayerInventory>().GetInventory();
        var lightManager = GameObject.Find("LightingManager").GetComponent<LightingManager>();

        SaveData saveData = new SaveData(coordsToHeight, pickupsAndEnemies.ToList(), playerPos, playerInventory,
            lightManager.GetTimeOfDay(), NightsSurvived, playerRot, generator.scale);

        //Save saveData as binary
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(savePath + path);
        bf.Serialize(file, saveData);
        file.Close();
    }

    private static bool IsMainScene()
    {
        //Ensure current scene is 'Main'
        return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Main";
    }

    public void LoadGame(string path = "SaveGame.data")
    {
        if (!IsMainScene()) return;
        
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(savePath + path, FileMode.Open);
        SaveData saveData = (SaveData) bf.Deserialize(file);
        file.Close();
        var coordsToHeightList = saveData.GetCoordsToHeightList();
        
        generator.DestroyAllCubes();
        generator.SetHeightMap(coordsToHeightList);
        var coordsToHeight = generator.GetHeightMap();

        // Reload everything
        foreach (var vb in coordsToHeight.Values.Where(vb => vb.isLoaded))
        {
            foreach (var block in vb.blocks.Where(b => b.isLoaded))
            {
                generator.InstantiateCube(block.location, block);
            }
        }
        
        generator.scale = saveData.GetMapScale();
        
        LoadEntitiesInScene(saveData);

        LoadPlayerState(saveData);
    }
    
    private void LoadEntitiesInScene(SaveData saveData)
    {
        foreach (var entity in saveData.GetEntitiesInScene())
        {
            if (entity.Item3 == "Enemy")
            {
                Instantiate(generator.enemyPrefab, entity.Item2, Quaternion.identity);
            }
            else if (entity.Item3 == "Item")
            {
                var item = GameManager.Instance.allEntities.First(e => e.name == entity.Item1);
                GameObject pickup = Instantiate(item.prefab, entity.Item2, Quaternion.identity);
                pickup.tag = "Pickup";
                Pickup p = pickup.AddComponent<Pickup>();
                p.item = item;
            }
        }
    }

    private void LoadPlayerState(SaveData saveData)
    {
        var playerPos = saveData.GetPlayerPosition();
        var playerRot = saveData.GetPlayerRotation();
        var playerInventory = saveData.GetInventory();
        var timeOfDay = saveData.GetTime();
        var nightsSurvived = saveData.GetNightsSurvived();
        var playerTransform = GameObject.FindWithTag("Player").transform;

        playerTransform.position = playerPos;
        playerTransform.rotation = playerRot;
        GameObject.Find("LightingManager").GetComponent<LightingManager>().SetTimeOfDay(timeOfDay);
        NightsSurvived = nightsSurvived;

        var playerInventoryItem = playerTransform.GetComponent<PlayerInventory>();
        playerInventoryItem.SetInventory(playerInventory.ToArray());

        var canvas = GameObject.Find("Canvas");
        canvas.GetComponent<HUD>().SetPlayerInventory(playerInventoryItem);
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
