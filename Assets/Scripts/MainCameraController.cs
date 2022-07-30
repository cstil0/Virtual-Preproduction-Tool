using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCameraController : MonoBehaviour
{
    public GameObject controller;
    public Vector3 startPosition;

    // Start is called before the first frame update
    void Start()
    {
        gameObject.transform.position = startPosition;
    }

    // Update is called once per frame
    void Update()
    {
        OVRInput.Update();

        if (OVRInput.Get(OVRInput.Button.Two))
            gameObject.transform.position = startPosition + controller.transform.position;
    }

}
