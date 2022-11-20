using UnityEngine;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Globalization;
using UnityEngine.InputSystem.Controls;
using Unity.VisualScripting;
using Facebook.WitAi;
using System.Collections;

public class UDPSender : MonoBehaviour
{
    // udpclient object
    public Camera screenCamera;
    UdpClient client;
    public int serverPort;
    public string ipAddress;

    bool resetStart;
    int buttonDown;
    bool positionChanged;

    Vector3 lastPos;
    public float sceneRotation;

    public GameObject OVRPlayer;

    // main thread that listens to UDP messages through a defined port
    void SendPosRot()
    {
        client = new UdpClient(serverPort);

        //client.Connect(IPAddress.Parse(ipAddress), serverPort);

        //IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), serverPort);

        // sending data
        IPEndPoint target = new IPEndPoint(IPAddress.Parse(ipAddress), serverPort);

        byte[] message = Encoding.ASCII.GetBytes("CAMERA_INFO:" + resetStart.ToString());
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

    void sendSceneRotation()
    {
        client = new UdpClient(serverPort);

        IPEndPoint target = new IPEndPoint(IPAddress.Parse(ipAddress), serverPort);

        Vector3 cameraRotation = screenCamera.transform.rotation.eulerAngles;
        Quaternion currentRotation = Quaternion.Euler(0.0f, cameraRotation.y + sceneRotation, 0.0f);
        byte[] message = Encoding.ASCII.GetBytes("SCENE_ROTATION:" + currentRotation.ToString());
        client.Send(message, message.Length, target);
        client.Close();
    }

    void sendCameraType()
    {
        ModesManager modesManager = GameObject.Find("Modes Manager").GetComponent<ModesManager>();
        if (modesManager.role == ModesManager.eRoleType.ASSISTANT)
        {
            client = new UdpClient(serverPort);
            IPEndPoint target = new IPEndPoint(IPAddress.Parse(ipAddress), serverPort);

            if (modesManager.mode == ModesManager.eModeType.MIXEDREALITY)
            {
                byte[] message = Encoding.ASCII.GetBytes("SEND_DISPLAY");
                client.Send(message, message.Length, target);
            }
            else if (modesManager.mode == ModesManager.eModeType.VIRTUALREALITY)
            {
                byte[] message = Encoding.ASCII.GetBytes("SEND_NDI");
                client.Send(message, message.Length, target);
            }

        }
        client.Close();
    }

    IEnumerator sendInitialParameters()
    {
        sendCameraType();
        yield return new WaitForSeconds(5);
        SendPosRot();
    }

    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(true);
        positionChanged = false;
        lastPos = screenCamera.transform.position;
        sceneRotation = 10;
        buttonDown = 0;
        StartCoroutine(sendInitialParameters());
    }

    // Update is called once per frame
    void Update()
    {
        int rotation = 0;
        Vector3 currentPos = screenCamera.transform.position;
        if (lastPos != currentPos)
        {
            SendPosRot();

            if (!positionChanged)
                resetStart = true;
            else
                resetStart = false;

            positionChanged = true;
            lastPos = currentPos;
        }

        else if (positionChanged)
        {
            resetStart = false;
            positionChanged = false;
        }

        if (OVRInput.Get(OVRInput.Button.Four))
        {
            //if (!buttonDown)
            //{
            sceneRotation = sceneRotation + 5;
            if (sceneRotation >= 360)
                sceneRotation = sceneRotation - 360;

            sendSceneRotation();

            buttonDown = 1;
            rotation = 5;

            //GameObject[] sceneItems = GameObject.FindGameObjectsWithTag("Items");

            //foreach (GameObject item in sceneItems)
            //{
            //    Vector3 itemPos = item.transform.position;
            //    Vector3 itemRot = item.transform.rotation.eulerAngles;

            //    item.transform.position = new Vector3(OVRPlayer.transform.position.x, itemPos.y, OVRPlayer.transform.position.z);
            //    item.transform.rotation = Quaternion.Euler(new Vector3(0.0f, rotation + itemRot.y , 0.0f));
            //    item.transform.position = item.transform.forward * itemPos.z;
            //    item.transform.position = item.transform.right * itemPos.x;
            //    //item.transform.RotateAround(OVRPlayer.transform.position, Vector3.up, 5*Time.deltaTime);
            //}

            //    buttonDown = true;
            //}
        }
        else
        {
            buttonDown = 0;
        }



        //GameObject[] sceneItems = GameObject.FindGameObjectsWithTag("Items");

        //foreach (GameObject item in sceneItems)
        //{
        //    Vector3 itemPos = item.transform.position;
        //    Vector3 itemRot = item.transform.rotation.eulerAngles;

        //    item.transform.position = new Vector3(OVRPlayer.transform.position.x, itemPos.y, OVRPlayer.transform.position.z);
        //    item.transform.rotation = Quaternion.Euler(new Vector3(0.0f, rotation + itemRot.y, 0.0f));
        //    item.transform.position = item.transform.forward * (itemPos.z + OVRPlayer.transform.position.z);
        //    item.transform.position = item.transform.right * (itemPos.x + OVRPlayer.transform.position.x);

        //    item.transform.RotateAround(OVRPlayer.transform.position, Vector3.up, rotation);
        //}

    }
}





//using System.Collections;
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

//public class UDPSender : MonoBehaviour
//{
//    // udpclient object
//    public Camera screenCamera;
//    UdpClient client;
//    public int serverPort;
//    public string ipAddress;
//    Vector3 lastRot;

//    // main thread that listens to UDP messages through a defined port
//    void UDPTest()
//    {
//        client = new UdpClient(serverPort);
//        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, serverPort);

//        // sending data
//        IPEndPoint target = new IPEndPoint(IPAddress.Parse(ipAddress), serverPort);
//        //int count = 4;
//        // send a couple of sample messages:
//        //for (int num = 1; num <= count; num++)
//        //{
//        //    byte[] message = new byte[num];
//        //    client.Send(message, message.Length, target);
//        //    //Debug.Log("Sent: " + message);
//        //}
//        Vector3 cameraPos = screenCamera.transform.position;
//        string specifier = "G";
//        // ESTO AS� ES MUY FEO
//        byte[] message = Encoding.ASCII.GetBytes(cameraPos.x.ToString(specifier, CultureInfo.InvariantCulture) + " " + cameraPos.y.ToString(specifier, CultureInfo.InvariantCulture) + " " + cameraPos.z.ToString(specifier, CultureInfo.InvariantCulture));
//        client.Send(message, message.Length, target);

//        Vector3 rotation = screenCamera.transform.rotation.eulerAngles - lastRot;
//        message = Encoding.ASCII.GetBytes(rotation.x.ToString(specifier, CultureInfo.InvariantCulture) + " " + rotation.y.ToString(specifier, CultureInfo.InvariantCulture) + " " + rotation.z.ToString(specifier, CultureInfo.InvariantCulture));
//        Vector3 cameraRot = screenCamera.transform.rotation.eulerAngles;
//        message = Encoding.ASCII.GetBytes(cameraRot.x.ToString(specifier, CultureInfo.InvariantCulture) + " " + cameraRot.y.ToString(specifier, CultureInfo.InvariantCulture) + " " + cameraRot.z.ToString(specifier, CultureInfo.InvariantCulture));
//        client.Send(message, message.Length, target);
//        lastRot = screenCamera.transform.rotation.eulerAngles;

//        client.Close();
//    }

//    // Start is called before the first frame update
//    void Start()
//    {
//        UDPTest();
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        UDPTest();
//    }
//}
