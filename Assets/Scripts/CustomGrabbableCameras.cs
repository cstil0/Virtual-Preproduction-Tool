using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomGrabbableCameras : MonoBehaviour
{
    Vector3 startPosDiff;
    Vector3 startRotDiff;
    GameObject currentHand;
    Vector3 startRotation;
    Vector3 startPosition;
    Vector3 handStartPos;
    Vector3 handStartRot;

    [SerializeField] bool buttonDown;
    [SerializeField] bool triggerOn;
    [SerializeField] FollowPathCamera followPathCamera;
    [SerializeField] GameObject dollyTracker;
    [SerializeField] GameObject virtualCamera;
    [SerializeField] GameObject rotationController;

    public void objectSelected(GameObject handCollider, bool isTrigger)
    {
        // pass the current hand so that it can be used both with right and left controllers
        currentHand = handCollider;
        triggerOn = isTrigger;
    }

    // Start is called before the first frame update
    void Start()
    {
        buttonDown = false;
        triggerOn = false;

        startPosition = transform.position;
        startRotation = transform.rotation.eulerAngles;

        //alreadyTriggered = false;

        //LimitRotation.alreadyTriggered = false;
        //gameObject.GetComponent<OVRGrabbable>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 position = new Vector3();
        Vector3 rotation = new Vector3();
        if (!DefinePath.instance.isPlaying)
        {
            if (OVRInput.Get(OVRInput.Button.SecondaryHandTrigger) && triggerOn)
            {
                if (!buttonDown)
                {
                    handStartPos = currentHand.transform.position;
                    handStartRot = currentHand.transform.rotation.eulerAngles;
                    startPosDiff = virtualCamera.transform.position - handStartPos;
                    startRotDiff = virtualCamera.transform.rotation.eulerAngles - handStartRot;
                    startPosition = virtualCamera.transform.position;
                    startRotation = virtualCamera.transform.rotation.eulerAngles;
                    buttonDown = true;
                }

                Vector3 posDiff = handStartPos - currentHand.transform.position;
                Vector3 globalPosition = startPosition - posDiff;
                position = globalPosition;
                Vector3 rotDiff = handStartRot - currentHand.transform.rotation.eulerAngles;
                rotation = startRotation - rotDiff;
                //Vector3 globalPosition = currentHand.transform.position + startPosDiff;


                //position = isBehindPlayer() ? gameObject.transform.position : globalPosition;

                //rotation = startRotation;
                //Vector3 currRot = currentHand.transform.rotation.eulerAngles;
                //rotation = currRot + startRotDiff;
            }
            else
            {
                if (buttonDown)
                {
                    buttonDown = false;
                }
                //alreadyTriggered = false;
                position = gameObject.transform.position;

                rotation = gameObject.transform.rotation.eulerAngles;
            }

            dollyTracker.transform.position = position;
            virtualCamera.transform.position = position;
            rotationController.transform.position = position;

            //gameObject.transform.position = new Vector3(position.x, -attachPoint.localPosition.y, position.z);
            virtualCamera.transform.rotation = Quaternion.Euler(rotation);
            rotationController.transform.rotation = Quaternion.Euler(rotation);
        }
    }
}
