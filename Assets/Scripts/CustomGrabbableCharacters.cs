using JetBrains.Annotations;
using Microsoft.MixedReality.Toolkit.Examples.Demos.EyeTracking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CustomGrabbableCharacters : MonoBehaviour
{
    //Vector3 startPosDiff;
    Vector3 handStartPos;
    Vector3 handStartRot;
    Vector3 startPos;
    Vector3 startRot;
    GameObject currentHand;

    [SerializeField] bool triggerOn;
    [SerializeField] bool buttonDown;

    public void objectSelected(GameObject handCollider, bool isTrigger)
    {
        // pass the current hand so that it can be used both with right and left controllers
        currentHand = handCollider;
        triggerOn = isTrigger;
    }

    // Start is called before the first frame update
    void Start()
    {
        triggerOn = false;
        buttonDown = false;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 position = new Vector3();
        Vector3 limitRot = new Vector3();

        // characters should not be grabbed while being on play mode
        if (!DefinePath.instance.isPlaying)
        {
            if (OVRInput.Get(OVRInput.Button.SecondaryHandTrigger) && triggerOn)
            {
                if (!buttonDown)
                {
                    handStartPos = currentHand.transform.position;
                    handStartRot = currentHand.transform.rotation.eulerAngles;
                    startPos = gameObject.transform.position;
                    startRot = gameObject.transform.rotation.eulerAngles;
                    //startPosDiff = gameObject.transform.position - handStartPos;
                    buttonDown = true;
                }

                Vector3 posDiff = handStartPos - currentHand.transform.position;
                position = startPos - posDiff;
                //Vector3 globalPosition = currentHand.transform.position + startPosDiff;
                //position = globalPosition;

                //RotationScale rotScale = gameObject.GetComponent<RotationScale>();
                //Vector3 rotation = rotScale.rotation;
                Vector3 rotDiff = handStartRot - currentHand.transform.rotation.eulerAngles;;
                limitRot = new Vector3(startRot.x, startRot.y - rotDiff.y, startRot.z);
            }
            else
            {
                if (buttonDown)
                    buttonDown = false;

                position = gameObject.transform.position;

                Vector3 rotation = gameObject.transform.rotation.eulerAngles;
                limitRot = rotation;
            }

            // make it touch always the floor
            gameObject.transform.position = position;
            gameObject.transform.rotation = Quaternion.Euler(limitRot);
        }
    }
}
