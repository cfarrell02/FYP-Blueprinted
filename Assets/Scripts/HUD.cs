using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    private readonly String prototypeVersion = "prototype-2";

    // Reference the text mesh pro text object
    public TextMeshProUGUI text;
    public TextMeshProUGUI inventoryText; // Will be replaced with a UI element
    public Image healthBar;
    public Color fullHealthColor, lowHealthColor;

    public PlayerInventory playerInventoryObject;



    InventoryItem<Entity>[] inventory;
    private Canvas canvas;

    // Start is called before the first frame update
    void Start()
    {
        canvas = GetComponent<Canvas>();
        inventory = playerInventoryObject.getInventory();
        string buildInfo = "Build: " + Application.version + " Platform: " + Application.platform + " Unity: " + Application.unityVersion + " OS: " + SystemInfo.operatingSystem;
        buildInfo += "\n" + Application.productName + ", " + Application.companyName + ", " + prototypeVersion;
        text.text = buildInfo;

    }

// Update is called once per frame
    void Update()
    {
        string inventoryString = "Inventory: ";
    
        // Clear existing icons in the canvas
        foreach (Transform child in canvas.transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < inventory.Length; i++)
        {
            if (inventory[i].item != null)
            {
                var name = inventory[i].item.name;
                bool hasBeenRendered = false;
                var images = canvas.GetComponentsInChildren<Image>();
                foreach (var image in images)
                {
                    if (image.name == name)
                    {
                        hasBeenRendered = true;
                    }
                }
                if (hasBeenRendered)
                {
                    continue;
                }
                
                var icon = CreateGameObjectIcon(inventory[i].item.prefab, name);
                // Add the icon to the canvas
                icon.transform.SetParent(canvas.transform);
                icon.transform.position = new Vector3(100 + (i * 100), 100, 0);
                icon.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            }
        }
        inventoryText.text = inventoryString;

        // Rest of your code...
    }

    Image CreateGameObjectIcon(GameObject item, string iconName)
    {
        Image image = new GameObject(iconName).AddComponent<Image>();
        var renderer = item.GetComponent<Renderer>();
    
        if (renderer != null)
        {
            image.sprite = Sprite.Create(renderer.material.mainTexture as Texture2D, new Rect(0, 0, renderer.material.mainTexture.width, renderer.material.mainTexture.height), new Vector2(0.5f, 0.5f));
        }
        else
        {
            var childRenderer = item.GetComponentInChildren<Renderer>();
            image.sprite = Sprite.Create(childRenderer.material.mainTexture as Texture2D, new Rect(0, 0, childRenderer.material.mainTexture.width, childRenderer.material.mainTexture.height), new Vector2(0.5f, 0.5f));
        }
    
        return image;
    }

}
