using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public Button startButton;
    public TextMeshProUGUI StartText;

    // Start is called before the first frame update
    void Start()
    {
        //Show the cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        startButton.onClick.AddListener(() => StartCoroutine(DisplaySplashScreen()));
        StartText.gameObject.SetActive(false);
    }

    void StartGame()
    {
        // Get the current active scene index
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        // Load the next scene by incrementing the current scene index
        int nextSceneIndex = (currentSceneIndex + 1) % SceneManager.sceneCountInBuildSettings;
        
        SceneManager.LoadScene(nextSceneIndex);
    }
    
    IEnumerator DisplaySplashScreen()
    {
        //Find all items on ui layer
        var allGameObjects = FindObjectsOfType<GameObject>();
        var uiItems = allGameObjects.Where(go => go.layer == 5).ToList();
        
        foreach (var uiItem in uiItems)
        {
            if (uiItem.name == "Canvas")
                continue;
            
            uiItem.SetActive(false);
        }
        
        StartText.gameObject.SetActive(true);
        yield return new WaitForSeconds(3);
        StartGame();
    }
}