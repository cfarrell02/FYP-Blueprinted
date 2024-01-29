using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : ScriptableObject
{
    protected int durability ;
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
    // public Vector3 renderRotation ;

}