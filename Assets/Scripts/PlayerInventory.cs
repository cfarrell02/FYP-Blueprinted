using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{

    public int inventoryCapacity = 10;

    private int inventorySize = 0;
    GameObject[] inventory;

    // Start is called before the first frame update
    void Start()
    {
        inventory = new GameObject[inventoryCapacity];
        
    }

    // Update is called once per frame
    void Update()
    {
        print(GetInventoryString());
        
    }

    string GetInventoryString()
    {
        string str = "";
        foreach (GameObject item in inventory)
        {
            if(item != null)
            str += item.name + "\n";
        }
        return str;
    }

    bool AddItem(GameObject item)
    {
        if (inventorySize >= inventoryCapacity)
            return false;

        for(int freeIndex = 0; freeIndex < inventoryCapacity; freeIndex++)
        {
            if (inventory[freeIndex] == null)
            {
                inventory[freeIndex] = item;
                inventorySize++;
                return true;
            }
        }
        return false;

    }

    bool RemoveItem(int index)
    {
        if (inventory[index] == null)
            return false;

        inventory[index] = null;
        inventorySize--;
        return true;
    }

    bool RemoveItem(GameObject item)
    {
        for(int itemIndex = 0; itemIndex< inventoryCapacity; ++itemIndex)
        {
            if (inventory[itemIndex] == item)
            {
                inventory[itemIndex] = null;
                inventorySize--;
                return true;
            }
        }
        return false;
    }
}
