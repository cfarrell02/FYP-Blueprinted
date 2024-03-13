using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public Button startButton, openLoadScreen, quitButton, loadBackButton,
        settingsButton, settingsBackButton, leaderboardButton, leaderboardBackButton;
    public TextMeshProUGUI StartText, LeaderboardText;
    public GameObject savesPanel; // Change Image to GameObject
    public TMP_InputField saveGameInput;
    
    
    
    //Settings
    public Slider volumeSlider;
    public AudioMixer audioMixer;

    
    private Animator animator;
    List<string> existingSaves = new List<string>();
List<Dictionary<string, object>> leaderboard = new List<Dictionary<string, object>>();
    
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        animator = GetComponent<Animator>();

        startButton.onClick.AddListener(() => StartCoroutine(DisplaySplashScreen()));
        openLoadScreen.onClick.AddListener(() => animator.SetBool("LoadScreen", true));
        quitButton.onClick.AddListener(() => Application.Quit());
        loadBackButton.onClick.AddListener(() => animator.SetBool("LoadScreen", false));
        settingsButton.onClick.AddListener(() => animator.SetBool("SettingsScreen", true));
        settingsBackButton.onClick.AddListener(() => animator.SetBool("SettingsScreen", false));
        leaderboardButton.onClick.AddListener(() => animator.SetBool("LeaderboardScreen", true));
        leaderboardBackButton.onClick.AddListener(() => animator.SetBool("LeaderboardScreen", false));
        
        StartText.gameObject.SetActive(false);
        PopulateSaves();
        
        
        volumeSlider.onValueChanged.AddListener((value) =>
        {
           audioMixer.SetFloat("SoundsVolume", value);
           print("Volume: " + value);
        });

    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        
        PopulateLeaderboard();
    }

    void StartGame()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = (currentSceneIndex + 1) % SceneManager.sceneCountInBuildSettings;

        SceneManager.LoadScene(nextSceneIndex);
    }
    
    void DeleteSave(string saveData)
    {
        System.IO.File.Delete("data/saves/" + saveData + ".data");
        PopulateSaves();
    }

    IEnumerator DisplaySplashScreen(bool newGame = true)
    {
        
    if (newGame)
    {
        

        if (existingSaves.Contains(saveGameInput.text))
        {
            print("Save already exists");
            yield break;
        }

        if (saveGameInput.text == "")
        {
            print("Save name cannot be empty");
            yield break;
        }
    }

    if(newGame)
        GameManager.Instance.currentSaveFile = saveGameInput.text;
    
    
        var allGameObjects = FindObjectsOfType<GameObject>();
        var uiItems = allGameObjects.Where(go => go.layer == 5).ToList();

        foreach (var uiItem in uiItems)
        {
            if (uiItem.name == "Canvas" || uiItem.name == "Background")
                continue;
            uiItem.SetActive(false);
        }

        StartText.gameObject.SetActive(true);
        yield return new WaitForSeconds(8);
        StartGame();
    }

    void PopulateLeaderboard()
    {
        
        if(leaderboard == null || leaderboard.Count == 0)
        {
            StartCoroutine(GameManager.Instance.firestoreManager.RetrieveDataFromFirestore("users", (data) =>
                {
                    if (data == null)
                    {
                        print("Failed to retrieve leaderboard data");
                        return;
                    }
                    leaderboard = data;
                })
            );
            
            LeaderboardText.text = "No entries yet";
            return;
        }
        
        leaderboard = leaderboard.OrderByDescending(x => x["nights_survived"]).ToList();
        
        string leaderboardText = "";
        foreach (var entry in leaderboard)
            
        {
            leaderboardText += $"{entry["name"]} ({entry["xp"]}) - {entry["nights_survived"]} nights survived\n";
        }
        LeaderboardText.text = leaderboardText;
    }

    void PopulateSaves()
    {
        // Get all the save files in the saves directory
        string[] saves = System.IO.Directory.GetFiles("data/saves");
        print("Populating saves: " + saves.Length);

        // Clear existing buttons in the savesPanel
        foreach (Transform child in savesPanel.transform)
        {
            Destroy(child.gameObject);
        }

        // Get the height of a single save button
        float buttonHeight = startButton.GetComponent<RectTransform>().rect.height;

        for (int i = 0; i < saves.Length; i++)
        {
            var save = saves[i];
            var fileName = System.IO.Path.GetFileName(save).Split('.')[0]; // Remove the file extension
            
            if(fileName == "") continue;
            existingSaves.Add(fileName);
            print("Save file: " + fileName);
            

            // Instantiate a new save button
            var saveButton = Instantiate(startButton, savesPanel.transform);
            var deleteButton = Instantiate(startButton, savesPanel.transform);

            // Set the position of the save button
            float yPos = savesPanel.transform.position.y - (i * buttonHeight) - savesPanel.transform.position.y*0.5f;
            saveButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, yPos);
            deleteButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(160, yPos);

            // Set the text of the TextMeshProUGUI component
            saveButton.GetComponentInChildren<TextMeshProUGUI>().text = fileName;
            
            deleteButton.GetComponentInChildren<TextMeshProUGUI>().text = "Delete";

            // Add a listener to the button for loading the save
            saveButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                LoadSave(fileName);
            });
            deleteButton.GetComponent<Button>().onClick.AddListener(() => DeleteSave(fileName));

            print("Adding save button for: " + fileName);
        }
    }


    void LoadSave(string saveData)
    {
        print("Loading save: " + saveData);
        GameManager.Instance.currentSaveFile = saveData;
        StartCoroutine(DisplaySplashScreen(false));

    }
}
