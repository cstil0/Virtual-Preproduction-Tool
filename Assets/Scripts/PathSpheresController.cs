using System.Collections;
using System.Collections.Generic;
using System.Xml.XPath;
using UnityEngine;
using static DirectorPanelManager;

public class PathSpheresController : MonoBehaviour
{
    public bool triggerOn = false;
    private bool secondaryTriggerButtonDown = false;

    public GameObject item = null;
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

                //StartCoroutine(HoverObjects.instance.deletePathPoint());
                StartCoroutine(deletePathPoint());
            }
        }
        else
            secondaryTriggerButtonDown = false;
    }

    public IEnumerator deletePathPoint()
    {
        gameObject.GetComponent<MeshRenderer>().enabled = false;
        HoverObjects.instance.pointAlreadySelected = false;
        HoverObjects.instance.currentPointCollider = null;

        while (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger)) yield return null;
        //yield return new WaitForSeconds(1f);


        Debug.Log("TRIGGER UP");
        if (HoverObjects.instance.currentSelectedForPath.layer == 10)
        {
            FollowPath followPath = HoverObjects.instance.currentSelectedForPath.GetComponent<FollowPath>();
            followPath.isPointOnTrigger = false;
        }
        else if (HoverObjects.instance.currentSelectedForPath.layer == 7)
        {
            FollowPathCamera followPathCamera = HoverObjects.instance.currentSelectedForPath.GetComponent<FollowPathCamera>();
            followPathCamera.isPointOnTrigger = false;
        }

        Destroy(gameObject);
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
