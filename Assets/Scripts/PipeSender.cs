using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Pipes;
using System;
using System.IO;
using System.Text;

public class PipeSender : MonoBehaviour
{
    public Camera screenCamera;
    //StreamString streamString;

    void SendMessage()
    {
        //Create Server Instance
        NamedPipeServerStream server = new NamedPipeServerStream("MyCOMApp", PipeDirection.InOut, 1);
        //Wait for a client to connect
        server.WaitForConnection();
        //Created stream for reading and writing
        StreamString serverStream = new StreamString(server);
        //Send Message to Client
        Vector3 cameraPos = screenCamera.transform.position;
        serverStream.WriteString(cameraPos.x.ToString() + " " + cameraPos.y.ToString() + " " + cameraPos.z.ToString());
        //Read from Client
        string dataFromClient = serverStream.ReadString();
        UnityEngine.Debug.Log("Received from Client: " + dataFromClient);
        //Close Connection
        server.Close();
    }

    // Start is called before the first frame update
    void Start()
    {
        SendMessage();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
