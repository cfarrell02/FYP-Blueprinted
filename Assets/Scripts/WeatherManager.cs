using System.Collections;
using System.Collections.Generic;
using Meryuhi.Rendering;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class WeatherManager : MonoBehaviour
{
    private FullScreenFog fog;
    private LightingManager lightingManager;
    
    [Range(0, 1)]
    public float Intensity, NightIntensity;
    [Range(0, 1)]
    public float Density, NightDensity;
    public Color Color, NightColor;
    private float intensityMultiplier = 1;
    
    //Weather Variables
    [Range(0,1)]
    public float rainProbability, snowProbability;
    [Range(10,100), Tooltip("Min Weather Time")]
    public float minRainTime, minSnowTime;
    [Range(100,1000), Tooltip("Max Weather Time")]
    public float maxRainTime, maxSnowTime;
    
    public GameObject snowBlock, rainParticleEffect, snowParticleEffect;

    private float weatherTimer = 0, snowBlockSpawnTimer = 0, rainTimer = 0, snowTimer = 0;
    private bool isRaining = false, isSnowing = false;
    private GameObject player;
    private float currentRainTime, currentSnowTime;
    
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
        fog.intensity.value = Intensity * intensityMultiplier;
        fog.color.value = Color;
        fog.density.value = Density;
        player = GameObject.FindGameObjectWithTag("Player");
    }
    

    // Update is called once per frame
    void Update()
    {   
        if (lightingManager.isNight())
        {
            fog.color.value = NightColor;
            fog.intensity.value = NightIntensity * intensityMultiplier;
            fog.density.value = NightDensity;
        }
        else
        {
            fog.color.value = Color;
            fog.intensity.value = Intensity * intensityMultiplier;
            fog.density.value = Density;
        }
        weatherTimer += Time.deltaTime;
        
        rainTimer += isRaining ? Time.deltaTime : 0;
        snowTimer += isSnowing ? Time.deltaTime : 0;

        if (weatherTimer < 5f)
        {
            return;
        }
        
        weatherTimer = 0;
        
        float weather = Random.Range(0f, 1f);
        if ((weather < rainProbability || isRaining) && !isSnowing)
        {
            if (!isRaining)
            {
                SetRain(true);
                currentRainTime = Random.Range(minRainTime, maxRainTime);
                rainTimer = 0;
                
            }
            
            if (rainTimer > currentRainTime)
            {
                SetRain(false);
                rainTimer = 0;
            }
            
        }else if ((weather < snowProbability || isSnowing) && !isRaining)
        {
            if (!isSnowing)
            {
                SetSnow(true);
                currentSnowTime = Random.Range(minSnowTime, maxSnowTime);
                snowTimer = 0;
            }
            
            if (snowTimer > currentSnowTime)
            {
                SetSnow(false);
                snowTimer = 0;
            }
            
        }
        


    }

    private void SetRain(bool raining)
    {
        isRaining = raining;
        Quaternion rotation = Quaternion.Euler(90, 0, 0);
        if (isRaining)
        {
            var rain = Instantiate(rainParticleEffect, 
                player.transform.position + new Vector3(0, 10, 0), rotation, player.transform);
            rain.tag = "Rain";
            
            
        }
        else
        {
            var rain = GameObject.FindGameObjectWithTag("Rain");
            Destroy(rain);
        }
    }
    
    private void SetSnow(bool snowing)
    {
        isSnowing = snowing;
        Quaternion rotation = Quaternion.Euler(90, 0, 0);
        if (isSnowing)
        {
            var snow = Instantiate(snowParticleEffect, 
                player.transform.position + new Vector3(0, 10, 0), rotation, player.transform);
            snow.tag = "Snow";
            
            List<Vector3> particlePositions = new List<Vector3>();
            for (int i = 0; i < 20; i++)
            {
                for (int j = 0; j < 20; j++)
                {
                    particlePositions.Add(new Vector3(i, 0, j));
                }
            }
            
            foreach (var pos in particlePositions)
            {
                Vector3 offset = new Vector3(-10,0,-10) + pos;
                Debug.DrawRay(snow.transform.position + offset, Vector3.down*100, Color.red, 1000);
            }
            
        }
        else
        {
            var snow = GameObject.FindGameObjectWithTag("Snow");
            Destroy(snow);
        }
    }

    public void SetIntensityMultiplier(float multiplier)
    {
        intensityMultiplier = multiplier;
    }
}
