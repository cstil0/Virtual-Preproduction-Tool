using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectsSelector : MonoBehaviour
{
    public bool isSelected = false;
    private bool secondaryIndexTriggerDown = false;
    public bool triggerOn = false;
    private bool isPlaying = false;
    [SerializeField] Canvas menuCanvas;

    private void OnEnable()
    {
        DirectorPanelManager.instance.OnPlayPath += playLinePath;
        DirectorPanelManager.instance.OnStopPath += stopLinePath;
    }
    private void OnDisable()
    {
        DirectorPanelManager.instance.OnPlayPath -= playLinePath;
        DirectorPanelManager.instance.OnStopPath -= stopLinePath;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void playLinePath()
    {
        isPlaying = !isPlaying;
    }

    void stopLinePath()
    {
        isPlaying = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger) && !isPlaying)
        {
            if (!secondaryIndexTriggerDown && triggerOn)
            {
                secondaryIndexTriggerDown = true;
                isSelected = !isSelected;

                menuCanvas.enabled = isSelected;
            }
        }
        else
            secondaryIndexTriggerDown = false;
    }
}
