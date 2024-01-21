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
    
    private GameObject player;
    

    public bool Use()
    {
        player = GameObject.FindWithTag("Player");
        durability--;
        switch (itemType)
        {
            case ItemType.Sword:
                Debug.Log("You used a sword");
                break;
            case ItemType.Blueprint:
                Debug.Log("You used a blueprint");
                break;
            case ItemType.Food:
                Debug.Log("You used food");
                break;
            case ItemType.Key:
                Debug.Log("You used a key");
                break;
            case ItemType.Health:
                Debug.Log("You used health");
                var playerInventory = player.GetComponent<PlayerInventory>();
                return playerInventory.AddHealth((int)value);
                
                break;
            
        }

        return false;
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



