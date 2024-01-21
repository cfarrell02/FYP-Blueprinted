using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : ScriptableObject
{
    protected int durability ;
    public int maxDurability ;
    public string name ;
    public int id ;
    public int maxStackSize ;
    public GameObject prefab ;
    public Vector3 renderOffset ;
    // public Vector3 renderRotation ;

}