using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class HUD : MonoBehaviour
{
    // Reference the text mesh pro text object
    public TextMeshProUGUI text;
    public TextMeshProUGUI inventoryText; // Will be replaced with a UI element

    public PlayerInventory playerInventoryObject;


    InventoryItem<Block>[] inventory;

    // Start is called before the first frame update
    void Start()
    {
        inventory = playerInventoryObject.getInventory();
        string buildInfo = "Build: " + Application.version + " Platform: " + Application.platform + " Unity: " + Application.unityVersion + " OS: " + SystemInfo.operatingSystem + " \nPrototype-1";
        text.text = buildInfo;

    }

    // Update is called once per frame
    void Update()
    {

        string inventoryString = "Inventory: ";
        for (int i = 0; i < inventory.Length; i++)
        {
            if (inventory[i].item.Name != null)
            {
                if(playerInventoryObject.GetSelectedBlockIndex() == i)
                {
                    inventoryString += "<color=red>";
                }
                inventoryString += inventory[i].item.Name + "(" + inventory[i].count + ") ";
                if (playerInventoryObject.GetSelectedBlockIndex() == i)
                {
                    inventoryString += "</color>";
                }
            }
        }
        inventoryText.text = inventoryString;

    }
}
