using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Pipes;
using System;
using System.IO;
using UnityEngine.UI;

// pipes library was used previously to send messages from the multi-camera application to the LED screen one, but it blocks the application when executing it if both projects are not running
// therefore, currently both applications communicate sending the message first to the VR one and then re-sending it to the corresponding one
public class PipeSender : MonoBehaviour
{
    public Camera screenCamera;
    public int serverPort;
    public string ipAddress;

    Vector3 lastPos;
    public float sceneRotation;

    public GameObject OVRPlayer;


    public void sendDist(GameObject distSlider)
    {
        using (NamedPipeServerStream pipeServer = new NamedPipeServerStream("VPT", PipeDirection.Out))
        {
            pipeServer.WaitForConnection();
            using (StreamWriter sw = new StreamWriter(pipeServer))
            {
                sw.AutoFlush = true;
                int distanceValue = (int)Math.Ceiling(distSlider.GetComponent<Slider>().value);
                sw.WriteLine("DIST:" + distanceValue.ToString());
                pipeServer.Close();
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
