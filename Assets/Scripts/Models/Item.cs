using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/Item", order = 1)]
public class Item : Entity
{

    public enum ItemType
    {
        Sword,
        Blueprint,
        Food,
        Key,
        Health
    }
    
    public ItemType itemType;

    public float speed;
    public float damage;
    public float value;

    public void Use()
    {
        durability--;
    }

    public void Break()
    {
        durability = 0;
    }

    public void Repair()
    {
        durability = maxDurability;
    }

    public void Repair(int amount)
    {
        durability += amount;
        if (durability > maxDurability)
        {
            durability = maxDurability;
        }
    }

    public void Update()
    {
        if (durability <= 0)
        {
            Break();
        }
    }

}



