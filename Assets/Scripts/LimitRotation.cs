using JetBrains.Annotations;
using Microsoft.MixedReality.Toolkit.Examples.Demos.EyeTracking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LimitRotation : MonoBehaviour
{
    Vector3 startPosDiff;
    GameObject currentHand;

    bool triggerOn;
    bool buttonDown;

    bool isBehindPlayer(Vector3 position)
    {
        Vector3 center = new Vector3(0.0f, 0.0f, 0.0f);

        if (position.x < center.x || position.z < center.z)
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

    public void objectSelected(GameObject handCollider)
    {
        currentHand = handCollider;
        triggerOn = true;
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

            Vector3 localPosition = currentHand.transform.localPosition + startPosDiff;
            Vector3 globalPosition = currentHand.transform.position + startPosDiff;
            position = isBehindPlayer(localPosition) ? gameObject.transform.position : globalPosition;

            RotationScale rotScale = gameObject.GetComponent<RotationScale>();
            Vector3 rotation = rotScale.rotation;
            Vector3 currRot = currentHand.transform.rotation.eulerAngles;
            limitRot = new Vector3(rotation.x, - currRot.y + rotation.y - 180.0f, rotation.y);
        }
        else
        {
            if (buttonDown)
            {
                triggerOn = false;
                buttonDown = false;
            }
            //alreadyTriggered = false;
            position = gameObject.transform.position;

            Vector3 rotation = gameObject.transform.rotation.eulerAngles;
            limitRot = rotation;
        }
        Transform attachPoint = gameObject.transform.GetChild(0);

        // make it touch always the floor
        gameObject.transform.position = new Vector3(position.x, -attachPoint.localPosition.y, position.z);


        gameObject.transform.rotation = Quaternion.Euler(limitRot);

    }
}
