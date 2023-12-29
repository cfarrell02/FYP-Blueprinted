using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Pickup: MonoBehaviour
{

    public enum PickupType
    {
        Sword,
        Axe,
        Pickaxe,
        Shovel,
        Hoe,
        Food,
        Key,
        Health
    }


    public Entity Item;

    [SerializeField]
    private int value = 20;

    [SerializeField]
    private GameObject inventoryPrefab;

    [SerializeField]
    public PickupType pickupType;

    private void Start()
    {
        switch (pickupType)
        {
            case PickupType.Sword:
                Item = new Sword("Sword", 100, 100, 1, 1, 10, inventoryPrefab);
                break;
            case PickupType.Axe:
                //Item = new Axe("Axe", 100, 100, 1, 1, 10, inventoryPrefab);
                break;
            case PickupType.Pickaxe:
               // Item = new Pickaxe("Pickaxe", 100, 100, 1, 1, 10, inventoryPrefab);
                break;
            case PickupType.Shovel:
               // Item = new Shovel("Shovel", 100, 100, 1, 1, 10, inventoryPrefab);
                break;
            case PickupType.Hoe:
              //  Item = new Hoe("Hoe", 100, 100, 1, 1, 10, inventoryPrefab);
                break;
            case PickupType.Food:
              //  Item = new Food("Food", 100, 100, 1, 1, 10, inventoryPrefab);
                break;
            case PickupType.Key:
              //  Item = new Key("Key", 100, 100, 1, 1, 10, inventoryPrefab);
                break;
            case PickupType.Health:
                Item = null;
                break;
        }

        
    }

    public int GetValue()
    {
        return value;
    }

}
