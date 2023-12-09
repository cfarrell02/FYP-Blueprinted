using UnityEngine;

public struct Block
{
    public string Name { get; set; }
    public int ID { get; set; }
    public int Durability { get; set; }
    public int MaxDurability { get; set; }
    public int StackSize { get; set; }
    public int MaxStackSize { get; set; }
    public Vector3 Location { get; set; }
    public Vector3 Rotation { get; set; }
    public Vector3 Scale { get; set; }
    public bool isLoaded { get; set; }

    // Constructor for initialization
    public Block(
        string name, int id, int durability, int maxDurability,
        int stackSize, int maxStackSize, Vector3 location,
        Vector3 rotation, Vector3 scale)
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

    }
}
