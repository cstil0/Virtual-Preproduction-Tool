using UnityEngine;
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
using UnityEditor.Rendering;

public class UDPSender : MonoBehaviour
{
    public static UDPSender instance = null;

    // udpclient object
    public Camera screenCamera;
    UdpClient client;
    public int assistantToScreenPort = 8050;
    public int assistantToDirectorPort = 8051;
    public int directoToAssistantPort = 8052;
    public string ipAddress;

    bool resetStart;
    int buttonDown;
    bool positionChanged;
    int posChangedCount = 0;

    Vector3 screenCameraStartPos;
    Quaternion screenCameraStartRot;

    Vector3 lastPos;
    public float sceneRotation;

    public GameObject OVRPlayer;
    [SerializeField] HoverObjects hoverObjects;
    [SerializeField] GameObject itemsParent;

    private void Awake()
    {
        if (instance)
        {
            if (instance != this)
                Destroy(gameObject);
        }
        else
            instance = this;
    }

    private void sendInfo(int port, string messageString)
    {
        string ipAddress = ModesManager.instance.IPAddress.text;
        client = new UdpClient(port);
        IPEndPoint target = new IPEndPoint(IPAddress.Parse(ipAddress), port);

        byte[] message = Encoding.ASCII.GetBytes(messageString);
        client.Send(message, message.Length, target);
        client.Close();
    }

    // -- DIRECTOR TO ASSISTANT MESSAGES --
    public void changeMainCamera(int cameraNum)
    {
        GameObject currentCamera = itemsParent.transform.Find("MainCamera " + cameraNum).gameObject;
        screenCamera = currentCamera.GetComponent<Camera>();

        sendInfo(directoToAssistantPort, "CHANGE_CAMERA:" + cameraNum);
    }

    public void sendChangeScreenDistance(float distance)
    {
        sendInfo(directoToAssistantPort, "CHANGE_SCREEN_DISTANCE:" + distance);
    }

    public void sendDeletePoint(int pointNum, string name)
    {
        try
        {
            sendInfo(directoToAssistantPort, "DELETE_POINT:" + name + ":" + pointNum);
        }
        catch (System.Exception e) { }
    }

    public void sendDeleteItemToAssistant(string name)
    {
        try
        {
            sendInfo(directoToAssistantPort, "DELETE_ITEM:" + name);
        }
        catch (System.Exception e) { }
    }

    public void sendChangeSpeed(float speed, string name)
    {
        try
        {
            sendInfo(directoToAssistantPort, "CHANGE_SPEED:" + name + ":" + speed);
        }
        catch (System.Exception e) { }
    }

    public void sendShowHideGridAssistant(bool isShowed)
    {
        sendInfo(directoToAssistantPort, "SHOW_HIDE_GRID:" + isShowed);
    }

    public void sendChangeLightColor(string focusName, Color color, bool isAccepted)
    {
        string colorHex = UnityEngine.ColorUtility.ToHtmlStringRGBA(color);
        sendInfo(directoToAssistantPort, "CHANGE_LIGHT_COLOR:" + focusName + ":" + colorHex + ":" + isAccepted);
    }

    public void sendChangeLightIntensity(string focusName, float intensity)
    {
        sendInfo(directoToAssistantPort, "CHANGE_LIGHT_INTENSITY:" + focusName + ":" + intensity);
    }

    // -- ASSISTANT TO SCREEN MESSAGES --
    // main thread that listens to UDP messages through a defined port
    public void SendPosRot()
    {
        client = new UdpClient(assistantToScreenPort);

        // sending data
        IPEndPoint target = new IPEndPoint(IPAddress.Parse(ipAddress), assistantToScreenPort);
        byte[] message = Encoding.ASCII.GetBytes("CAMERA_INFO:" + resetStart.ToString());
        client.Send(message, message.Length, target);

        Vector3 cameraPos = screenCamera.transform.position;
        string specifier = "G";
        message = Encoding.ASCII.GetBytes(cameraPos.ToString(specifier, CultureInfo.InvariantCulture));
        client.Send(message, message.Length, target);

        Quaternion cameraRot = screenCamera.transform.rotation;
        message = Encoding.ASCII.GetBytes(cameraRot.ToString(specifier, CultureInfo.InvariantCulture));
        client.Send(message, message.Length, target);

        client.Close();
    }

    void sendSceneRotation(float rotationAngle)
    {
        try
        {
            Vector3 cameraRotation = screenCamera.transform.rotation.eulerAngles;
            Quaternion currentRotation = Quaternion.Euler(0.0f, cameraRotation.y + sceneRotation, 0.0f);
            sendInfo(assistantToScreenPort, "SCENE_ROTATION:" + currentRotation.ToString());
            sendInfo(assistantToDirectorPort, "SCENE_ROTATION:" + currentRotation.ToString());
        }
        catch (Exception e) { }
    }

    public void sendChangeCameraScreen(string cameraNum)
    {
        GameObject currentCamera = itemsParent.transform.Find("MainCamera " + cameraNum).gameObject;
        screenCamera = currentCamera.GetComponent<Camera>();
        sendInfo(assistantToScreenPort, "CHANGE_CAMERA: " + cameraNum);
    }

    public void sendChangeScreenDistanceAssistant(string changeDistanceMessage)
    {
        sendInfo(assistantToScreenPort, "CHANGE_SCREEN_DISTANCE:" + changeDistanceMessage);
    }

    void sendCameraType()
    {
        ModesManager modesManager = GameObject.Find("Modes Manager").GetComponent<ModesManager>();
        if (modesManager.role == ModesManager.eRoleType.ASSISTANT)
        {
            client = new UdpClient(assistantToScreenPort);
            IPEndPoint target = new IPEndPoint(IPAddress.Parse(ipAddress), assistantToScreenPort);

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

    public void changeMainCamera(GameObject currentCamera)
    {
        string[] cameraName = currentCamera.name.Split(" ");
        int cameraNum = int.Parse(cameraName[1]);

        screenCamera = currentCamera.GetComponent<Camera>();

        screenCameraStartPos = screenCamera.transform.position;
        screenCameraStartRot = screenCamera.transform.rotation;

        sendInfo(assistantToScreenPort, "CHANGE_CAMERA: " + cameraNum);
    }

    public void sendResetPosRot()
    {
        SendPosRot();
        sendInfo(assistantToScreenPort, "RESET_POSROT");
    }

    // -- ASSISTANT TO DIRECTOR MESSAGES --
    public void sendPointPath(GameObject item, Vector3 pathPoint)
    {
        try
        {
            string message = "NEW_POINT:" + item.transform.name + ":";
            message += pathPoint.x.ToString(CultureInfo.InvariantCulture) + " " + pathPoint.y.ToString(CultureInfo.InvariantCulture) + " " + pathPoint.z.ToString(CultureInfo.InvariantCulture);
            sendInfo(assistantToDirectorPort, message);
        }
        catch (System.Exception e) { }
    }

    public void sendRotationPath(GameObject item, Quaternion pathRotation)
    {
        try
        {
            string message = "NEW_ROTATION:" + item.transform.name + ":";
            message += pathRotation.x.ToString(CultureInfo.InvariantCulture) + " " + pathRotation.y.ToString(CultureInfo.InvariantCulture) + " " + pathRotation.z.ToString(CultureInfo.InvariantCulture) + " " + pathRotation.w.ToString(CultureInfo.InvariantCulture);
            sendInfo(assistantToDirectorPort, message);
        }
        catch (System.Exception e) { }
    }

    public void sendItemMiddle(string name, string wrongName)
    {
        //this is needed because if starting it at the menu script, it will get disabled and the coroutine will break
        StartCoroutine(sendItem(name, wrongName));
    }

    public IEnumerator sendItem(string name, string wrongName)
    {
        // wait five seconds to ensure that object was already spawned at client side
        yield return new WaitForSeconds(1.0f);

        try
        {
            sendInfo(assistantToDirectorPort, "NEW_ITEM:" + name + ":" + wrongName);
        }
        catch (System.Exception e) { }
    }

    public void sendDeleteItemToDirector(string name)
    {
        sendInfo(assistantToDirectorPort, "DELETE_ITEM:" + name);
    }

    public void sendDeletePointToDirector(int pointNum, string itemName)
    {
        sendInfo(assistantToDirectorPort, "DELETE_POINT:" + pointNum + ":" + itemName);
    }

    public void sendChangeItemColor(string itemName, string colorHex)
    {
        Debug.Log("SENDING CHANGE COLOR ITEM: " + itemName + " " + colorHex);
        sendInfo(assistantToDirectorPort, "CHANGE_ITEM_COLOR:" + itemName + ":" + colorHex);
    }

    public void sendChangePathColor(string itemName, string colorHex)
    {
        Debug.Log("SENDING CHANGE COLOR PATH: " + itemName + " " + colorHex);
        sendInfo(assistantToDirectorPort, "CHANGE_PATH_COLOR:" + itemName + ":" + colorHex);
    }

    public void sendChangePointColor(string itemName, string pointName, string colorHex)
    {
        sendInfo(assistantToDirectorPort, "CHANGE_POINT_COLOR:" + itemName + ":" + pointName + ":" + colorHex);
    }

    public void sendShowHideGridDirector(bool isShowed)
    {
        sendInfo(assistantToDirectorPort, "SHOW_HIDE_GRID:" + isShowed);
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

        GameObject[] pathContainers = GameObject.FindGameObjectsWithTag("PathContainer");
        // get all lines in the scene and rotate all of their points
        foreach (GameObject path in pathContainers)
        {
            LineRenderer lineRenderer = path.GetComponentInChildren<LineRenderer>();
            // reasign lineRenderer points
            Vector3[] pathPoints = new Vector3[lineRenderer.positionCount];
            lineRenderer.GetPositions(pathPoints);
            for (int i = 0; i < pathPoints.Length; i++)
            {
                Vector3 point = pathPoints[i];
                pathPoints[i] = rotatePointAround(point, pivotPoint, rotationQuat);
            }

            lineRenderer.SetPositions(pathPoints);

            path.transform.RotateAround(pivotPoint, Vector3.up, rotationAngle);
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

        if (ModesManager.instance.role == ModesManager.eRoleType.DIRECTOR)
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

            if (ModesManager.instance.role == ModesManager.eRoleType.ASSISTANT)
                SendPosRot();

            positionChanged = true;
            lastPos = currentPos;
            posChangedCount = 0;
        }

        else if (positionChanged)
        {
            // needed to make sure that position is not changing,
            // since there are frames in between when where position does not change
            posChangedCount += 1;
            if (posChangedCount >= 10)
            {
                resetStart = false;
                positionChanged = false;
            }
        }

        if (OVRInput.Get(OVRInput.RawButton.Y))
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
    }
}
