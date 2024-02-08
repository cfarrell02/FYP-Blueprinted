using System.Collections;
using System.Collections.Generic;
using Meryuhi.Rendering;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class FogManager : MonoBehaviour
{
    private FullScreenFog fog;
    private LightingManager lightingManager;
    
    [Range(0, 1)]
    public float Intensity, NightIntensity;
    [Range(0, 1)]
    public float Density, NightDensity;
    public Color Color, NightColor;
    // Start is called before the first frame update
    void Start()
    {
        Volume volume = GetComponent<Volume>();
        lightingManager = FindObjectOfType<LightingManager>();
        fog = volume.profile.components[0] as FullScreenFog; // Wont be chaning this so we can just cast it
        if(fog == null)
        {
            Debug.LogError("Fog is null");
            return;
        }
        
        fog.active = true;
        fog.intensity.value = Intensity;
        fog.color.value = Color;
        fog.density.value = Density;
    }
    

    // Update is called once per frame
    void Update()
    {   
        if (lightingManager.isNight())
        {
            fog.color.value = NightColor;
            fog.intensity.value = NightIntensity;
            fog.density.value = NightDensity;
        }
        else
        {
            fog.color.value = Color;
            fog.intensity.value = Intensity;
            fog.density.value = Density;
        }
        
        
    }
}
