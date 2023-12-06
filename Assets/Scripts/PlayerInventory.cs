using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{

    public int inventoryCapacity = 10;

    private int inventorySize = 0;
    Block[] inventory;
    private GameObject _lookedAtObject;
    private BlockyTerrain blockyTerrain;

    private void Awake()
    {
        inventory = new Block[inventoryCapacity];
    }
    

    // Start is called before the first frame update
    void Start()
    {
       // inventory = new GameObject[inventoryCapacity];
       blockyTerrain = FindObjectOfType<BlockyTerrain>(); // Will only be one instance of BlockyTerrain, hopefully
        
    }

    // Update is called once per frame
    void Update()
    {


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
            if (hit.collider.gameObject.GetComponent<Renderer>() == null)
                return;


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
                var block = blockyTerrain.getHeightMap()[new Vector2(_lookedAtObject.transform.position.x, _lookedAtObject.transform.position.z)];


                if (AddItem(block))
                {
                    bool success = blockyTerrain.RemoveBlock(block.Location);
                    print("Removed block: " + success);
                    Destroy(_lookedAtObject);
                }
            }
        }
    }

    string GetInventoryString()
    {
        string inventoryString = "Inventory: ";
        for (int i = 0; i < inventory.Length; i++)
        {
            if (inventory[i].Name != null )
            {
                inventoryString += inventory[i].Name + " ";
            }
        }
        return inventoryString;

    }

    bool AddItem(Block item)
    {
        if (inventorySize >= inventoryCapacity)
            return false;

        for (int freeIndex = 0; freeIndex < inventoryCapacity; freeIndex++)
        {
            if (inventory[freeIndex].Name == null)
            {
                inventory[freeIndex] = item; // Assign the reference to the inventory
                inventorySize++;
                return true;
            }
        }
        return false;
    }

    bool RemoveItem(int index)
    {
        if (inventory[index].Name == null)
            return false;

        inventory[index] = new Block();
        inventorySize--;
        return true;
    }

    public Block[] getInventory()
    {
        return inventory;
    }

    bool RemoveItem(Block item)
    {
        for(int itemIndex = 0; itemIndex< inventoryCapacity; ++itemIndex)
        {
            if (inventory[itemIndex].Equals(item))
            {
                inventory[itemIndex] = new Block(); // Assign the reference to the inventory
                inventorySize--;
                return true;
            }
        }
        return false;
    }


}

