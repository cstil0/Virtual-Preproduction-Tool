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
        if (OVRInput.Get(OVRInput.Button.SecondaryHandTrigger) && triggerOn)
        {
            if (!buttonDown)
            {
                Vector3 handStartPos = currentHand.transform.position;
                startPosDiff = gameObject.transform.position - handStartPos;
                buttonDown = true;
            }

            position = currentHand.transform.position + startPosDiff;
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
        }
        Transform attachPoint = gameObject.transform.GetChild(0);

        // make it touch always the floor
        gameObject.transform.position = new Vector3(position.x, -attachPoint.localPosition.y, position.z);

        RotationScale rotScale = gameObject.GetComponent<RotationScale>();
        Vector3 rotation = rotScale.rotation;
        Vector3 currRot = gameObject.transform.rotation.eulerAngles;
        Vector3 limitRot = new Vector3(rotation.x, currRot.y, rotation.y);
        gameObject.transform.rotation = Quaternion.Euler(limitRot);

    }
}
