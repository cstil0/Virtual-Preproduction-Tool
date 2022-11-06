﻿//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using System;
//using System.Text;
//using System.Net;
//using System.Net.Sockets;
//using System.Threading;
//using UnityEngine.UI;
//using System.IO;
//using System.Globalization;

//public class UDPSender: MonoBehaviour
//{
//    // udpclient object
//    public Camera screenCamera;
//    UdpClient client;
//    public int serverPort;
//    public string ipAddress;

//    bool resetStart;
//    bool buttonDown;

//    Vector3 lastPos;

//    // main thread that listens to UDP messages through a defined port
//    void SendPosRot()
//    {
//        client = new UdpClient(serverPort);

//        //client.Connect(IPAddress.Parse(ipAddress), serverPort);

//        //IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), serverPort);

//        // sending data
//        IPEndPoint target = new IPEndPoint(IPAddress.Parse(ipAddress), serverPort);

//        byte[] message = Encoding.ASCII.GetBytes(resetStart.ToString());
//        client.Send(message, message.Length, target);

//        Vector3 cameraPos = screenCamera.transform.position;
//        string specifier = "G";
//        //byte[] message = Encoding.ASCII.GetBytes(cameraPos.ToString(specifier, CultureInfo.InvariantCulture) + " " + cameraPos.y.ToString(specifier, CultureInfo.InvariantCulture) + " " + cameraPos.z.ToString(specifier, CultureInfo.InvariantCulture));
//        message = Encoding.ASCII.GetBytes(cameraPos.ToString(specifier, CultureInfo.InvariantCulture));
//        client.Send(message, message.Length, target);

//        //Vector3 cameraRot = screenCamera.transform.rotation.eulerAngles;
//        Quaternion cameraRot = screenCamera.transform.rotation;
//        //message = Encoding.ASCII.GetBytes(cameraRot.x.ToString(specifier, CultureInfo.InvariantCulture) + " " + cameraRot.y.ToString(specifier, CultureInfo.InvariantCulture) + " " + cameraRot.z.ToString(specifier, CultureInfo.InvariantCulture));
//        message = Encoding.ASCII.GetBytes(cameraRot.ToString(specifier, CultureInfo.InvariantCulture));
//        client.Send(message, message.Length, target);

//        Debug.Log("messages sended");

//        client.Close();
//    }

//    // Start is called before the first frame update
//    void Start()
//    {
//        gameObject.SetActive(true);
//        buttonDown = false;
//        lastPos = screenCamera.transform.position;
//        SendPosRot();
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        Vector3 currentPos = screenCamera.transform.position;
//        if (lastPos != currentPos)
//        {
//            SendPosRot();

//            if (!buttonDown)
//                resetStart = true;
//            else
//                resetStart = false;

//            buttonDown = true;
//            lastPos = currentPos;
//        }

//        else if (buttonDown)
//        {
//            resetStart = false;
//            buttonDown = false;
//        }        
//    }
//}





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

public class UDPSender : MonoBehaviour
{
    // udpclient object
    public Camera screenCamera;
    UdpClient client;
    public int serverPort;
    public string ipAddress;
    Vector3 lastRot;

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
        // ESTO AS� ES MUY FEO
        byte[] message = Encoding.ASCII.GetBytes(cameraPos.x.ToString(specifier, CultureInfo.InvariantCulture) + " " + cameraPos.y.ToString(specifier, CultureInfo.InvariantCulture) + " " + cameraPos.z.ToString(specifier, CultureInfo.InvariantCulture));
        client.Send(message, message.Length, target);

        Vector3 rotation = screenCamera.transform.rotation.eulerAngles - lastRot;
        message = Encoding.ASCII.GetBytes(rotation.x.ToString(specifier, CultureInfo.InvariantCulture) + " " + rotation.y.ToString(specifier, CultureInfo.InvariantCulture) + " " + rotation.z.ToString(specifier, CultureInfo.InvariantCulture));
        Vector3 cameraRot = screenCamera.transform.rotation.eulerAngles;
        message = Encoding.ASCII.GetBytes(cameraRot.x.ToString(specifier, CultureInfo.InvariantCulture) + " " + cameraRot.y.ToString(specifier, CultureInfo.InvariantCulture) + " " + cameraRot.z.ToString(specifier, CultureInfo.InvariantCulture));
        client.Send(message, message.Length, target);
        lastRot = screenCamera.transform.rotation.eulerAngles;

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
