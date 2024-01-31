using System.Collections.Generic;
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
    public int nightsSurvived {get; set;} = 0;
    

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
    }
    
    public void IncreaseNightsSurvived()
    {
        nightsSurvived++;
    }
}