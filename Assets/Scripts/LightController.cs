using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightController : MonoBehaviour
{
    private Color originalColor;
    [SerializeField] Light light;

    // Start is called before the first frame update
    void Start()
    {
        originalColor = light.color;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Color getOriginalLightColor()
    {
        return originalColor;
    }

    public Color getLightColor()
    {
        return light.color;
    }

    public void changeLightColor(Color color, bool isAccepted)
    {
        light.color = color;

        if (isAccepted)
            originalColor = color;
    }

    public void changeLightIntensity(float intensity)
    {
        light.intensity = intensity;
    }
}
