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
        Health
        
    }
    
    public ItemType itemType;
    
    [Header("Item Properties")]
    [Tooltip("Speed of which this tool can be used")]
    public float speed;
    [Tooltip("How much damage this tool does")]
    public float damage;
    [Tooltip("Only if tool is consumable, how much x it gives")]
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
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 100))
                {
                    Debug.Log(hit.transform.name);
                    if (hit.transform.tag == "Enemy")
                    {
                        Debug.Log("You hit an enemy");
                        var enemy = hit.transform.GetComponent<Health>();
                        enemy.TakeDamage((int)damage);
                    }
                }
                break;
            case ItemType.Blueprint:
                Debug.Log("You used a blueprint");
                break;
            case ItemType.Food:
                Debug.Log("You used food");
                break;
            case ItemType.Health:
                Debug.Log("You used health");
                var playerHealth = player.GetComponent<Health>();
                playerHealth.Heal((int)value);
                
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



