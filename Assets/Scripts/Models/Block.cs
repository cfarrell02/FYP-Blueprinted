using UnityEngine;

public class Block : Entity
{

    public Vector3 Location { get; set; }
    public Vector3 Rotation { get; set; }
    public Vector3 Scale { get; set; }
    public int Durability { get; set; }
    public int MaxDurability { get; set; }

    // Constructor for initialization
    public Block(
        string name, int id, int durability, int maxDurability,
        int stackSize, int maxStackSize, Vector3 location,
        Vector3 rotation, Vector3 scale, GameObject prefab)
    {
        Name = name;
        ID = id;
        Durability = durability;
        MaxDurability = maxDurability;
        StackSize = stackSize;
        MaxStackSize = maxStackSize;
        Location = location;
        Rotation = rotation;
        Scale = scale;
        isLoaded = false;
        this.prefab = prefab;

    }

    // Empty constructor
    public Block()
    {
        Name = null;
        ID = 0;
        StackSize = 0;
        MaxStackSize = 0;
        Location = new Vector3(0, 0, 0);
        Rotation = new Vector3(0, 0, 0);
        Scale = new Vector3(0, 0, 0);
        isLoaded = false;
        Durability = 0;
        MaxDurability = 0;
    }
}
