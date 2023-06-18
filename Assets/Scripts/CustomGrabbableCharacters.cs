using JetBrains.Annotations;
using Microsoft.MixedReality.Toolkit.Examples.Demos.EyeTracking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// this scripts substitutes the Oculus OVRGrabbable script, in order to limit rotation allow for rotation only in the y-axis
public class CustomGrabbableCharacters : MonoBehaviour
{
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

    void Start()
    {
        triggerOn = false;
        buttonDown = false;
    }

    void Update()
    {
        Vector3 position = new Vector3();
        Vector3 limitRot = new Vector3();

        // characters should not be grabbed while being on play mode
        if (!DefinePath.instance.isPlaying)
        {
            if (OVRInput.Get(OVRInput.Button.SecondaryHandTrigger) && triggerOn && !DirectorPanelManager.instance.isGridShown)
            {
                if (!buttonDown)
                {
                    // save the initial position and rotation for both the hand and the character
                    handStartPos = currentHand.transform.position;
                    handStartRot = currentHand.transform.rotation.eulerAngles;
                    startPos = gameObject.transform.position;
                    startRot = gameObject.transform.rotation.eulerAngles;
                    buttonDown = true;
                }

                // compute how much the character should move and rotate based on the controller's difference
                Vector3 posDiff = handStartPos - currentHand.transform.position;
                position = startPos - posDiff;
                Vector3 rotDiff = handStartRot - currentHand.transform.rotation.eulerAngles;
                // consider only hand's rotation in the y axis while mantaining x and z
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
