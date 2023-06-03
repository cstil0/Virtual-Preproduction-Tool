using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

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

        // ensure that pointNum was already assigned before changing its rotation
        if (lastRotation != gameObject.transform.rotation && pointNum != -1)
        {
            changePointRotation();
            lastRotation = gameObject.transform.rotation;
        }
    }

    public void changePointRotation()
    {
        string[] pathName = transform.parent.name.Split(" ");
        int pathNum = int.Parse(pathName[1]);
        followPathCamera.pathRotations[pathNum + 1] = gameObject.transform.rotation.eulerAngles;
    }
}
