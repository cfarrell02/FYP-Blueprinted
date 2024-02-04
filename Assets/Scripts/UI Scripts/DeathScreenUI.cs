using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeathScreenUI : MonoBehaviour
{
    public Button startButton;
    public TextMeshProUGUI leaderboardText;

    // Start is called before the first frame update
    void Start()
    {
        //Show the cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        
        startButton.onClick.AddListener(NextLevel);

        var leaderboard = GameManager.Instance.leaderboard;
        leaderboardText.text = "Leaderboard\n";
        foreach (var entry in leaderboard.entries)
        {
            leaderboardText.text += $"{entry.name} - {entry.score}\n";
        }

    }

    void NextLevel()
    {
        // Get the current active scene index
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        // Load the next scene by incrementing the current scene index
        int nextSceneIndex = (currentSceneIndex + 1) % SceneManager.sceneCountInBuildSettings;
        
        SceneManager.LoadScene(nextSceneIndex);
    }
    

}