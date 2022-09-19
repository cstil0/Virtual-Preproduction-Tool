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
    bool resetStart;
    bool buttonDown;
    //StreamString streamString;

    void SendMessage()
    {
        //Create Server Instance
        NamedPipeServerStream server = new NamedPipeServerStream("MyCOMApp", PipeDirection.InOut, 1);
        //Wait for a client to connect
        server.WaitForConnection();
        //Created stream for reading and writing
        StreamString serverStream = new StreamString(server);

        string message = resetStart.ToString();
        serverStream.WriteString(message);

        string specifier = "G";
        Vector3 cameraPos = screenCamera.transform.position;
        message = cameraPos.ToString(specifier, CultureInfo.InvariantCulture);
        serverStream.WriteString(message);

        //Vector3 cameraRot = screenCamera.transform.rotation.eulerAngles;
        Quaternion cameraRot = screenCamera.transform.rotation;
        //message = Encoding.ASCII.GetBytes(cameraRot.x.ToString(specifier, CultureInfo.InvariantCulture) + " " + cameraRot.y.ToString(specifier, CultureInfo.InvariantCulture) + " " + cameraRot.z.ToString(specifier, CultureInfo.InvariantCulture));
        message = cameraRot.ToString(specifier, CultureInfo.InvariantCulture);
        serverStream.WriteString(message);

        //Close Connection
        server.Close();
    }

    // Start is called before the first frame update
    void Start()
    {
        buttonDown = false;
        SendMessage();
    }

    // Update is called once per frame
    void Update()
    {
        OVRInput.Update();

        if (OVRInput.Get(OVRInput.Button.Two))
        {
            SendMessage();
            if (!buttonDown)
                resetStart = true;
            else
                resetStart = false;

            buttonDown = true;
        }

        else if (buttonDown)
        {
            resetStart = false;
            buttonDown = false;
        }
    }
}
