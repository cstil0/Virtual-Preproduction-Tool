using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

// this script handles the rotation of minicameras to set the rotation of each point in the camera's path
public class CameraRotationController : MonoBehaviour
{
    public bool triggerOn = false;
    private bool triggerButtonDown = false;
    public bool isSelected = false;
    private Vector3 rotationPan = new Vector3(0.0f, 20.0f, 0.0f);
    private Vector3 rotationTilt = new Vector3(20.0f, 0.0f, 0.0f);

    public FollowPathCamera followPathCamera;
    private Quaternion lastRotation;
    public int pointNum = -1;

    private void OnEnable()
    {
        UDPReceiver.instance.OnChangeMiniCameraColor += changeMiniCameraColor;
    }

    private void OnDisable()
    {
        UDPReceiver.instance.OnChangeMiniCameraColor -= changeMiniCameraColor;
    }

    void Start()
    {
        lastRotation = gameObject.transform.rotation;
    }

    void Update()
    {
        // first check the trigger to inform follow path camera that mini camera to avoid instantiating new path points
        if (triggerOn)
        {
            if (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger))
            {
                if (!triggerButtonDown)
                {
                    isSelected = !isSelected;
                    triggerButtonDown = true;
                }
            }
            else
                triggerButtonDown = false;
        }

        // rotate the minicamera according to thumbstick input
        if (OVRInput.Get(OVRInput.Button.SecondaryThumbstickRight) && isSelected)
        {
            gameObject.transform.Rotate(rotationPan * Time.deltaTime);
        }
        if (OVRInput.Get(OVRInput.Button.SecondaryThumbstickLeft) && isSelected)
        {
            gameObject.transform.Rotate(-rotationPan * Time.deltaTime);
        }
        if (OVRInput.Get(OVRInput.Button.SecondaryThumbstickUp) && isSelected)
        {
            gameObject.transform.Rotate(rotationTilt * Time.deltaTime);
        }
        if (OVRInput.Get(OVRInput.Button.SecondaryThumbstickDown) && isSelected)
        {
            gameObject.transform.Rotate (-rotationTilt * Time.deltaTime);
        }

        // apply the selected rotation when changed, but first ensure that pointNum was already assigned before changing its rotation
        if (lastRotation != gameObject.transform.rotation && pointNum != -1)
        {
            changePointRotation();
            lastRotation = gameObject.transform.rotation;
        }
    }

    // used to assign the new rotation in the pathrotations array
    public void changePointRotation()
    {
        string[] pointName = transform.parent.name.Split(" ");
        int pointNum = int.Parse(pointName[1]);
        followPathCamera.pathRotations[pointNum + 1] = gameObject.transform.rotation.eulerAngles;
    }

    // used to change the multicamera color when the event is triggered
    void changeMiniCameraColor(string cameraName, string pointName, Color color)
    {
        StartCoroutine(waitCameraAssigned(cameraName, pointName, color));
    }

    // wait until there is a reference of its corresponding item to change the point's color on client side
    IEnumerator waitCameraAssigned(string cameraName, string pointName, Color color)
    {
        while (followPathCamera == null) yield return null;

        string currCameraName = followPathCamera.gameObject.name;
        string currPointName = transform.parent.name;
        
        // check if the received minicamera and camera names correspond to the current one, and change its color accordingly if it is the case
        if (cameraName == currCameraName && currPointName == pointName)
        {
            HoverObjects.instance.changeColorMaterials(gameObject, color, false);
        }
    }
}
