using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;

public class GameManager : MonoBehaviour
{
    // Singleton instance
    private static GameManager _instance;

    // Public property to access the instance
    //Singleton taken from stack overflow
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

    public bool inputEnabled = true;
    public bool craftingIsOpen = false;
    public bool isPaused = false;
    public int NightsSurvived { get; set; } = 0;
    
    private string leaderboardFilePath;
    public string savePath = "data/saves/";
    public string currentSaveFile;
    private BlockyTerrain generator;
    
    public Leaderboard leaderboardEntries = new Leaderboard(new List<LeaderboardEntry>());


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


        }

        leaderboardFilePath = "data/leaderboard/leaderboard.json";
    }
    
    
    private void Start()
    {
        LoadLeaderboard();
        
        //On application quit, save the game
        Application.quitting += () => SaveGame(currentSaveFile + ".data");
            
    }
    
    IEnumerator LateStart()
    {
        yield return new WaitForNextFrameUnit();
        var file = File.Exists(savePath + currentSaveFile + ".data");
        if (file)
        {
            LoadGame(currentSaveFile + ".data");
        }
        else
        {
            generator.GenerateInitialTerrain();
        }
        HUD hud = GameObject.Find("Canvas").GetComponent<HUD>();
        hud.TriggerLoadEnd();
    }


    private void Update()
    {
        if(!IsMainScene()) return;
        
        
        if (!generator )
        {
            print(IsMainScene());
            generator = GameObject.Find("Generator").GetComponent<BlockyTerrain>();
            StartCoroutine(LateStart());
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
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void IncreaseNightsSurvived()
    {
        NightsSurvived++;
    }
    
    public void TogglePause()
    {
        if (isPaused)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            inputEnabled = true;
            Time.timeScale = 1;
            isPaused = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            inputEnabled = false;
            Time.timeScale = 0;
            isPaused = true;
        }
    }
    
    public void ResetGame()
    {
        string dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string name = currentSaveFile == "" ? "New Game " + dateTime : currentSaveFile;
        
        
        var levelManager = GameObject.FindGameObjectWithTag("Player").GetComponent<LevelManager>();
        AddToLeaderboard(name, NightsSurvived, levelManager.GetCurrentXP());
        NightsSurvived = 0;
    }
    
    public void AddToLeaderboard(string name, int score, int xp = 0)
    {
        leaderboardEntries.entries.Add(new LeaderboardEntry(name, score, xp));
        leaderboardEntries.entries = leaderboardEntries.entries.OrderByDescending(e => e.night).ToList();
        SaveLeaderboard();
    }
    
    private void SaveLeaderboard()
    {
        string json = JsonUtility.ToJson(leaderboardEntries);
        print(json);
        File.WriteAllText(leaderboardFilePath, json);
    }
    
    private void LoadLeaderboard()
    {
        if (File.Exists(leaderboardFilePath))
        {
            string json = File.ReadAllText(leaderboardFilePath);
            leaderboardEntries = JsonUtility.FromJson<Leaderboard>(json);
        }
    }
    
    public void SaveGame(string path = "SaveGame.data")
    {
        if (!IsMainScene() || path==".data") return;
        print("Saving game to " + savePath + path);
        var pickups = GameObject.FindGameObjectsWithTag("Pickup");
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        var pickupsAndEnemies = pickups.Concat(enemies).ToArray();
        var playerTransform = GameObject.FindWithTag("Player").transform;
        var coordsToHeight = generator.GetHeightMap();
        int playerXP = playerTransform.GetComponent<LevelManager>().GetCurrentXP();

        Vector3 playerPos = playerTransform.position;
        Quaternion playerRot = playerTransform.rotation;
        var playerInventory = playerTransform.GetComponent<PlayerInventory>().GetInventory();
        var lightManager = GameObject.Find("LightingManager").GetComponent<LightingManager>();

        SaveData saveData = new SaveData(coordsToHeight, pickupsAndEnemies.ToList(), playerPos, playerInventory,
            lightManager.GetTimeOfDay(), NightsSurvived, playerRot, generator.frequency, playerXP);

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
        
        generator.frequency = saveData.GetMapScale();
        
        LoadEntitiesInScene(saveData);

        LoadPlayerState(saveData);
    }
    
    private void LoadEntitiesInScene(SaveData saveData)
    {
        //Remove all default spawned entities
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject[] pickups = GameObject.FindGameObjectsWithTag("Pickup");
        GameObject[] allEntities = enemies.Concat(pickups).ToArray();
        foreach (var entity in allEntities)
        {
            Destroy(entity);
        }
        foreach (var entity in saveData.GetEntitiesInScene())
        {
            if (entity.Item3 == "Enemy")
            {
                generator.enemyPrefab.InstantiateEnemy(entity.Item2);
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

    [Serializable]
    public struct LeaderboardEntry
    {
        public string name;
        public int night, xp;
        
        public LeaderboardEntry(string name, int night, int xp)
        {
            this.name = name;
            this.night = night;
            this.xp = xp;
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


}
