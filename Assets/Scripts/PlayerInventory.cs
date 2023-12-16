using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        var sword = new Sword("Sword", 100, 100, 1, 1, 10);
        inventory[0] = new InventoryItem<Entity>(sword, 1);
        inventorySize++;
        var blueprint = new Blueprint("Blueprint", 1, 1, 1, 1);
        inventory[1] = new InventoryItem<Entity>(blueprint, 1);
        inventorySize++;
        
    }

    // Update is called once per frame
    void Update()
    {
        var selectedItem = inventory[selectedBlockIndex];

        if (selectedItem.item != null && (selectedItem.item is Blueprint || selectedItem.item is Block))
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
                if (hit.collider.gameObject.GetComponent<Renderer>() == null || hit.collider.gameObject.tag != "Cube")
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
                    var block = blockyTerrain.FindBlock(_lookedAtObject.transform.position);


                    if (AddItem(block))
                    {
                        bool success = blockyTerrain.RemoveBlock(block.Location);
                        if (success)
                        {
                            Destroy(_lookedAtObject);

                            // Make a list of all six blocks around the block that was just removed
                            Vector3[] surroundingBlocks = new Vector3[6];
                            surroundingBlocks[0] = new Vector3(block.Location.x + 1, block.Location.y, block.Location.z);
                            surroundingBlocks[1] = new Vector3(block.Location.x - 1, block.Location.y, block.Location.z);
                            surroundingBlocks[2] = new Vector3(block.Location.x, block.Location.y + 1, block.Location.z);
                            surroundingBlocks[3] = new Vector3(block.Location.x, block.Location.y - 1, block.Location.z);
                            surroundingBlocks[4] = new Vector3(block.Location.x, block.Location.y, block.Location.z + 1);
                            surroundingBlocks[5] = new Vector3(block.Location.x, block.Location.y, block.Location.z - 1);
                            foreach (Vector3 surroundingBlock in surroundingBlocks)
                            {
                                var surroundingBlockItem = blockyTerrain.FindBlock(surroundingBlock);
                                // Instantiate the surrounding block if it exists
                                if (surroundingBlockItem.Name != null)
                                {
                                    var foundBlock = blockyTerrain.FindBlock(surroundingBlock);
                                    if (foundBlock.Name != null && !foundBlock.isLoaded)
                                    {
                                        //Add the block to the terrain, the method also instantiates the block
                                        blockyTerrain.AddBlock(surroundingBlock, surroundingBlockItem);
                                        //surroundingBlockPrefab.GetComponent<Renderer>().material.color = Color.red;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else if (Input.GetMouseButtonDown(1))
            {
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
                    var blockToAdd = new Block(blockItem.Name, blockItem.ID, blockItem.Durability, blockItem.MaxDurability, blockItem.StackSize, blockItem.MaxStackSize, placePos, new Vector3(0, 0, 0), new Vector3(1, 1, 1));
                    blockyTerrain.AddBlock(placePos, blockToAdd);
                    // Remove the block from the inventory
                    RemoveItem(selectedBlockIndex);
                }
            }

        }

        print(inventorySize + " " + inventoryCapacity);
        print(selectedBlockIndex);

        // Scroll wheel to change selected block
        if (Input.GetAxis("Mouse ScrollWheel") > 0f) // forward
        {
            selectedBlockIndex++;
            if (selectedBlockIndex >= inventorySize)
                selectedBlockIndex = 0;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f) // backwards
        {
            selectedBlockIndex--;
            if (selectedBlockIndex < 0)
                selectedBlockIndex = inventorySize - 1;
        }


        // This block renders the block like its being held in the player's hand, may need tweaking when new blocks or items are added
        if (selectedItem.item is Block)
        {

            // Instantiate a prefab of the selected block childed to the camera
            var selectedBlock = inventory[selectedBlockIndex];
            if (selectedBlock.item != null)
            {
                if (renderedObject != null)
                    Destroy(renderedObject);

                var cubePrefab = blockyTerrain.cubePrefab; // Will need to be updated when new blocks are added

                // Set the offset for block placement (down and to the right from the camera)
                Vector3 placeOffset = mainCamera.transform.forward - mainCamera.transform.up * 0.5f + mainCamera.transform.right * 0.5f;

                // Calculate the position to place the block
                Vector3 newPosition = mainCamera.transform.position + placeOffset;

                // Instantiate the block at the calculated position
                renderedObject = Instantiate(cubePrefab, newPosition, Quaternion.identity);
                // Turn off the collider so the raycast doesn't hit it
                renderedObject.GetComponent<Collider>().enabled = false;
                renderedObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            }
            

            if (renderedObject != null)
            {
                // rotate same as camera
                renderedObject.transform.rotation = mainCamera.transform.rotation;
            }
        }
        else
        {
            if (renderedObject != null)
                Destroy(renderedObject);
            renderedObject = null;
        }

        if (selectedItem.item is Tool)
        {
            var tool = (Tool) selectedItem.item;

            tool.Update();

            if (Input.GetMouseButtonDown(0))
            {
                tool.Use();
            }

            
        }


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

    bool AddItem(Block item)
    {
        if (inventorySize >= inventoryCapacity)
            return false;

        for (int freeIndex = 0; freeIndex < inventoryCapacity; freeIndex++)
        {
            if (inventory[freeIndex].item !=null && inventory[freeIndex].item.ID == item.ID)
            {
                inventory[freeIndex].count++;
                return true;
            }
        }
        for (int freeIndex = 0; freeIndex < inventoryCapacity; freeIndex++)
        {
            if (inventory[freeIndex].item == null)
            {
                inventory[freeIndex] = new InventoryItem<Entity>(item, 1);
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

    bool RemoveItem(Block item)
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


}

