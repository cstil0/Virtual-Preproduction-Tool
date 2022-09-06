using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Pipes;
using System;
using System.IO;
using System.Text;
using System.Globalization;

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
        string specifier = "G";
        // ESTO ASÍ ES MUY FEO
        serverStream.WriteString(cameraPos.x.ToString(specifier, CultureInfo.InvariantCulture) + " " + cameraPos.y.ToString(specifier, CultureInfo.InvariantCulture) + " " + cameraPos.z.ToString(specifier, CultureInfo.InvariantCulture));
        Debug.Log("Position Sent!!");
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
        SendMessage();
    }
}
