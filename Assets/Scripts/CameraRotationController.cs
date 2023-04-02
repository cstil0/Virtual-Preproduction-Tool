using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotationController : MonoBehaviour
{
    public Transform currentSelectedMiniCamera;
    //public GameObject currentSelectedCamera;
    public bool triggerOn = false;
    private bool triggerButtonDown = false;
    public bool isSelected = false;
    private Vector3 rotationPan = new Vector3(0.0f, 20.0f, 0.0f);
    private Vector3 rotationTilt = new Vector3(20.0f, 0.0f, 0.0f);

    public FollowPathCamera followPathCamera;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // first check the trigger to inform follow path camera that mini camera to avoid instantiating new path points
        if (triggerOn)
        {
            followPathCamera.isMiniCameraOnTrigger = true;
            if (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger))
            {
                if (!triggerButtonDown)
                {
                    isSelected = !isSelected;
                    triggerButtonDown = true;
                    if (isSelected)
                        currentSelectedMiniCamera = transform;
                    else
                        currentSelectedMiniCamera = null;

                }
            }
            else
                triggerButtonDown = false;
        }
        else
            followPathCamera.isMiniCameraOnTrigger = false;


        if (OVRInput.Get(OVRInput.Button.SecondaryThumbstickRight) && isSelected)
        {
            currentSelectedMiniCamera.Rotate(rotationPan * Time.deltaTime);
            changePointRotation();
        }
        if (OVRInput.Get(OVRInput.Button.SecondaryThumbstickLeft) && isSelected)
        {
            currentSelectedMiniCamera.Rotate(-rotationPan * Time.deltaTime);
            changePointRotation();
        }
        if (OVRInput.Get(OVRInput.Button.SecondaryThumbstickUp) && isSelected)
        {
            currentSelectedMiniCamera.Rotate(rotationTilt * Time.deltaTime);
            changePointRotation();
        }
        if (OVRInput.Get(OVRInput.Button.SecondaryThumbstickDown) && isSelected)
        {
            currentSelectedMiniCamera.Rotate (-rotationTilt * Time.deltaTime);
            changePointRotation();
        }
    }

    void changePointRotation()
    {
        string[] pathName = transform.parent.name.Split(" ");
        int pathNum = int.Parse(pathName[1]);
        //followPathCamera.pathRotations[pathNum] = currentSelectedMiniCamera.rotation.eulerAngles;
        GameObject dollyTracker = followPathCamera.cinemachineSmoothPath.gameObject;
        dollyTracker.transform.rotation = currentSelectedMiniCamera.rotation;
    }
}
