using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this script handles spotlights behaviour from multi-camera panel
public class LightController : MonoBehaviour
{
    private Color originalColor;
    private float intensity;
    [SerializeField] Light light;

    void Start()
    {
        originalColor = light.color;
    }

    void Update()
    {
        
    }

    // original color is different than current one if the color has not been accepted yet
    public Color getOriginalLightColor()
    {
        return originalColor;
    }

    public Color getLightColor()
    {
        return light.color;
    }

    public float getIntensity()
    {
        return intensity;
    }

    public void changeLightColor(Color color, bool isAccepted)
    {
        light.color = color;

        if (isAccepted)
            originalColor = color;
    }

    public void changeLightIntensity(float newIntensity)
    {
        intensity = newIntensity;
        light.intensity = intensity;
    }
}
