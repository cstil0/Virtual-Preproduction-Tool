﻿using UnityEngine;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Globalization;
using System.Collections;
using UnityEngine.UIElements;
using UnityEngine.Animations;
using System.Linq;
using System;
using System.Collections.Generic;
using Facebook.WitAi.CallbackHandlers;
using Microsoft.MixedReality.Toolkit.Input;

public class UDPSender : MonoBehaviour
{
    public static UDPSender instance = null;

    // udpclient object
    public Camera screenCamera;
    UdpClient client;
    public int serverPort = 8050;
    public int assistantToDirectorPort = 8051;
    int rotateScenePort = 8053;
    public string ipAddress;

    bool resetStart;
    int buttonDown;
    bool positionChanged;

    Vector3 screenCameraStartPos;
    Quaternion screenCameraStartRot;

    Vector3 lastPos;
    public float sceneRotation;

    public GameObject OVRPlayer;
    [SerializeField] HoverObjects hoverObjects;

    private void Awake()
    {
        if (instance)
        {
            if (instance != this)
                Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    // main thread that listens to UDP messages through a defined port
    public void SendPosRot()
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

    void sendSceneRotation(float rotationAngle)
    {
        try
        {
            // first send rotation to screen project
            UdpClient clientScreen = new UdpClient(serverPort);

            IPEndPoint targetScreen = new IPEndPoint(IPAddress.Parse(ipAddress), serverPort);

            Vector3 cameraRotation = screenCamera.transform.rotation.eulerAngles;
            Quaternion currentRotation = Quaternion.Euler(0.0f, cameraRotation.y + sceneRotation, 0.0f);
            byte[] message = Encoding.ASCII.GetBytes("SCENE_ROTATION:" + currentRotation.ToString());
            clientScreen.Send(message, message.Length, targetScreen);
            clientScreen.Close();

            // then, send rotation to director project (the constant value)
            UdpClient clientDir = new UdpClient(rotateScenePort);

            IPEndPoint targetDir = new IPEndPoint(IPAddress.Parse(ipAddress), rotateScenePort);

            message = Encoding.ASCII.GetBytes(Convert.ToString(rotationAngle));
            clientDir.Send(message, message.Length, targetDir);
            clientDir.Close();
        }
        catch (Exception e) { }
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
            client.Close();
        }
    }

    // ESTARIA BÉ FER UNA FUNCIÓ GENERAL QUE REBI EL MISSATGE I L'ENVII
    public void sendChangeCamera()
    {
        screenCameraStartPos = screenCamera.transform.position;
        screenCameraStartRot = screenCamera.transform.rotation;

        client = new UdpClient(serverPort);
        IPEndPoint target = new IPEndPoint(IPAddress.Parse(ipAddress), serverPort);
        byte[] message = Encoding.ASCII.GetBytes("CHANGE_CAMERA");
        client.Send(message, message.Length, target);
        client.Close();
    }

    public void sendResetPosRot()
    {
        SendPosRot();
        client = new UdpClient(serverPort);
        IPEndPoint target = new IPEndPoint(IPAddress.Parse(ipAddress), serverPort);
        byte[] message = Encoding.ASCII.GetBytes("RESET_POSROT");
        client.Send(message, message.Length, target);
        client.Close();
    }

    public void sendPointPath(GameObject item, Vector3 pathPoint)
    {
        try
        {
            client = new UdpClient(assistantToDirectorPort);

            string ipAddress = ModesManager.instance.IPAddress.text;
            // sending data
            IPEndPoint target = new IPEndPoint(IPAddress.Parse(ipAddress), assistantToDirectorPort);

            byte[] message = Encoding.ASCII.GetBytes("NEW_POINT: " + item.transform.name);
            client.Send(message, message.Length, target);

            message = Encoding.ASCII.GetBytes(pathPoint.x.ToString(CultureInfo.InvariantCulture) + " " + pathPoint.y.ToString(CultureInfo.InvariantCulture) + " " + pathPoint.z.ToString(CultureInfo.InvariantCulture));
            client.Send(message, message.Length, target);

            client.Close();
        }
        catch (System.Exception e) { }
    }

    public void sendItem(String name)
    {
        try
        {
            client = new UdpClient(assistantToDirectorPort);
            string ipAddress = ModesManager.instance.IPAddress.text;
            IPEndPoint target = new IPEndPoint(IPAddress.Parse(ipAddress), assistantToDirectorPort);
            byte[] message = Encoding.ASCII.GetBytes("NEW_ITEM:" + name);
            client.Send(message, message.Length, target);
            client.Close();
        }
        catch(System.Exception e) { }
    }

    IEnumerator sendInitialParameters()
    {
        sendCameraType();
        yield return new WaitForSeconds(5);
        //SendPosRot();
    }

    public void rotateItemsInScene(float rotationAngle)
    {
        Vector3 pivotPoint = OVRPlayer.transform.position;
        Quaternion rotationQuat = Quaternion.Euler(0.0f, rotationAngle, 0.0f);

        // get all items and characters in the scene
        GameObject[] sceneItems = GameObject.FindGameObjectsWithTag("Items");
        foreach (GameObject item in sceneItems)
        {
            // rotate item
            item.transform.RotateAround(pivotPoint, Vector3.up, rotationAngle);
            // rotate path points in the item
            FollowPath followPath = item.GetComponent<FollowPath>();
            if (followPath == null)
                continue;

            List<Vector3> pathPositions = followPath.pathPositions;
            for (int i = 0; i < pathPositions.Count; i++)
            {
                Vector3 point = pathPositions[i];
                pathPositions[i] = rotatePointAround(point, pivotPoint, rotationQuat);
            }
                
            item.GetComponent<FollowPath>().pathPositions = pathPositions;

            // rotate also the start point for the character
            Vector3 startPos = followPath.startPosition;
            followPath.startPosition = rotatePointAround(startPos, pivotPoint, rotationQuat);
        }

        GameObject[] sceneLines = GameObject.FindGameObjectsWithTag("Line");
        // get all lines in the scene and rotate all of their points
        foreach (GameObject line in sceneLines)
        {
            LineRenderer lineRenderer = line.GetComponent<LineRenderer>();
            Vector3[] pathPoints = new Vector3[lineRenderer.positionCount];
            lineRenderer.GetPositions(pathPoints);
            for (int i = 0; i < pathPoints.Length; i++)
            {
                Vector3 point = pathPoints[i];
                pathPoints[i] = rotatePointAround(point, pivotPoint, rotationQuat);
            }

            lineRenderer.SetPositions(pathPoints);
        }
    }

    Vector3 rotatePointAround(Vector3 point, Vector3 pivotPoint, Quaternion rotationQuat)
    {
        // move to the world center
        Vector3 dir = point - pivotPoint;
        // rotate
        dir = rotationQuat * dir;
        // go back to world coordinates
        return dir + pivotPoint;
    }

    // Start is called before the first frame update
    void Start()
    {
        screenCameraStartPos = screenCamera.transform.position;
        screenCameraStartRot = screenCamera.transform.rotation;

        gameObject.SetActive(true);
        positionChanged = true;
        resetStart = true;
        lastPos = screenCamera.transform.position;
        sceneRotation = 10;
        buttonDown = 0;
        sendCameraType();

        //StartCoroutine(sendInitialParameters());
    }

    // Update is called once per frame
    void Update()
    {
        int rotation = 0;
        Vector3 currentPos = screenCamera.transform.position;
        if (lastPos != currentPos)
        {

            if (!positionChanged)
                resetStart = true;
            else
                resetStart = false;

            SendPosRot();
            positionChanged = true;
            lastPos = currentPos;
        }

        else if (positionChanged)
        {
            resetStart = false;
            positionChanged = false;
        }

        if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger))
        {
            //if (!buttonDown)
            //{
            float rotationConstant = 30.0f * Time.deltaTime;
            // The way the secondary scene reads the angle is in total (not just the delta one)
            sceneRotation += rotationConstant;
            if (sceneRotation >= 360)
                sceneRotation = sceneRotation - 360;

            sendSceneRotation(rotationConstant);
            rotateItemsInScene(rotationConstant);

            buttonDown = 1;
            rotation = 5;

            //    //GameObject[] sceneItems = GameObject.FindGameObjectsWithTag("Items");

            //    //foreach (GameObject item in sceneItems)
            //    //{
            //    //    Vector3 itemPos = item.transform.position;
            //    //    Vector3 itemRot = item.transform.rotation.eulerAngles;

            //    //    item.transform.position = new Vector3(OVRPlayer.transform.position.x, itemPos.y, OVRPlayer.transform.position.z);
            //    //    item.transform.rotation = Quaternion.Euler(new Vector3(0.0f, rotation + itemRot.y , 0.0f));
            //    //    item.transform.position = item.transform.forward * itemPos.z;
            //    //    item.transform.position = item.transform.right * itemPos.x;
            //    //    //item.transform.RotateAround(OVRPlayer.transform.position, Vector3.up, 5*Time.deltaTime);
            //    //}

            //    //    buttonDown = true;
            //    //}
        }
        else
        {
                buttonDown = 0;
        }

        if (OVRInput.Get(OVRInput.RawButton.A) && !hoverObjects.itemAlreadySelected)
        {
            screenCamera.transform.position = screenCameraStartPos;
            screenCamera.transform.rotation = screenCameraStartRot;

            positionChanged = true;
            resetStart = true;

            sendResetPosRot();
            //SendPosRot();
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
