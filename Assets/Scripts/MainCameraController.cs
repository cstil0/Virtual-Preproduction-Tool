using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCameraController : MonoBehaviour
{
    public GameObject controller;
    Vector3 cameraStartPos;
    Vector3 controllerStartPos;
    //Vector3 lastRotation

    // Start is called before the first frame update
    void Start()
    {
        cameraStartPos = gameObject.transform.position;
        controllerStartPos = controller.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        OVRInput.Update();

        //if (OVRInput.Get(OVRInput.Button.Two))
        //{
            Vector3 diffPos = controller.transform.position - controllerStartPos;
            gameObject.transform.position = cameraStartPos + diffPos;
            
            gameObject.transform.rotation = controller.transform.rotation;
        //}
    }

}
