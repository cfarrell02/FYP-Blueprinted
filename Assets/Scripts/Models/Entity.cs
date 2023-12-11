using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity
{
    public string Name { get; set; }
    public int ID { get; set; }
    public bool isLoaded { get; set; }
    public int StackSize { get; set; }
    public int MaxStackSize { get; set; }

}