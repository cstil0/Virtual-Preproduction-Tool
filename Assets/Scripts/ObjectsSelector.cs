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
        UDPReceiver.instance.OnChangeItemColor += changeItemColorDirector;
    }
    private void OnDisable()
    {
        DirectorPanelManager.instance.OnPlayPath -= playLinePath;
        DirectorPanelManager.instance.OnStopPath -= stopLinePath;
        UDPReceiver.instance.OnChangeItemColor -= changeItemColorDirector;
    }

    void Start()
    {
        
    }

    void playLinePath()
    {
        isPlaying = !isPlaying;

        // hide buttons on play
        GameObject itemControlMenu = transform.Find("ItemControlMenu").gameObject;
        itemControlMenu.GetComponent<Canvas>().enabled = false;
    }

    void stopLinePath()
    {
        isPlaying = false;

        // show buttons again if object was selected before playing, else ensure they are hidden
        GameObject itemControlMenu = transform.Find("ItemControlMenu").gameObject;
        if (isSelected)
            itemControlMenu.GetComponent<Canvas>().enabled = true;
        else
            itemControlMenu.GetComponent<Canvas>().enabled = false;

    }

    void Update()
    {
        if (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger) && !isPlaying)
        {
            if (!secondaryIndexTriggerDown && triggerOn)
            {
                secondaryIndexTriggerDown = true;
                isSelected = !isSelected;

                // enable buttons when object is selected
                menuCanvas.enabled = isSelected;
            }
        }
        else
            secondaryIndexTriggerDown = false;
    }

    // used to change objec's color at client side when a change of color is received
    private void changeItemColorDirector(string itemName, Color color)
    {
        // the event is received for all objects in the scene, so first check that this is the corresponding one
        if (itemName == gameObject.name)
            HoverObjects.instance.changeColorMaterials(gameObject, color, false);
    }
}
