using System.Collections;
using System.Collections.Generic;
using System.Xml.XPath;
using UnityEngine;
using static DirectorPanelManager;

public class PathSpheresController : MonoBehaviour
{
    public bool triggerOn = false;
    private bool secondaryTriggerButtonDown = false;

    public GameObject item;
    FollowPath followPath;
    FollowPathCamera followPathCamera;
    public bool isBeingCreated = true;


    // Update is called once per frame
    void Update()
    {
        if (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger) && triggerOn)
        {
            // once the hand has exit the trigger at least once, then the point is able to be deleted
            if (!secondaryTriggerButtonDown && !isBeingCreated)
            {
                secondaryTriggerButtonDown = true;
                string[] splittedName = gameObject.transform.name.Split(" ");
                int pointNum = int.Parse(splittedName[1]);

                if (followPath != null)
                    followPath.deletePathPoint(pointNum);
                else if (followPathCamera != null)
                    followPathCamera.deletePathPoint(pointNum);

                StartCoroutine(HoverObjects.instance.deletePathPoint());
                Destroy(gameObject);
            }
        }
        else
            secondaryTriggerButtonDown = false;
    }

    public void changeTriggerState(bool newTriggerState)
    {
        // if point is on trigger we do not want to instantiate new points when pressing trigger button
        triggerOn = newTriggerState;
        // avoid each point changing this variable - do it only on change of state
        if (followPath != null)
            followPath.isPointOnTrigger = triggerOn;
        if (followPathCamera != null)
            followPathCamera.isPointOnTrigger = triggerOn;
    }

    public void getFollowPath()
    {
        item.TryGetComponent<FollowPath>(out followPath);
        item.TryGetComponent<FollowPathCamera>(out followPathCamera);
    }
}
