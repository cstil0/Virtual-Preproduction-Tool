using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCameraController : MonoBehaviour
{
    public GameObject controller;
    Vector3 cameraStartPos;
    Vector3 controllerStartPos;

    Vector3 cameraStartRot;
    Vector3 controllerStartRot;

    bool buttonDown;

    //Vector3 lastRotation

    // Start is called before the first frame update
    void Start()
    {
        cameraStartPos = gameObject.transform.position;
        controllerStartPos = controller.transform.position;

        cameraStartRot = gameObject.transform.rotation.eulerAngles;
        controllerStartRot = controller.transform.rotation.eulerAngles;

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
                cameraStartPos = gameObject.transform.position;
                controllerStartPos = controller.transform.position;

                cameraStartRot = gameObject.transform.rotation.eulerAngles;
                controllerStartRot = controller.transform.rotation.eulerAngles;
            }

            Vector3 diffPos = controller.transform.position - controllerStartPos;
            gameObject.transform.position = cameraStartPos + diffPos;

            Vector3 diffRot = controller.transform.rotation.eulerAngles - controllerStartRot;
            gameObject.transform.rotation = Quaternion.Euler(cameraStartRot + diffRot);

            buttonDown = true;
        }
        else if (buttonDown)
        {
            buttonDown = false;
        }
    }

}
