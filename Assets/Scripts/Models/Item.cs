using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static Utils.Utils;

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
    public AudioClip hitSound,missSound;
    
    private GameObject player;
    

    public bool Use()
    {
        player = GameObject.FindWithTag("Player");
        durability--;
        switch (itemType)
        {
            case ItemType.Sword:
                var audioSource = player.GetComponent<AudioSource>();
                Debug.Log("You used a sword");
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                //Every layer except minimap
                LayerMask mask = ~LayerMask.GetMask("Minimap","Ignore Raycast");


                if (Physics.Raycast(ray, out hit, 10, mask))
                {
                    Debug.Log(hit.transform.name);
                    if (hit.transform.CompareTag("Enemy"))
                    {
                        Debug.Log("You hit an enemy");
                        var enemy = hit.transform.GetComponent<Health>();
                        enemy.TakeDamage((int)damage);

                        // Apply knockback
                        var enemyBehaviour = hit.transform.GetComponent<EnemyBehaviour>();
                        if (enemyBehaviour != null)
                        {
                            // add upwards force
                            var direction = ray.direction + Vector3.up;
                            enemyBehaviour.TakeKnockback(direction, 100);
                        }

                        audioSource.PlayOneShot(hitSound);
                    }
                    else
                    {
                        audioSource.PlayOneShot(missSound);
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
    
    public override void CopyOf(Entity entity)
    {
        if (entity is Item == false)
        {
            return;
        }
        var item = (Item) entity;
        
        name = item.name;
        id = item.id;
        durability = item.durability;
        maxDurability = item.maxDurability;
        maxStackSize = item.maxStackSize;
        prefab = item.prefab;
        renderOffset = item.renderOffset;
        value = item.value;
        itemType = item.itemType;
        speed = item.speed;
        damage = item.damage;
        craftable = item.craftable;
        recipe = item.recipe;
        minLevel = item.minLevel;
    }

}



