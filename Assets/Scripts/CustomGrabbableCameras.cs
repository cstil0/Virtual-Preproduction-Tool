using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomGrabbableCameras : MonoBehaviour
{
    GameObject currentHand;
    Quaternion startRotation;
    //Vector3 startRotation;
    Vector3 startPosition;
    Vector3 handStartPos;
    //Vector3 handStartRot;
    Quaternion handStartRot;

    [SerializeField] bool buttonDown;
    [SerializeField] bool triggerOn;
    [SerializeField] FollowPathCamera followPathCamera;
    public GameObject dollyTracker;
    public GameObject virtualCamera;
    public GameObject rotationController;

    public void objectSelected(GameObject handCollider, bool isTrigger)
    {
        // pass the current hand so that it can be used both with right and left controllers
        currentHand = handCollider;
        triggerOn = isTrigger;
    }

    void Start()
    {
        buttonDown = false;
        triggerOn = false;

        startPosition = transform.position;
        startRotation = transform.rotation;
        //startRotation = transform.rotation.eulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 position = new Vector3();
        Quaternion rotation = new Quaternion();
        //Vector3 rotation = new Vector3();
        
        // cameras should not be able to be grabbed while being on play mode
        if (!DefinePath.instance.isPlaying)
        {
            if (OVRInput.Get(OVRInput.Button.SecondaryHandTrigger) && triggerOn && !DirectorPanelManager.instance.isGridShown)
            {
                if (!buttonDown)
                {
                    // get position and rotation of both of the virtual camera and hand when the controller hits the camera
                    handStartPos = currentHand.transform.position;
                    handStartRot = currentHand.transform.rotation;
                    //handStartRot = currentHand.transform.rotation.eulerAngles;
                    startPosition = virtualCamera.transform.position;
                    startRotation = virtualCamera.transform.rotation;
                    //startRotation = virtualCamera.transform.rotation.eulerAngles;
                    buttonDown = true;

                    UDPSender.instance.changeResetStart(true);
                    UDPSender.instance.SendPosRot();
                }

                // compute how much the camera should move and rotate based on the difference between the current and the initial one from the hand
                Vector3 posDiff = handStartPos - currentHand.transform.position;
                position = startPosition - posDiff;
                Quaternion rotDiff = Quaternion.Inverse(handStartRot) * currentHand.transform.rotation;
                //Vector3 rotDiff = handStartRot - currentHand.transform.rotation.eulerAngles;
                rotation = startRotation * rotDiff;
                //Vector3 rotationCorrected = correctRotation(rotation.normalized);

                // apply position and rotation to each involved gameobject
                dollyTracker.transform.position = position;
                virtualCamera.transform.position = position;
                rotationController.transform.position = position;

                virtualCamera.transform.rotation = rotation;
                //virtualCamera.transform.rotation = Quaternion.Euler(rotationCorrected);
                rotationController.transform.rotation = rotation;
                //rotationController.transform.rotation = Quaternion.Euler(rotationCorrected);
            }
            else
            {
                // if the camera is not being grabbed, ensure that all gameobjects have the same position and rotation
                if (buttonDown)
                    buttonDown = false;

                position = virtualCamera.transform.position;
                rotation = virtualCamera.transform.rotation;
            }
        }
    }

    Vector3 correctRotation(Vector3 cameraRotation)
    {
        Vector3 correctedRotation = new Vector3(cameraRotation.x, cameraRotation.y, cameraRotation.z);
        while (correctedRotation.x < 0.0f)
            correctedRotation.x += 360.0f;
        while (correctedRotation.x > 360.0f)
            correctedRotation.x -= 360.0f;

        while (correctedRotation.y < 0.0f)
            correctedRotation.y += 360.0f;
        while (correctedRotation.y > 360.0f)
            correctedRotation.y -= 360.0f;

        while (correctedRotation.z < 0.0f)
            correctedRotation.z += 360.0f;
        while (correctedRotation.z > 360.0f)
            correctedRotation.z -= 360.0f;

        return correctedRotation;
    }
}
