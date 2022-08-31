using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCameraController : MonoBehaviour
{
    public GameObject controller;
    Vector3 cameraStartPos;
    Vector3 controllerStartPos;

    Quaternion cameraStartRot;
    Quaternion controllerStartRot;

    bool buttonDown;

    // Start is called before the first frame update
    void Start()
    {
        buttonDown = false;
    }

    // Update is called once per frame
    void Update()
    {
        OVRInput.Update();

        if (OVRInput.Get(OVRInput.Button.Two))
        {
            if (!buttonDown)
            {
                // save the start position and rotation of both the controller and the camera, so that it resets every time the button is pressed
                cameraStartPos = gameObject.transform.position;
                controllerStartPos = controller.transform.position;

                cameraStartRot = gameObject.transform.rotation;
                controllerStartRot = controller.transform.rotation;
            }

            // Compute the new rotation and position of the camera taking the difference with respect to the original one
            // In this way they are not affecting the "ideal" start position and orientation of each other
            Vector3 diffPos = controller.transform.position - controllerStartPos;
            gameObject.transform.position = cameraStartPos + diffPos;

            // Quaternion sum and substraction are done with prodtucts
            Quaternion diffRot = controller.transform.rotation * Quaternion.Inverse(controllerStartRot);
            gameObject.transform.rotation = diffRot * cameraStartRot;

            buttonDown = true;
        }
        else if (buttonDown)
        {
            buttonDown = false;
        }
    }

}
