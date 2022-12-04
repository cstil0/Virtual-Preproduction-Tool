using JetBrains.Annotations;
using Microsoft.MixedReality.Toolkit.Examples.Demos.EyeTracking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LimitPositionRotation : MonoBehaviour
{
    Vector3 startPosDiff;
    GameObject currentHand;

    bool triggerOn;
    bool buttonDown;

    bool isBehindPlayer()
    {
        Transform OVRPlayer = GameObject.Find("OVRPlayerController").transform;
        // get hand position in local coordinates of OVRPlayer
        Vector3 localPosition = OVRPlayer.InverseTransformPoint(currentHand.transform.position);

        if (localPosition.z < 0.2f)
            return true;
        else
            return false;
    }

    //bool alreadyTriggered;
    private void OnTriggerEnter(Collider other)
    {
        //bool alreadyTriggered = other.GetComponent<HoverObjects>().alreadyTriggered;
        //if (other.gameObject.layer == 3 && !alreadyTriggered)
        //{

        //}
    }

    public void objectSelected(GameObject handCollider, bool isSelected)
    {
        // pass the current hand so that it can be used both with right and left controllers
        currentHand = handCollider;
        triggerOn = isSelected;
    }

    // Start is called before the first frame update
    void Start()
    {
        triggerOn = false;
        buttonDown = false;

        //alreadyTriggered = false;

        //LimitRotation.alreadyTriggered = false;
        //gameObject.GetComponent<OVRGrabbable>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 position = new Vector3();
        Vector3 limitRot = new Vector3();

        if (OVRInput.Get(OVRInput.Button.SecondaryHandTrigger) && triggerOn)
        {
            if (!buttonDown)
            {
                Vector3 handStartPos = currentHand.transform.position;
                startPosDiff = gameObject.transform.position - handStartPos;
                buttonDown = true;
            }

            Vector3 globalPosition = currentHand.transform.position + startPosDiff;
            //position = isBehindPlayer() ? gameObject.transform.position : globalPosition;
            position = globalPosition;

            RotationScale rotScale = gameObject.GetComponent<RotationScale>();
            Vector3 rotation = rotScale.rotation;
            Vector3 currRot = currentHand.transform.rotation.eulerAngles;
            limitRot = new Vector3(-rotation.x, - currRot.y + rotation.y, -rotation.z);
        }
        else
        {
            if (buttonDown)
            {
                buttonDown = false;
            }
            //alreadyTriggered = false;
            position = gameObject.transform.position;

            Vector3 rotation = gameObject.transform.rotation.eulerAngles;
            limitRot = rotation;
        }
        Transform attachPoint = gameObject.transform.GetChild(0);

        // make it touch always the floor
        gameObject.transform.position = new Vector3(position.x, position.y, position.z);
        //gameObject.transform.position = new Vector3(position.x, -attachPoint.localPosition.y, position.z);


        gameObject.transform.rotation = Quaternion.Euler(limitRot);

    }
}
