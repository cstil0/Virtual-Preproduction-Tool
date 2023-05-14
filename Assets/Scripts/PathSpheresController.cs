using System;
using System.Collections;
using System.Xml.XPath;
using UnityEngine;

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

    private Vector3 lastPosition;

    public int pathNum;
    public int pointNum;

    private void OnEnable()
    {
        UDPReceiver.instance.OnChangePointColor += changePointColor;
    }

    private void OnDisable()
    {
        UDPReceiver.instance.OnChangePointColor -= changePointColor;
    }

    private void Start()
    {
        lastPosition = gameObject.transform.position;
    }
    // Update is called once per frame
    void Update()
    {
        bool isPlaying = DefinePath.instance.isPlaying;
        if (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger) && triggerOn && !isPlaying)
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
                        // DESELECT ALL POINTS - BETTER FROM HOVER OBJECTS
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

        if (OVRInput.Get(OVRInput.Button.SecondaryHandTrigger) && !isPlaying)
        {
            if (lastPosition != gameObject.transform.position)
            {
                // change both the position in the follow camera component and the line renderer
                Vector3 newPosition = gameObject.transform.position;
                GameObject line = null;
                //int pathNum = -1;
                if (followPathCamera != null)
                {
                    Vector3 direction = lastPosition - newPosition;
                    Vector3 directionInv = newPosition - lastPosition;
                    Vector3 directionCorrected = new Vector3(direction.x, -direction.y, direction.z);
                    followPathCamera.relocatePoint(pointNum, directionCorrected, false, directionInv);
                    //followPathCamera.pathPositions[pointNum] = newPosition;
                    //line = transform.parent.parent.Find("Line").gameObject;

                }
                if (followPath != null)
                {
                    Vector3 distance = lastPosition - newPosition;
                    followPath.pathPositions[pointNum] = followPath.pathPositions[pointNum] - distance;
                    line = transform.parent.Find("Line").gameObject;

                    DefinePath.instance.triggerPointPathChanged(pathNum, pointNum, distance);

                    // get the line by looking at the path container's childs
                    LineRenderer lineineRenderer = line.GetComponent<LineRenderer>();
                    lineineRenderer.SetPosition(pointNum, newPosition);
                }
                lastPosition = gameObject.transform.position;
            }
        }

        if (isSelected && OVRInput.Get(OVRInput.Button.SecondaryThumbstickUp) && !isPlaying)
        {
            if (followPath != null)
            {
                followPath.relocatePoint(pointNum, upVector);
                DefinePath.instance.triggerPointPathChanged(pathNum, pointNum, downVector);
            }
            if (followPathCamera != null)
                followPathCamera.relocatePoint(pointNum, upVector, true, new Vector3(0.0f, 0.0f, 0.0f));
        }

        if (isSelected && OVRInput.Get(OVRInput.Button.SecondaryThumbstickDown) && !isPlaying)
        {
            if (followPath != null)
            {
                followPath.relocatePoint(pointNum, downVector);
                DefinePath.instance.triggerPointPathChanged(pathNum, pointNum, upVector);
            }
            if (followPathCamera != null)
                followPathCamera.relocatePoint(pointNum, downVector, true, new Vector3(0.0f, 0.0f, 0.0f));
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

    void changePointColor(string itemName, string pointName, Color color)
    {
        StartCoroutine(waitItemAssigned(itemName, pointName, color));
    }

    IEnumerator waitItemAssigned(string itemName, string pointName, Color color)
    {
        while (item == null) yield return null;

        if (itemName == item.name && gameObject.name == pointName)
        {
            Renderer renderer = gameObject.GetComponent<Renderer>();
            Material material = renderer.material;
            material.color = color;
        }
    }
}
