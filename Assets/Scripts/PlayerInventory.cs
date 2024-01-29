using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem.HID;
[System.Serializable]
public struct InventoryItem<T>
{
    public T item;
    public int count;

    public InventoryItem(T item, int count)
    {
        this.item = item;
        this.count = count;
    }
}


public class PlayerInventory : MonoBehaviour
{
    
    public int inventoryCapacity = 10;
    [SerializeField, Tooltip("This is the starting items of the player.")]
    private InventoryItem<Entity>[] startingItems;

    [SerializeField, Tooltip("This is the maximum health of the player.")]
    private int playerHealth = 100;
    
    private int currentHealth;

    private InventoryItem<Entity>[] inventory;
    private GameObject _lookedAtObject;
    private BlockyTerrain blockyTerrain;
    private GameObject mainCamera;
    private GameObject renderedObject;
    private int inventorySize;
    private GameManager gameManager;


    private int selectedBlockIndex;

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

        for(int i = 0; i < startingItems.Length; ++i)
        {
            inventory[i] = startingItems[i];
        }

        gameManager = GameManager.Instance;
        
    }

    void Update()
    {
        CheckBlockInFront();
        HandleInput();
        UseSelectedTool();
        HandleHealth();
        RenderSelectedItem();
        
        if (Input.GetKeyDown(KeyCode.Q))
        {
            DropHeldItem();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            var list = GetAllCraftableItems();
            print("Craftable items:");
            foreach (var item in list)
            {
                print(item.Item1.name + ": " + item.Item2);
            }
        }
    }

    //This retrieves a list of all craftable items
    //This method is quite expensive and should only be called when necessary.
    public List<(Entity, int)> GetAllCraftableItems()
    {
        Entity[] allItems = gameManager.allEntities.Select(x => x.craftable ? x : null).Where(x => x != null).ToArray();
        List<(Entity, int)> craftableItems = new List<(Entity, int)>();

        foreach (Entity item in allItems)
        {
            List<(Entity, int)> recipeList = item.recipe.Select(x => x.item != null ? (x.item, x.count) : (null, 0)).ToList();
            (bool, int)[] ingredientCheckList = new (bool, int)[recipeList.Count];
            foreach (var ingredient in recipeList)
            {
                var (contains, count) = InventoryContains(ingredient.Item1);
                if(contains && count >= ingredient.Item2)
                    ingredientCheckList[recipeList.IndexOf(ingredient)] = (true, count/ingredient.Item2);
                else
                    ingredientCheckList[recipeList.IndexOf(ingredient)] = (false, 0);
            }
            if (ingredientCheckList.All(x => x.Item1))
            {
                int minCount = ingredientCheckList.Min(x => x.Item2);
                craftableItems.Add((item, minCount));
            }
            
        }
        
        return craftableItems;
    }
    

    //Helper function to check if the inventory contains an entity and how many of that entity
    (bool,int) InventoryContains(Entity entity)
    {
        for (int i = 0; i < inventory.Length; i++)
        {
            if (inventory[i].item && inventory[i].item.id == entity.id)
            {
                return (true,inventory[i].count);
            }
        }
        return (false,-1);
    }
    


    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Pickup"))
        {
            var pickup = other.gameObject.GetComponent<Pickup>();
            
            AddItem(pickup.item);
            Destroy(other.gameObject);
        }

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
                ChangeBlockColor(_lookedAtObject, false);
            }
    
            if (hit.collider.gameObject.GetComponent<Renderer>() == null || !hit.collider.gameObject.CompareTag("Cube"))
                return;
    
            _lookedAtObject = hit.collider.gameObject;
            ChangeBlockColor(_lookedAtObject, true);
        }
        else
        {
            if (_lookedAtObject != null)
            {
                ChangeBlockColor(_lookedAtObject, false);
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



    void HandleInput()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            selectedBlockIndex++;
            if (selectedBlockIndex >= inventoryCapacity)
                selectedBlockIndex = 0;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            selectedBlockIndex--;
            if (selectedBlockIndex < 0)
                selectedBlockIndex = inventoryCapacity - 1;
        }
        //Check if num key is pressed
        for (int i = 1; i <= inventoryCapacity; ++i)
        {
            string numKey = i.ToString();
            numKey = numKey.Length > 1 ? numKey.Substring(1) : numKey;
            if(Input.GetKeyDown(numKey))
            {
                selectedBlockIndex = i-1;
            }
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
        
        Vector3 spawnPos = transform.position + Vector3.up*.77f + transform.forward*1.5f;
        
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
            if (hit.collider.gameObject.GetComponent<Renderer>() == null || !hit.collider.gameObject.CompareTag("Cube"))
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
            blockToAdd.color = blockItem.color;
            
            blockyTerrain.AddBlock(placePos, blockToAdd);
            // Remove the block from the inventory
            RemoveItem(selectedBlockIndex);
        
    }
    }

    void ChangeBlockColor(GameObject blockObject, bool newColor)
    {
        
        if (blockObject != null)
        {
            var block = blockyTerrain.FindBlock(blockObject.transform.position);
            if (block != null)
            {
                if (newColor)
                {
                    blockObject.GetComponent<Renderer>().material.color = Color.red;
                }
                else
                {
                    blockObject.GetComponent<Renderer>().material.color = block.color;
                }
            }
        }
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
        }
        for (int i = 0; i < inventoryCapacity; i++)
        {
            if (inventory[i].item == null)
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
    
    public int GetMaxHealth()
    {
        return playerHealth;
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
    
    public bool RemoveHealth(int health)
    {
        if (currentHealth <= 0)
        {
            return false;
        }
        
        if (currentHealth - health < 0)
        {
            currentHealth = 0;
            return true;
        }
        currentHealth -= health;
        return true;
    }


}

