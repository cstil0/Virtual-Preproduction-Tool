using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimitRotation : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        RotationScale rotScale = gameObject.GetComponent<RotationScale>();
        Vector3 rotation = rotScale.rotation;
        Vector3 currRot = gameObject.transform.rotation.eulerAngles;
        Vector3 limitRot = new Vector3(rotation.x, currRot.y, rotation.y);
        gameObject.transform.rotation = Quaternion.Euler(limitRot);

        // make it touch always the floor
        Vector3 position = gameObject.transform.position;
        gameObject.transform.position = new Vector3(position.x, 0.0f, position.z);

    }
}
