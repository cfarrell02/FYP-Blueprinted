using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
[CreateAssetMenu(fileName = "Block", menuName = "ScriptableObjects/Block", order = 1)]
public class Block : Entity
{

    [Tooltip("This is the location of the block.")]
    public Vector3 location ;
    [Tooltip("This is the rotation of the block.")]
    public Vector3 rotation ;
    [Tooltip("This is the scale of the block.")]
    public Vector3 scale ;
    [Tooltip("This is a bool for if the block is loaded.")]
    public bool isLoaded ;
    [Tooltip("This is the color of the block.")]
    public Color color ;
    [SerializeField, Tooltip("This is a value that depends on the block type.")]
    public int value;
    [SerializeField, Tooltip("This is the type of block, that determines how it behaves.")]
    public BlockType blockType;
    [SerializeField, Tooltip("Can other blocks be placed on this block?")]
    public bool canPlaceOn;
    [SerializeField, Tooltip("Can this block be picked up?")]
    public bool canPickUp;

    public enum BlockType
    {
        Normal,
        AntiSpawn,
        Empty,
        Light,
    }


    

    // Constructor for initialization
    public Block(
        string name, int id, int durability, int maxDurability,
         int maxStackSize, Vector3 location,
        Vector3 rotation, Vector3 scale, GameObject prefab, BlockType blockType)
    {
        this.name = name;
        this.id = id;
        this.durability = durability;
        this.maxDurability = maxDurability;
        this.maxStackSize = maxStackSize;
        this.location = location;
        this.rotation = rotation;
        this.scale = scale;
        isLoaded = false;
        this.prefab = prefab;
        this.blockType = blockType;
        color = prefab.GetComponent<Renderer>().material.color;
    }
    
    public Block()
    {
        name = "Block";
        id = 0;
        durability = 0;
        maxDurability = 0;
        maxStackSize = 0;
        location = new Vector3(0, 0, 0);
        rotation = new Vector3(0, 0, 0);
        scale = new Vector3(0, 0, 0);
        isLoaded = true;
        prefab = null;
        blockType = BlockType.Normal;
        color = new Color(1, 1, 1);
    }
    
    public override void CopyOf(Entity entity)
    {
        if (entity is Block == false)
        {
            return;
        }
        var block = (Block) entity;
        
        name = block.name;
        id = block.id;
        durability = 0;
        maxDurability = block.maxDurability;
        maxStackSize = block.maxStackSize;
        location = block.location;
        rotation = block.rotation;
        scale = block.scale;
        isLoaded = block.isLoaded;
        prefab = block.prefab;
        renderOffset = block.renderOffset;
        color = block.color;
        value = block.value;
        blockType = block.blockType;
        canPlaceOn = block.canPlaceOn;
        canPickUp = block.canPickUp;
    }

}
