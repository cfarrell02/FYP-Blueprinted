using System.Collections;
using System.Collections.Generic;
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
        
        RenderSelectedItem();

        
    }

    void Update()
    {
        CheckBlockInFront();
        HandleMouseClicks();
        HandleScrollWheel();
        UseSelectedTool();
        HandleHealth();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Pickup"))
        {
            var pickup = other.gameObject.GetComponent<Pickup>();
            if (pickup == null) // Pickup is not a pickup
                return;

            if (pickup.item is Item)
            {
                var item = (Item)pickup.item; // Special case for health / will need other like this for any consumables
                if (item.itemType == Item.ItemType.Health)
                {
                    if (currentHealth == playerHealth)
                        return;
                    
                    currentHealth += (int)item.value;
                    if (currentHealth > playerHealth)
                        currentHealth = playerHealth;
                }
            }
            
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

    void HandleMouseClicks()
    {
        if (Input.GetMouseButtonDown(0)) 
        {
            
            AddBlockToInventory();
        }
        else if (Input.GetMouseButtonDown(1) && inventory[selectedBlockIndex].item is Block)
        {
            PlaceBlockFromInventory();
        }


    }

    void HandleScrollWheel()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            selectedBlockIndex++;
            if (selectedBlockIndex >= inventorySize)
                selectedBlockIndex = 0;
            RenderSelectedItem();
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            selectedBlockIndex--;
            if (selectedBlockIndex < 0)
                selectedBlockIndex = inventorySize - 1;
            RenderSelectedItem();
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

            //var cubePrefab = blockyTerrain.cubePrefab;
            Vector3 offset = selectedItem.item.renderOffset;

            Vector3 placeOffset = Camera.main.transform.forward *offset.z + Camera.main.transform.up * offset.y + Camera.main.transform.right * offset.x;
            
            Vector3 newPosition = Camera.main.transform.position + placeOffset;

            renderedObject = Instantiate(selectedItem.item.prefab, newPosition, Quaternion.identity);
            renderedObject.transform.parent = mainCamera.transform;
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
        var selectedItem = inventory[selectedBlockIndex];

        if (selectedItem.item is Item)
        {
            var tool = (Item)selectedItem.item;
            tool.Update();

            if (Input.GetMouseButtonDown(0))
            {
                tool.Use();
            }
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
                // {
                //     Destroy(_lookedAtObject); //TODO Move this to the blockyTerrain class
                // }
    
            }
        }
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
    
            //Assume the entity is a block -- REMOVE THIS LATER WHEN WE HAVE MORE ENTITIES
            var blockItem = (Block)block.item;
    
            // Add the block to the terrain
            var blockToAdd = ScriptableObject.CreateInstance<Block>();
            blockToAdd.name = blockItem.name;
            blockToAdd.id = blockItem.id;
            blockToAdd.location = placePos;
            blockToAdd.rotation = blockItem.rotation;
            blockToAdd.scale = blockItem.scale;
            blockToAdd.prefab = blockItem.prefab;
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
        if (inventorySize >= inventoryCapacity)
            return false;
    
        for (int i = 0; i < inventoryCapacity; i++)
        {
            if (inventory[i].item != null && inventory[i].item.id == item.id)
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


}

