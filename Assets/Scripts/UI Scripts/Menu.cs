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
    public GameObject savesPanel; // Change Image to GameObject
    public TMP_InputField saveGameInput;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        startButton.onClick.AddListener(() => StartCoroutine(DisplaySplashScreen()));
        StartText.gameObject.SetActive(false);
        PopulateSaves();

    }

    void StartGame()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = (currentSceneIndex + 1) % SceneManager.sceneCountInBuildSettings;

        SceneManager.LoadScene(nextSceneIndex);
    }

    IEnumerator DisplaySplashScreen()
    {
        GameManager.Instance.currentSaveFile = saveGameInput.text;
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
            print("Save file: " + fileName);

            // Instantiate a new save button
            var saveButton = Instantiate(startButton, savesPanel.transform);

            // Set the position of the save button
            float yPos = savesPanel.transform.position.y - (i * buttonHeight) - 120;
            saveButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, yPos);

            // Set the text of the TextMeshProUGUI component
            saveButton.GetComponentInChildren<TextMeshProUGUI>().text = fileName;

            // Add a listener to the button for loading the save
            saveButton.GetComponent<Button>().onClick.AddListener(() => LoadSave(fileName));

            print("Adding save button for: " + fileName);
        }
    }


    void LoadSave(string saveData)
    {
        GameManager.Instance.currentSaveFile = saveData;
        StartGame();

    }
}
