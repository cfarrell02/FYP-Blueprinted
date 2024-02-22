using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightBehaviour : MonoBehaviour
{
    private Light light;
    public Material emmissiveMaterial, nonEmmissiveMaterial;
    private Renderer renderer;
    
    public float oilCapacity = 100f, oilUsage = 0.1f;
    
    
    private float oil;
    
    // Start is called before the first frame update
    void Start()
    {
        oil = oilCapacity;
        light = GetComponentInChildren<Light>();
        renderer = GetComponentInChildren<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (oil > 0)
        {
            oil -= oilUsage * Time.deltaTime;
            light.intensity = oil / oilCapacity;
            renderer.material = emmissiveMaterial;
        }
        else
        {
            //Turn off emission on material
            renderer.material = nonEmmissiveMaterial;
        }
    }
    
    public void RefillOil()
    {
        oil = oilCapacity;
    }
}
