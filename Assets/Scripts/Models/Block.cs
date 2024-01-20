using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
[CreateAssetMenu(fileName = "Block", menuName = "ScriptableObjects/Block", order = 1)]
public class Block : Entity
{

    public Vector3 location ;
    public Vector3 rotation ;
    public Vector3 scale ;
    public bool isLoaded ;


    

    // Constructor for initialization
    public Block(
        string name, int id, int durability, int maxDurability,
        int stackSize, int maxStackSize, Vector3 location,
        Vector3 rotation, Vector3 scale, GameObject prefab)
    {
        this.name = name;
        this.id = id;
        this.durability = durability;
        this.maxDurability = maxDurability;
        this.stackSize = stackSize;
        this.maxDurability = maxStackSize;
        this.location = location;
        this.rotation = rotation;
        this.scale = scale;
        isLoaded = false;
        this.prefab = prefab;
    }
    
    public Block()
    {
        name = "Block";
        id = 0;
        durability = 0;
        maxDurability = 0;
        stackSize = 0;
        maxStackSize = 0;
        location = new Vector3(0, 0, 0);
        rotation = new Vector3(0, 0, 0);
        scale = new Vector3(0, 0, 0);
        isLoaded = true;
        prefab = null;
    }
    
    public void InstantiateBlock(Block block)
    {
        name = block.name;
        id = block.id;
        durability = block.durability;
        maxDurability = block.maxDurability;
        stackSize = block.stackSize;
        maxStackSize = block.maxStackSize;
        location = block.location;
        rotation = block.rotation;
        scale = block.scale;
        isLoaded = block.isLoaded;
        prefab = block.prefab;
    }


}
