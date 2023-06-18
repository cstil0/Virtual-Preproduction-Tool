using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


// this script was used during motion parallax testing in Sala Aranyó, by mapping the controller movements to the camera in the set to generate the tracking
public class MainCameraController : MonoBehaviour
{
    public GameObject controller;
    Vector3 cameraStartPos;
    Vector3 controllerStartPos;

    Quaternion cameraStartRot;
    Quaternion controllerStartRot;

    bool buttonDown;
    bool currentlySending;

    [SerializeField] HoverObjects hoverObjects;

    void Start()
    {
        buttonDown = false;
        currentlySending = false;
    }

    void Update()
    {
        OVRInput.Update();

        // after pressing the B button from the controller, the main camera 1 can be controlled by moving the user's hand
        // this was used for testing the LED wall movement during testing performed at Sala Aranyó
        if (OVRInput.Get(OVRInput.RawButton.B) && gameObject.name == "MainCamera 1" && !hoverObjects.itemAlreadySelected)
        {
            if (!buttonDown)
                currentlySending = !currentlySending;

            if (currentlySending) {
                // save the start position and rotation of both the controller and the camera, so that it resets every time the button is pressed
                cameraStartPos = gameObject.transform.position;
                controllerStartPos = controller.transform.position;

                cameraStartRot = gameObject.transform.rotation;
                controllerStartRot = controller.transform.rotation;
            }
            buttonDown = true;
        }
        else if (buttonDown)
        {
            buttonDown = false;
        }
        if (currentlySending)
        {
            // Compute the new rotation and position of the camera taking the difference with respect to the original one
            // In this way they are not affecting the "ideal" start position and orientation of each other
            Vector3 diffPos = controller.transform.position - controllerStartPos;
            gameObject.transform.position = cameraStartPos + diffPos;

            // Quaternion sum and substraction are done with prodtucts
            Quaternion diffRot = controller.transform.rotation * Quaternion.Inverse(controllerStartRot);
            gameObject.transform.rotation = diffRot * cameraStartRot;
        }
    }

}
