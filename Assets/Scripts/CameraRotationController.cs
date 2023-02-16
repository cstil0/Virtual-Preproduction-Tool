using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotationController : MonoBehaviour
{
    public GameObject currentSelectedMiniCamera;
    public GameObject currentSelectedCamera;
    public bool triggerOn = false;
    private bool triggerButtonDown = false;
    private bool isSelected = false;
    private Vector3 rotationPan = new Vector3(0.0f, 5.0f, 0.0f);
    private Vector3 rotationTilt = new Vector3(5.0f, 0.0f, 0.0f);

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger))
        {
            if (!triggerButtonDown && triggerOn)
            {
                isSelected = !isSelected;
                triggerButtonDown = true;
            }
        }
        else
        {
            triggerButtonDown = false;
        }
        if (OVRInput.Get(OVRInput.Button.PrimaryThumbstickRight) && isSelected)
        {
            currentSelectedMiniCamera.transform.Rotate(rotationPan * Time.deltaTime);
            changePointRotation();
        }
        if (OVRInput.Get(OVRInput.Button.PrimaryThumbstickLeft) && isSelected)
        {
            currentSelectedMiniCamera.transform.Rotate(-rotationPan * Time.deltaTime);
            changePointRotation();
        }
        if (OVRInput.Get(OVRInput.Button.PrimaryThumbstickUp) && isSelected)
        {
            currentSelectedMiniCamera.transform.Rotate(rotationTilt * Time.deltaTime);
            changePointRotation();
        }
        if (OVRInput.Get(OVRInput.Button.PrimaryThumbstickDown) && isSelected)
        {
            currentSelectedMiniCamera.transform.Rotate(rotationTilt * Time.deltaTime);
            changePointRotation();
        }
    }

    void changePointRotation()
    {

    }
}
