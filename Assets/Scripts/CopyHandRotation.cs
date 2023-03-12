using Microsoft.MixedReality.Toolkit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyHandRotation : MonoBehaviour
{
    [SerializeField] GameObject handController;
    [SerializeField] GameObject dollyTracker;
    [SerializeField] FollowPathCamera followPathCamera;
    private bool triggerOn = false;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        triggerOn = followPathCamera.triggerOn;

        Vector3 dollyPosition = dollyTracker.transform.position;
        // this is safer than checking if button was pressed, since the hand will only move the first object that triggered
        if (dollyPosition != transform.position)
        {
            // since OVRGrabbable only works with the one that triggered with the OVRGrabber, we cannot have two OVRGrabbables at a time
            // therefore, we need to copy the hand rotation when the dolly tracker is grabbed because camera rotation is not managed through the dolly tracker
            if (triggerOn)
            {
                //Quaternion handRotation = Quaternion.Euler(- handController.transform.rotation.eulerAngles);
                //Quaternion cameraRotation = transform.rotation;

                transform.rotation = handController.transform.rotation;
                //transform.rotation = handRotation * cameraRotation;
            }
            //transform.position = dollyTracker.transform.position;
            transform.position = dollyPosition;
            dollyTracker.transform.rotation = transform.rotation;
        }
    }
}
