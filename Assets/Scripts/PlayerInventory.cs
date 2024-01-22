using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem.HID;

public struct InventoryItem<T>
{
    public T item { get; set;}
    public int count { get; set; }

    public InventoryItem(T item, int count)
    {
        this.item = item;
        this.count = count;
    }
}


public class PlayerInventory : MonoBehaviour
{

    public int inventoryCapacity = 10;
    [SerializeField] 
    private Entity[] startingItems;

    [SerializeField]
    private int playerHealth = 100;

    private int currentHealth;

    private int inventorySize = 0;
    private InventoryItem<Entity>[] inventory;
    private GameObject _lookedAtObject;
    private BlockyTerrain blockyTerrain;
    private GameObject mainCamera;
    private GameObject renderedObject;

    int selectedBlockIndex = 0;

    private void Awake()
    {
        inventory = new InventoryItem<Entity>[inventoryCapacity];
    }
    

    // Start is called before the first frame update
    void Start()
    {
       // inventory = new GameObject[inventoryCapacity];
       blockyTerrain = FindObjectOfType<BlockyTerrain>(); // Will only be one instance of BlockyTerrain, hopefully
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera");

        
        selectedBlockIndex = 0;
        currentHealth = playerHealth;
        
        for (int i = 0; i < startingItems.Length; i++)
        {
            AddItem(startingItems[i]);
        }
        

        
    }

    void Update()
    {
        CheckBlockInFront();
        HandleScrollWheel();
        UseSelectedTool();
        HandleHealth();
        RenderSelectedItem();
        
        if (Input.GetKeyDown(KeyCode.Q))
        {
            DropHeldItem();
        }

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Pickup"))
        {
            var pickup = other.gameObject.GetComponent<Pickup>();
            
            AddItem(pickup.item);
            Destroy(other.gameObject);
        }
    
        if (other.gameObject.tag == "Enemy")
        {
            currentHealth -= 10;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {

    }

    void CheckBlockInFront()
    {
    
        RaycastHit hit;
        Transform cameraTransform = Camera.main.transform;
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
    
        if (Physics.Raycast(ray, out hit, 5f))
        {
            if (_lookedAtObject != null && _lookedAtObject != hit.collider.gameObject)
            {
                ChangeBlockColor(_lookedAtObject, new Color(.2703f,.6601f,.8773f,1));
            }
    
            if (hit.collider.gameObject.GetComponent<Renderer>() == null || hit.collider.gameObject.tag != "Cube")
                return;
    
            _lookedAtObject = hit.collider.gameObject;
            ChangeBlockColor(_lookedAtObject, Color.red);
        }
        else
        {
            if (_lookedAtObject != null)
            {
                ChangeBlockColor(_lookedAtObject, new Color(.2703f, .6601f, .8773f, 1));
                _lookedAtObject = null;
            }
        }
    }

    void HandleHealth()
    {
        if (currentHealth <= 0)
        {
            print("Player died!");
            //TODO: Add death screen
        }
    }



    void HandleScrollWheel()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            selectedBlockIndex++;
            if (selectedBlockIndex >= inventorySize)
                selectedBlockIndex = 0;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            selectedBlockIndex--;
            if (selectedBlockIndex < 0)
                selectedBlockIndex = inventorySize - 1;
        }
    }

    void RenderSelectedItem()
    {
        var selectedItem = inventory[selectedBlockIndex];

        //if (selectedItem.item is Block)
        
        // Instantiate block as selected
        var selectedBlock = inventory[selectedBlockIndex];
        if (selectedBlock.item != null && selectedBlock.item.prefab)
        {
            if (renderedObject)
                Destroy(renderedObject);

        // Assuming selectedItem.item.prefab is the prefab you want to instantiate

        Vector3 offset = selectedItem.item.renderOffset;

        // Calculate the position based on the camera's position and orientation
        Vector3 placeOffset = Camera.main.transform.forward * offset.z + Camera.main.transform.up * offset.y + Camera.main.transform.right * offset.x;
        Vector3 newPosition = Camera.main.transform.position + placeOffset;

        // Instantiate the object at the calculated position and face it towards the camera
        renderedObject = Instantiate(selectedItem.item.prefab, newPosition, Quaternion.LookRotation(Camera.main.transform.forward));

        // Set the object as a child of the camera
        renderedObject.transform.parent = mainCamera.transform;

        // Set the layer (if needed)
        renderedObject.layer = 7;

               
            var collider = renderedObject.GetComponent<Collider>();
            if (collider)
                collider.enabled = false;

            renderedObject.transform.localScale *= 0.5f;


        }

        else
        {
            if (renderedObject)
                Destroy(renderedObject);
            renderedObject = null;
        }
    }


    void UseSelectedTool()
    {
        if (Input.GetMouseButtonDown(0))
        {

            var selectedItem = inventory[selectedBlockIndex];

            if (selectedItem.item is Item)
            {
                var tool = (Item)selectedItem.item;
                tool.Update();

                if (tool.itemType == Item.ItemType.Blueprint)
                {
                    AddBlockToInventory(); // Blueprint is a tool that adds blocks to the inventory
                    return;
                }
                bool toDelete = tool.Use(); // Other tools are used when the player clicks
                if (toDelete)
                {
                    RemoveItem(selectedBlockIndex);
                }
            }
            else if (inventory[selectedBlockIndex].item is Block)
                PlaceBlockFromInventory(); // Blocks are placed when the player clicks
        }
    }

    void AddBlockToInventory()
    {
        if (_lookedAtObject)
        {
            var block = blockyTerrain.FindBlock(_lookedAtObject.transform.position);
            
            
            if(!(inventory[selectedBlockIndex].item is Item && ((Item)inventory[selectedBlockIndex].item).itemType == Item.ItemType.Blueprint))
            {
                return;
            }
    
            if (AddItem(block))
            {
                print(block.location);
                blockyTerrain.RemoveBlock(block.location);
    
            }
        }
    }
    
    void DropHeldItem()
    {
        //Only drop the item if it is an item
        if(!(inventory[selectedBlockIndex].item is Item))
            return;
        
        Vector3 spawnPos = transform.position + Vector3.up*.77f + Vector3.forward*1.5f;
        
        var item = (Item)inventory[selectedBlockIndex].item;
        var itemObject = Instantiate(item.prefab, spawnPos, Quaternion.identity);
        var pickup = itemObject.AddComponent<Pickup>();
        pickup.item = item;
        
        itemObject.tag = "Pickup";
        itemObject.AddComponent<Rigidbody>();
        
        var triggerCollider = itemObject.AddComponent<SphereCollider>();
        triggerCollider.isTrigger = true;
        triggerCollider.radius = 0.5f;  

        
        
        RemoveItem(selectedBlockIndex);
        
        
    }
    
    void PlaceBlockFromInventory()
    {
        // Raycast from the camera to the block in front of the player
    
        RaycastHit hit;
        Transform cameraTransform = Camera.main.transform;
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
    
        if (Physics.Raycast(ray, out hit, 5f))
        {
            if (hit.collider.gameObject.GetComponent<Renderer>() == null || hit.collider.gameObject.tag != "Cube")
                return;
        }
    
        var block = inventory[selectedBlockIndex];
        if (block.count != 0 && _lookedAtObject != null && block.item.name != null)
        {
            var blockPos = _lookedAtObject.transform.position;
            //Determine which side the raycast hit the looked at object
            Vector3 hitNormal = hit.normal;
            // Place bloc one over from blockpos in the direction of the hit normal
            Vector3 placePos = blockPos + hitNormal * blockyTerrain.cubeObject.prefab.transform.localScale.x;
    
            //Assume the entity is a block -- Should be fine since we are checking for this in the if statement
            var blockItem = (Block)block.item;
    
            // Add the block to the terrain
            var blockToAdd = ScriptableObject.CreateInstance<Block>();
            blockToAdd.name = blockItem.name;
            blockToAdd.id = blockItem.id;
            blockToAdd.location = placePos;
            blockToAdd.rotation = blockItem.rotation;
            blockToAdd.scale = blockItem.scale;
            blockToAdd.prefab = blockItem.prefab;
            blockToAdd.renderOffset = blockItem.renderOffset;
            blockToAdd.isLoaded = true;
            blockToAdd.maxStackSize = blockItem.maxStackSize;
            blockyTerrain.AddBlock(placePos, blockToAdd);
            // Remove the block from the inventory
            RemoveItem(selectedBlockIndex);
        
    }
    }

    void ChangeBlockColor(GameObject blockObject, Color color)
    {
        blockObject.GetComponent<Renderer>().material.color = color;
    }


    string GetInventoryString()
    {
        string inventoryString = "Inventory: ";
        for (int i = 0; i < inventory.Length; i++)
        {
            if (inventory[i].item is not Block)
            {
                throw new System.Exception("Inventory item is not a block");
            }
            if (inventory[i].item.name != null )
            {
                inventoryString += inventory[i].item.name + " ";
            }
        }
        return inventoryString;
    
    }

    bool AddItem(Entity item)
    {
        int stackSize = item.maxStackSize;
        
        print(item.name + " " +stackSize);
        if (inventorySize >= inventoryCapacity)
            return false;
    
        for (int i = 0; i < inventoryCapacity; i++)
        {
            if (inventory[i].item != null && inventory[i].item.id == item.id && inventory[i].count < stackSize)
            {
                inventory[i].count++;
                return true;
            }
            else if (inventory[i].item == null)
            {
                inventory[i] = new InventoryItem<Entity>(item, 1);
                inventorySize++;
                return true;
            }
        }
        return false;
    }

    bool RemoveItem(int index)
    {
        if (inventory[index].item == null)
            return false;
    
        if (inventory[index].count > 1)
        {
            inventory[index].count--;
            return true;
        }
        inventory[index] = new InventoryItem<Entity>(null, 0);
        inventorySize--;
        return true;
    }

    public InventoryItem<Entity>[] getInventory()
    {
        return inventory;
    }

    bool RemoveItem(Entity item)
    {
        for(int itemIndex = 0; itemIndex< inventoryCapacity; ++itemIndex)
        {
            if (inventory[itemIndex].Equals(item))
            {
                inventory[itemIndex] = new InventoryItem<Entity>(new Block(), 0);
                inventorySize--;
                return true;
            }
        }
        return false;
    }

    public int GetSelectedBlockIndex()
    {
        return selectedBlockIndex;
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }
    
    public bool AddHealth(int health)
    {
        if (currentHealth >= playerHealth)
        {
            return false;
        }
        
        if (currentHealth + health > playerHealth)
        {
            currentHealth = playerHealth;
            return true;
        }
        currentHealth += health;
        return true;
    }


}

