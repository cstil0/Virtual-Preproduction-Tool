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

    // main thread that listens to UDP messages through a defined port
    void UDPTest()
    {
        client = new UdpClient(serverPort);
        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, serverPort);

        // sending data
        IPEndPoint target = new IPEndPoint(IPAddress.Parse(ipAddress), serverPort);
        //int count = 4;
        // send a couple of sample messages:
        //for (int num = 1; num <= count; num++)
        //{
        //    byte[] message = new byte[num];
        //    client.Send(message, message.Length, target);
        //    //Debug.Log("Sent: " + message);
        //}
        Vector3 cameraPos = screenCamera.transform.position;
        string specifier = "G";
        // ESTO ASÍ ES MUY FEO
        byte[] message = Encoding.ASCII.GetBytes(cameraPos.x.ToString(specifier, CultureInfo.InvariantCulture) + " " + cameraPos.y.ToString(specifier, CultureInfo.InvariantCulture) + " " + cameraPos.z.ToString(specifier, CultureInfo.InvariantCulture));
        client.Send(message, message.Length, target);

        Vector3 cameraRot = screenCamera.transform.rotation.eulerAngles;
        message = Encoding.ASCII.GetBytes(cameraRot.x.ToString(specifier, CultureInfo.InvariantCulture) + " " + cameraRot.y.ToString(specifier, CultureInfo.InvariantCulture) + " " + cameraRot.z.ToString(specifier, CultureInfo.InvariantCulture));
        client.Send(message, message.Length, target);

        client.Close();
    }

    // Start is called before the first frame update
    void Start()
    {
        UDPTest();
    }

    // Update is called once per frame
    void Update()
    {
        UDPTest();
    }
}
