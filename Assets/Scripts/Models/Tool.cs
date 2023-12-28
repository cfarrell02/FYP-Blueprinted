using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class Tool : Entity
{
    public int Durability { get; set; }
    public int MaxDurability { get; set; }

    public virtual void Use()
    {
        Durability--;
    }

    public virtual void Break()
    {
        Durability = 0;
    }

    public virtual void Repair()
    {
        Durability = MaxDurability;
    }

    public virtual void Repair(int amount)
    {
        Durability += amount;
        if (Durability > MaxDurability)
        {
            Durability = MaxDurability;
        }
    }

    public virtual void Update()
    {
        if (Durability <= 0)
        {
            Break();
        }
    }

}

public class Sword : Tool
{
    public int Damage { get; set; }

    // Constructor for initialization
    public Sword(
               string name, int durability, int maxDurability,
                      int stackSize, int maxStackSize, int damage, GameObject prefab)
    {
        Name = name;
        ID = 100;
        Durability = durability;
        MaxDurability = maxDurability;
        StackSize = stackSize;
        MaxStackSize = maxStackSize;
        Damage = damage;
        isLoaded = false;
        this.prefab = prefab;
        

    }

    // Empty constructor

    public Sword()
    {
        Name = null;
        ID = 100;
        StackSize = 0;
        MaxStackSize = 0;
        isLoaded = false;
        Durability = 0;
        MaxDurability = 0;
        Damage = 0;
        prefab = null;
    }

    public override void Use()
    {
        Debug.Log("Sword used");
        // Find the enemy that the player is looking at
        RaycastHit hit;
        //Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward, Color.red, 10);
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit))
        {
            // If the player is looking at an enemy, damage it
            if (hit.transform.gameObject.tag == "Enemy")
            {
                hit.transform.gameObject.GetComponent<Enemy>().TakeDamage(Damage);
            }
        }

        base.Use();
    }
}

public class Blueprint : Tool
{ 

    // Constructor for initialization
    public Blueprint(
                      string name, int durability, int maxDurability,
                                           int stackSize, int maxStackSize, GameObject prefab)
    {
        Name = name;
        ID = 101;
        Durability = durability;
        MaxDurability = maxDurability;
        StackSize = stackSize;
        MaxStackSize = maxStackSize;
        isLoaded = false;
        this.prefab = prefab;

    }

    // Empty constructor

    public Blueprint()
    {
        Name = null;
        ID = 101;
        StackSize = 0;
        MaxStackSize = 0;
        isLoaded = false;
        Durability = 0;
        MaxDurability = 0;
    }

    public override void Use()
    {
        // Find the block that the player is looking at
        //RaycastHit hit;
        //if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit))
        //{
        //    // If the player is looking at a block, replace it with the block in the blueprint
        //    if (hit.transform.gameObject.tag == "Block")
        //    {
        //        hit.transform.gameObject.GetComponent<Block>().ID = BlockID;
        //    }
        //}

        //TODO Port over the block placement code from the inventory script!!!

        base.Use();
    }


}