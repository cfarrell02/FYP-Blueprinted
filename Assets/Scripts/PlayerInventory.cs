using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{

    public int inventoryCapacity = 10;

    private int inventorySize = 0;
    GameObject[] inventory;
    private GameObject _lookedAtObject;

    // Start is called before the first frame update
    void Start()
    {
        inventory = new GameObject[inventoryCapacity];
        
    }

    // Update is called once per frame
    void Update()
    {
        print("Inventory: ");
        print(GetInventoryString());

        // Raycast to see what block is in front of the player
        RaycastHit hit;
        Transform cameraTransform = Camera.main.transform;
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

        if (Physics.Raycast(ray, out hit, 5f))
        {
            if (_lookedAtObject != null && _lookedAtObject != hit.collider.gameObject)
            {
                _lookedAtObject.GetComponent<Renderer>().material.color = Color.white;
            }

            _lookedAtObject = hit.collider.gameObject;
            _lookedAtObject.GetComponent<Renderer>().material.color = Color.red;
        }
        else
        {
            if (_lookedAtObject != null)
            {
                _lookedAtObject.GetComponent<Renderer>().material.color = Color.white;
                _lookedAtObject = null;
            }
        }

        // on click, add the block to the inventory
        if (Input.GetMouseButtonDown(0))
        {
            if (_lookedAtObject != null)
            {
                if (AddItem(_lookedAtObject))
                {
                    Destroy(_lookedAtObject);
                }
            }
        }
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
