using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    public GameObject swordPrefab;
    [SerializeField]
    public GameObject blueprintPrefab;

    [SerializeField]
    private int playerHealth = 100;

    private int currentHealth;

    private int inventorySize = 0;
    InventoryItem<Entity>[] inventory;
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

        // Temp add in items to inventory
        //var sword = new Sword("Sword", 100, 100, 1, 1, 10, swordPrefab);
        //inventory[0] = new InventoryItem<Entity>(sword, 1);
        //inventorySize++;
        var blueprint = new Blueprint("Blueprint", 1, 1, 1, 1, blueprintPrefab);
        inventory[0] = new InventoryItem<Entity>(blueprint, 1);
        inventorySize++;

     //   AddItem(new Sword("Blueprint", 100, 100, 1, 1, 10, blueprintPrefab));

        selectedBlockIndex = 0;
        currentHealth = playerHealth;
        
    }

    void Update()
    {
        CheckBlockInFront();
        HandleMouseClicks();
        HandleScrollWheel();
        RenderSelectedItem();
        UseSelectedTool();
        HandleHealth();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Pickup")
        {
            var pickup = other.gameObject.GetComponent<Pickup>();
            if (pickup == null) // Pickup is not a pickup
                return;
                
            var pickupItem = pickup.Item;


            if (pickup.pickupType == Pickup.PickupType.Health && currentHealth<playerHealth)
            {
                currentHealth += pickup.GetValue();
                if (currentHealth > playerHealth)
                    currentHealth = playerHealth;

                Destroy(other.gameObject);
                return;
            }
            else
            {
                if (AddItem(pickupItem))
                {
                    Destroy(other.gameObject);
                }
            }
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
        var selectedItem = inventory[selectedBlockIndex];

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
            if (selectedItem.item is Block || selectedItem.item is Blueprint)
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
        if (Input.GetMouseButtonDown(0) && inventory[selectedBlockIndex].item is Blueprint) 
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
            if (selectedBlock.item != null && selectedBlock.item.prefab != null)
            {
                if (renderedObject != null)
                    Destroy(renderedObject);

                //var cubePrefab = blockyTerrain.cubePrefab;

                Vector3 placeOffset = Camera.main.transform.forward - Camera.main.transform.up * 0.5f + Camera.main.transform.right * 0.5f;


                //Temp fix for sword being weird
                if (selectedItem.item is Sword)
                    placeOffset = Camera.main.transform.forward + Camera.main.transform.right * 0.5f;
                else if (selectedItem.item is Blueprint)
                    placeOffset = Camera.main.transform.forward + Camera.main.transform.right * 0.5f;

                Vector3 newPosition = Camera.main.transform.position + placeOffset;

                renderedObject = Instantiate(selectedItem.item.prefab, newPosition, Quaternion.identity);
               
                var collider = renderedObject.GetComponent<Collider>();
                if (collider != null)
                        collider.enabled = false;

                renderedObject.transform.localScale *= 0.5f;


                if (renderedObject != null)
                {
                    renderedObject.transform.rotation = Camera.main.transform.rotation;
                }
            }

            else
            {
                if (renderedObject != null)
                    Destroy(renderedObject);
                renderedObject = null;
            }
    }

    void UseSelectedTool()
    {
        var selectedItem = inventory[selectedBlockIndex];

        if (selectedItem.item is Tool)
        {
            var tool = (Tool)selectedItem.item;
            tool.Update();

            if (Input.GetMouseButtonDown(0))
            {
                tool.Use();
            }
        }
    }

    void AddBlockToInventory()
    {
        var selectedItem = inventory[selectedBlockIndex];

        if (_lookedAtObject != null)
        {
            var block = blockyTerrain.FindBlock(_lookedAtObject.transform.position);

            if (AddItem(block))
            {
                blockyTerrain.RemoveBlock(block.Location);
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
        if (block.count != 0 && _lookedAtObject != null && block.item.Name != null)
        {
            var blockPos = _lookedAtObject.transform.position;
            //Determine which side the raycast hit the looked at object
            Vector3 hitPoint = hit.point;
            Vector3 hitNormal = hit.normal;
            Vector3 hitDirection = hitPoint - blockPos;
            // Place bloc one over from blockpos in the direction of the hit normal
            Vector3 placePos = blockPos + hitNormal * blockyTerrain.cubePrefab.transform.localScale.y;

            //Assume the entity is a block -- REMOVE THIS LATER WHEN WE HAVE MORE ENTITIES
            var blockItem = (Block)block.item;

            // Add the block to the terrain
            var blockToAdd = new Block(blockItem.Name, blockItem.ID, blockItem.Durability, blockItem.MaxDurability, blockItem.StackSize, blockItem.MaxStackSize, placePos, new Vector3(0, 0, 0), new Vector3(1, 1, 1), blockyTerrain.cubePrefab);
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
            if (inventory[i].item.Name != null )
            {
                inventoryString += inventory[i].item.Name + " ";
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
            if (inventory[i].item != null && inventory[i].item.ID == item.ID)
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

