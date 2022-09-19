using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine.UI;
using System.IO;
using System.Globalization;

public class UDPSender: MonoBehaviour
{
    // udpclient object
    public Camera screenCamera;
    UdpClient client;
    public int serverPort;
    public string ipAddress;

    bool resetStart;
    bool buttonDown;

    // main thread that listens to UDP messages through a defined port
    void UDPTest()
    {
        client = new UdpClient(serverPort);
        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, serverPort);

        // sending data
        IPEndPoint target = new IPEndPoint(IPAddress.Parse(ipAddress), serverPort);

        byte[] message = Encoding.ASCII.GetBytes(resetStart.ToString());
        client.Send(message, message.Length, target);

        Vector3 cameraPos = screenCamera.transform.position;
        string specifier = "G";
        //byte[] message = Encoding.ASCII.GetBytes(cameraPos.ToString(specifier, CultureInfo.InvariantCulture) + " " + cameraPos.y.ToString(specifier, CultureInfo.InvariantCulture) + " " + cameraPos.z.ToString(specifier, CultureInfo.InvariantCulture));
        message = Encoding.ASCII.GetBytes(cameraPos.ToString(specifier, CultureInfo.InvariantCulture));
        client.Send(message, message.Length, target);

        //Vector3 cameraRot = screenCamera.transform.rotation.eulerAngles;
        Quaternion cameraRot = screenCamera.transform.rotation;
        //message = Encoding.ASCII.GetBytes(cameraRot.x.ToString(specifier, CultureInfo.InvariantCulture) + " " + cameraRot.y.ToString(specifier, CultureInfo.InvariantCulture) + " " + cameraRot.z.ToString(specifier, CultureInfo.InvariantCulture));
        message = Encoding.ASCII.GetBytes(cameraRot.ToString(specifier, CultureInfo.InvariantCulture));
        client.Send(message, message.Length, target);

        client.Close();
    }

    // Start is called before the first frame update
    void Start()
    {
        buttonDown = false;
        UDPTest();
    }

    // Update is called once per frame
    void Update()
    {
        OVRInput.Update();

        if (OVRInput.Get(OVRInput.Button.Two))
        {
            UDPTest();

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
