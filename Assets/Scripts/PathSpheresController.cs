using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.XPath;
using UnityEngine;
using static DirectorPanelManager;

public class PathSpheresController : MonoBehaviour
{
    public bool triggerOn = false;
    private bool secondaryTriggerButtonDown = false;
    public bool isSelected = false;

    public GameObject item = null;
    public FollowPath followPath;
    public FollowPathCamera followPathCamera;
    public bool isBeingCreated = true;
    [SerializeField] Vector3 upVector = new Vector3(0.0f, 0.005f, 0.0f);
    [SerializeField] Vector3 downVector = new Vector3(0.0f, -0.005f, 0.0f);


    // Update is called once per frame
    void Update()
    {
        if (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger) && triggerOn)
        {
            // once the hand has exit the trigger at least once, then the point is able to be deleted
            if (!secondaryTriggerButtonDown && !isBeingCreated)
            {
                secondaryTriggerButtonDown = true;
                string[] splittedName = { "" };
                Transform parent = null;
                if (followPath != null)
                {
                    splittedName = gameObject.transform.name.Split(" ");
                    parent = gameObject.transform.parent;
                }
                else if (followPathCamera != null)
                {
                    splittedName = gameObject.transform.parent.name.Split(" ");
                    parent = gameObject.transform.parent.parent;
                }

                try
                {
                    int pointNum = int.Parse(splittedName[1]);
                    isSelected = !isSelected;

                    if (isSelected)
                    {
                        // iterate through all points and deselect all
                        for (int i = 1; i < parent.childCount; i++)
                        {
                            if (i == pointNum + 1)
                                continue;

                            GameObject currPoint = null;
                            if (followPath != null)
                                currPoint = parent.GetChild(i).gameObject;
                            if (followPathCamera != null)
                                currPoint = parent.GetChild(i).GetChild(1).gameObject;

                            currPoint.GetComponent<PathSpheresController>().isSelected = false;
                            Renderer renderer = currPoint.GetComponent<Renderer>();
                            Material parentMaterial = renderer.material;
                            parentMaterial.color = DefinePath.instance.selectedLineColor;
                        }
                    }
                }
                catch (Exception e) { Debug.LogError(e.Message); }

                
                //if (followPath != null)
                //    followPath.deletePathPoint(pointNum);
                //else if (followPathCamera != null)
                //    followPathCamera.deletePathPoint(pointNum);

                //StartCoroutine(HoverObjects.instance.deletePathPoint());
                //StartCoroutine(deletePathPoint());
            }
        }
        else
            secondaryTriggerButtonDown = false;

        if (OVRInput.Get(OVRInput.Button.SecondaryHandTrigger))
        {
            if (!isBeingCreated && triggerOn)
            {
                // change both the position in the follow camera component and the line renderer
                Vector3 newPosition = gameObject.transform.position;
                GameObject line = null;
                int pathNum = -1;
                if (followPathCamera != null)
                {
                    string[] pathName = transform.parent.name.Split(" ");
                    pathNum = int.Parse(pathName[1]);
                    followPathCamera.pathPositions[pathNum] = newPosition;
                    line = transform.parent.parent.Find("Line").gameObject;
                }
                if (followPath != null)
                {
                    string[] pathName = gameObject.name.Split(" ");
                    pathNum = int.Parse(pathName[1]);
                    followPath.pathPositions[pathNum] = newPosition;
                    line = transform.parent.Find("Line").gameObject;
                }
                // get the line by looking at the path container's childs
                LineRenderer lineineRenderer = line.GetComponent<LineRenderer>();
                lineineRenderer.SetPosition(pathNum, newPosition);
            }
        }

        if (isSelected && OVRInput.Get(OVRInput.Button.SecondaryThumbstickUp))
        {
            secondaryTriggerButtonDown = true;
            string[] splittedName = { "" };
            if (followPath != null)
                splittedName = gameObject.transform.name.Split(" ");
            else if (followPathCamera != null)
                splittedName = gameObject.transform.parent.name.Split(" ");
            
            int pointNum = int.Parse(splittedName[1]);

            if (followPath != null)
                followPath.relocatePoint(pointNum, upVector);
            if (followPathCamera != null)
                followPathCamera.relocatePoint(pointNum, upVector);
        }

        if (isSelected && OVRInput.Get(OVRInput.Button.SecondaryThumbstickDown))
        {
            string[] splittedName = { "" };
            if (followPath != null)
                splittedName = gameObject.transform.name.Split(" ");
            else if (followPathCamera != null)
                splittedName = gameObject.transform.parent.name.Split(" ");
            
            int pointNum = int.Parse(splittedName[1]);

            if (followPath != null)
                followPath.relocatePoint(pointNum, downVector);
            if (followPathCamera != null)
                followPathCamera.relocatePoint(pointNum, downVector);
        }
    }

    public IEnumerator deletePathPoint()
    {
        gameObject.GetComponent<MeshRenderer>().enabled = false;
        HoverObjects.instance.pointAlreadySelected = false;
        HoverObjects.instance.currentPointCollider = null;

        while (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger)) yield return null;
        //yield return new WaitForSeconds(1f);


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

        // add the rigidbody once the hand did the trigger exit to avoid pulling out the OVRPlayer
        if (!triggerOn)
        {
            if (followPathCamera != null)
                gameObject.transform.parent.GetComponent<SphereCollider>().enabled = true;
        }
    }

    public void getFollowPath()
    {
        item.TryGetComponent<FollowPath>(out followPath);
        item.TryGetComponent<FollowPathCamera>(out followPathCamera);
    }
}
