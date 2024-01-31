using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteAlways]
public class LightingManager : MonoBehaviour
{
    [SerializeField] private Light directionalLight;
    [SerializeField] private LightingPreset preset;
    [SerializeField, Header("Multiplier to control speed of time of day")] private float timeMultiplier = 0.1f;
    
    [SerializeField, Range(0, 24)] private float timeOfDay;

    // Start is called before the first frame update
    void Start()
    {
        
        
    }

    // Update is called once per frame
    void Update()
    {
        if (preset == null)
        {
            return;
        }
        if (Application.isPlaying)
        {
            timeOfDay += Time.deltaTime * timeMultiplier;
            timeOfDay %= 24; //Modulus to ensure always between 0-24
            UpdateLighting(timeOfDay / 24f);
            UpdateFog(timeOfDay / 24f);

        }
        else
        {
            UpdateLighting(timeOfDay / 24f);
        }
        
        //At end of day
        if (timeOfDay > 23.9f)
        {
            GameManager.Instance.IncreaseNightsSurvived();
            timeOfDay = 0;
        }
        
    }
    
    public bool isNight()
    {
        return timeOfDay > 19 || timeOfDay < 6;
    }
    
    private void UpdateLighting(float timePercent)
    {
        RenderSettings.ambientLight = preset.AmbientColor.Evaluate(timePercent);
        RenderSettings.fogColor = preset.FogColor.Evaluate(timePercent);
        if (directionalLight != null)
        {
            directionalLight.color = preset.DirectionalColor.Evaluate(timePercent);
            directionalLight.transform.localRotation = Quaternion.Euler(new Vector3((timePercent * 360f) - 90f, 170f, 0));
        }
    }
    
    private void UpdateFog(float timePercent)
    {
        RenderSettings.fogColor = preset.FogColor.Evaluate(timePercent);
        RenderSettings.fogDensity = preset.FogDensity.Evaluate(timePercent);
    }
    
    private void OnValidate()
    {
        if (directionalLight != null)
        {
            return;
        }
        if (RenderSettings.sun != null)
        {
            directionalLight = RenderSettings.sun;
        }
        else
        {
            Light[] lights = GameObject.FindObjectsOfType<Light>();
            foreach (Light light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    directionalLight = light;
                    return;
                }
            }
        }
    }
}
