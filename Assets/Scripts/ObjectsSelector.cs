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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void playLinePath()
    {
        isPlaying = !isPlaying;

        GameObject itemControlMenu = transform.Find("ItemControlMenu").gameObject;
        itemControlMenu.GetComponent<Canvas>().enabled = false;
    }

    void stopLinePath()
    {
        isPlaying = false;

        GameObject itemControlMenu = transform.Find("ItemControlMenu").gameObject;
        if (isSelected)
            itemControlMenu.GetComponent<Canvas>().enabled = true;
        else
            itemControlMenu.GetComponent<Canvas>().enabled = false;

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

    private void changeItemColorDirector(string itemName, Color color)
    {
        if (itemName == gameObject.name)
            HoverObjects.instance.changeColorMaterials(gameObject, color, false);
    }
}
