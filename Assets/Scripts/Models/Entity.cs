using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : ScriptableObject
{
    public int durability ;
    [Tooltip("This is the maximum durability of the item.")]
    public int maxDurability ;
    [Tooltip("This is the name of the item.")]
    public string name ;
    [Tooltip("This is the id of the item. It must be unique.")]
    public int id ;
    [Tooltip("This is the maximum stack size of the item.")]
    public int maxStackSize ;
    [Tooltip("This is the prefab of the item.")]
    public GameObject prefab ;
    [Tooltip("This is the icon of the item.")]
    public Sprite icon ;
    [Tooltip("This is an offset used for rendering the item.")]
    public Vector3 renderOffset ;
    [Tooltip("Is this craftable?")] 
    public bool craftable;
    [Tooltip("This is the recipe for crafting the item. Only applicable if craftable is true.")]
    public InventoryItem<Entity>[] recipe;
    
    // public Vector3 renderRotation ;

}