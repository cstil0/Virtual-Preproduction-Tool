using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LimitRotation : MonoBehaviour
{
    //private void OnTriggerEnter(Collider other)
    //{
    //    bool alreadyTriggered = other.GetComponent<CheckControllerTriggered>().alreadyTriggered;
    //    if (gameObject.transform.childCount > 0 && !alreadyTriggered)
    //        checkChildMaterials(gameObject, true);
    //}

    //private void OnTriggerExit(Collider other)
    //{
    //    if (gameObject.transform.childCount > 0)
    //        checkChildMaterials(gameObject, false);
    //}

    // Start is called before the first frame update
    void Start()
    {
        //LimitRotation.alreadyTriggered = false;
    }

    // Update is called once per frame
    void Update()
    {
        Transform attachPoint = gameObject.transform.GetChild(0);
        RotationScale rotScale = gameObject.GetComponent<RotationScale>();
        Vector3 rotation = rotScale.rotation;
        Vector3 currRot = gameObject.transform.rotation.eulerAngles;
        Vector3 limitRot = new Vector3(rotation.x, currRot.y, rotation.y);
        gameObject.transform.rotation = Quaternion.Euler(limitRot);

        // make it touch always the floor
        Vector3 position = gameObject.transform.position;
        gameObject.transform.position = new Vector3(position.x, -attachPoint.localPosition.y, position.z);

    }
}
