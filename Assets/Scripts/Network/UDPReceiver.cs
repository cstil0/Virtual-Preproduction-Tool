using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Globalization;
using Unity.VisualScripting;
using System.Security.Cryptography;

public class UDPReceiver : MonoBehaviour
{
    // udpclient object
    //UdpClient client;
    UdpClient clientPath;
    UdpClient clientPlay;
    UdpClient clientRotation;
    // separate it in different ports since this is used both for assistant and director,
    // so that it is less messy for me
    [SerializeField] int serverPort = 8050;
    [SerializeField] int assistantToDirectorPort = 8051;
    [SerializeField] int directoToAssistantPort = 8052;
    [SerializeField] int rotateScenePort = 8053;

    [SerializeField] GameObject itemsParent;

    //Thread receiveThread;
    Thread assistantToDirectorThread;
    Thread directorToAssistantThread;
    Thread receiveSceneRotationThread;

    bool pointPositionParsed;
    bool pointRotationParsed;
    bool newItemParsed;
    bool playParsed;
    bool speedParsed;
    bool deletePointParsed;
    bool deleteItemParsed;
    bool sceneRotationParsed;

    String receivedMessage;
    String receivedPointName;
    String receivedSpeedName;
    float receivedSpeedValue;
    String receivedDeleteItemName;
    String receivedDeletePointName;
    int receivedDeletePointNum;
    String newReceivedName;
    String newWrongReceivedName;
    int receivedCount;
    string receivedPointPosition;
    string receivedPointRotation;
    string receivedPlay;
    string receivedSceneRotation;

    public Camera ScreenCamera;
    Vector3 startPos;
    Vector3 remoteStartPos;
    Vector3 currPos;
    Vector3 newPointPosition;

    //Vector3 startRot;
    Quaternion startRot;
    //Vector3 remoteStartRot;
    Quaternion remoteStartRot;
    //Vector3 currRot;
    Quaternion currRot;


    public GameObject hermione;
    public GameObject harry;

    enum assistantToDirectorMessages{
        NEW_ITEM,
        NEW_POINT,
        NEW_ROTATION
    }

    enum directorToAssistantMessages
    {
        CHANGE_SPEED,
        PLAY_PATH,
        DELETE_POINT,
        DELETE_ITEM
    }

    // main thread that listens to UDP messages through a defined port
    void UDP_ReceieveThread()
    {
        UdpClient client = new UdpClient(serverPort);
        // loop needed to keep listening
        while (true)
        {
            try
            {
                // recieve messages through the end point
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, serverPort);
                byte[] receiveBytes = client.Receive(ref remoteEndPoint);
                receivedMessage = Encoding.ASCII.GetString(receiveBytes);
                bool resetStart = Convert.ToBoolean(receivedMessage);

                receiveBytes = client.Receive(ref remoteEndPoint);
                // once the message is recieved, encode it as ASCII
                receivedMessage = Encoding.ASCII.GetString(receiveBytes);
                Debug.Log("Position: " + receivedMessage);

                //string[] splittedMessage = receivedMessage.Split(" ");
                string[] splittedMessage = receivedMessage.Split(", ");
                currPos = new Vector3(float.Parse(splittedMessage[0][1..], CultureInfo.InvariantCulture), float.Parse(splittedMessage[1], CultureInfo.InvariantCulture), float.Parse(splittedMessage[2][..^1], CultureInfo.InvariantCulture));

                if (resetStart)
                {
                    remoteStartPos = new Vector3(currPos.x, currPos.y, currPos.z);
                }

                receiveBytes = client.Receive(ref remoteEndPoint);
                // once the message is recieved, encode it as ASCII
                receivedMessage = Encoding.ASCII.GetString(receiveBytes);
                Debug.Log("Rotation: " + receivedMessage);

                //splittedMessage = receivedMessage.Split(" ");
                splittedMessage = receivedMessage.Split(", ");
                //currRot = new Vector3(float.Parse(splittedMessage[0], CultureInfo.InvariantCulture), float.Parse(splittedMessage[1], CultureInfo.InvariantCulture), float.Parse(splittedMessage[2], CultureInfo.InvariantCulture));
                currRot = new Quaternion(float.Parse(splittedMessage[0][1..], CultureInfo.InvariantCulture), float.Parse(splittedMessage[1], CultureInfo.InvariantCulture), float.Parse(splittedMessage[2], CultureInfo.InvariantCulture), float.Parse(splittedMessage[3][..^1], CultureInfo.InvariantCulture));

                //if (remoteStartRot == new Vector3(0.0f, 0.0f, 0.0f))
                //{
                //    remoteStartRot = new Vector3(currRot.x, currRot.y, currRot.z);
                //}
                if (resetStart)
                {
                    remoteStartRot = new Quaternion(currRot.x, currRot.y, currRot.z, currRot.w);
                }

            }
            catch (Exception e)
            {
                print("Exception thrown " + e.Message);
            }
        }
    }

    void UDP_assistantToDirectorReceive()
    {
        clientPath = new UdpClient(assistantToDirectorPort);
        // loop needed to keep listening
        while (true)
        {
            try
            {
                // recieve messages through the end point
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, assistantToDirectorPort);
                byte[] receivedBytes = clientPath.Receive(ref remoteEndPoint);

                string receivedString = Encoding.ASCII.GetString(receivedBytes);
                string[] splittedMessage = receivedString.Split(":");
                assistantToDirectorMessages message_enum = (assistantToDirectorMessages)Enum.Parse(typeof(assistantToDirectorMessages), splittedMessage[0]);
                string message = splittedMessage[1];

                switch (message_enum)
                {
                    case assistantToDirectorMessages.NEW_ITEM:
                        newReceivedName = splittedMessage[1];
                        newWrongReceivedName = splittedMessage[2];
                        newItemParsed = false;
                        break;
                    case assistantToDirectorMessages.NEW_POINT:
                        receivedPointName = message;
                        receivedBytes = clientPath.Receive(ref remoteEndPoint);
                        receivedPointPosition = Encoding.ASCII.GetString(receivedBytes);
                        pointPositionParsed = false;
                        break;
                    case assistantToDirectorMessages.NEW_ROTATION:
                        receivedPointRotation = Encoding.ASCII.GetString(receivedBytes);
                        pointRotationParsed = false;
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.Log("Exception thrown " + e.Message);
            }
        }
    }

    void UDP_directorToAssistantReceive()
    {
        clientPlay = new UdpClient(directoToAssistantPort);
        // loop needed to keep listening
        while (true)
        {
            try
            {
                // recieve messages through the end point
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, directoToAssistantPort);
                byte[] receiveBytes = clientPlay.Receive(ref remoteEndPoint);

                string receivedString = Encoding.ASCII.GetString(receiveBytes);
                string[] splittedMessage = receivedString.Split(":");
                directorToAssistantMessages message_enum = (directorToAssistantMessages)Enum.Parse(typeof(directorToAssistantMessages), splittedMessage[0]);

                switch (message_enum)
                {
                    case directorToAssistantMessages.PLAY_PATH:
                        receivedPlay = splittedMessage[1];
                        playParsed = false;
                        break;
                    case directorToAssistantMessages.CHANGE_SPEED:
                        receivedSpeedName = splittedMessage[1];
                        receivedSpeedValue = float.Parse(splittedMessage[2]);
                        speedParsed = false;
                        break;
                    case directorToAssistantMessages.DELETE_POINT:
                        receivedDeletePointName = splittedMessage[1];
                        receivedDeletePointNum = int.Parse(splittedMessage[2]);
                        deletePointParsed = false;
                        break;
                    case directorToAssistantMessages.DELETE_ITEM:
                        deleteItemParsed = false;
                        receivedDeleteItemName = splittedMessage[1];
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.Log("Exception thrown " + e.Message);
            }
        }
    }

    void UDP_RotateSceneReceive()
    {
        clientRotation = new UdpClient(rotateScenePort);
        // loop needed to keep listening
        while (true)
        {
            try
            {
                // recieve messages through the end point
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, rotateScenePort);
                byte[] receiveBytes = clientRotation.Receive(ref remoteEndPoint);
                receivedSceneRotation = Encoding.ASCII.GetString(receiveBytes);

                sceneRotationParsed = false;
            }
            catch (Exception e)
            {
                Debug.Log("Exception thrown " + e.Message);
            }
        }
    }

    void parsePointPosition()
    {
        pointPositionParsed = true;

        GameObject item = itemsParent.transform.Find(receivedPointName).gameObject;
        int itemNum = int.Parse(receivedPointName.Split(" ")[1]);

        Transform pathContainer;
        try
        {
            pathContainer = GameObject.Find("PathParent(Clone)").transform;
            pathContainer.name = "Path " + itemNum;
        }
        catch (Exception e)
        {
            pathContainer = GameObject.Find("Path " + itemNum).transform;
        }

        string[] splittedMessage = receivedPointPosition.Split(" ");
        float posX = float.Parse(splittedMessage[0], CultureInfo.InvariantCulture);
        float posY = -float.Parse(splittedMessage[1], CultureInfo.InvariantCulture);
        float posZ = float.Parse(splittedMessage[2], CultureInfo.InvariantCulture);
        Vector3 newPointPosition = new Vector3(posX, posY, posZ);

        item.TryGetComponent(out FollowPath followPath);
        item.TryGetComponent(out FollowPathCamera followPathCamera);
        if (followPath != null)
        {
            followPath.pathContainer = pathContainer.gameObject;
            StartCoroutine(followPath.defineNewPathPoint(newPointPosition, false));
            int pointsCount = followPath.pathPositions.Count;
            if (pointsCount == 1)
                ItemsDirectorPanelController.instance.addPointsLayout(receivedPointName);

            ItemsDirectorPanelController.instance.addNewPointButton(receivedPointName, pointsCount - 1);

            // add point to line renderer of the corresponding path
            Transform pointTransform = pathContainer.GetChild(pointsCount);
            // y is sended conditionated to the character height to make sure that it is always on the floor,
            // so real y is taken from the actual sphere point position
            float realPosY = pointTransform.position.y;
             Vector3 realNewPosition = new Vector3(posX, realPosY, posZ);
            GameObject line = pathContainer.GetChild(0).gameObject;
            LineRenderer lineRenderer = line.GetComponent<LineRenderer>();
            if (pointsCount > 1)
            {
                lineRenderer.positionCount += 1;
            }
            lineRenderer.SetPosition(pointsCount - 1, realNewPosition);
        }
    }

    void parsePointRotation()
    {
        pointRotationParsed = true;

        // handle exceptions in case the item is not found
        try
        {
            GameObject item = itemsParent.transform.Find(receivedPointName).gameObject;
            int itemNum = int.Parse(receivedPointName.Split(" ")[1]);

            Transform pathContainer;
            try
            {
                pathContainer = GameObject.Find("PathParent(Clone)").transform;
                pathContainer.name = "Path " + itemNum;
            }
            catch (Exception e)
            {
                pathContainer = GameObject.Find("Path " + itemNum).transform;
            }

            string[] splittedMessage = receivedPointRotation.Split(" ");
            float rotX = float.Parse(splittedMessage[1], CultureInfo.InvariantCulture);
            float rotY = -float.Parse(splittedMessage[2], CultureInfo.InvariantCulture);
            float rotZ = float.Parse(splittedMessage[3], CultureInfo.InvariantCulture);
            float rotW = float.Parse(splittedMessage[4], CultureInfo.InvariantCulture);
            Quaternion newRotation = new Quaternion(rotX, rotY, rotZ, rotW);

            item.TryGetComponent(out FollowPathCamera followPathCamera);
            if (followPathCamera != null)
            {
                if (followPathCamera != null)
                {
                    followPathCamera.pathContainer = pathContainer.gameObject;
                    StartCoroutine(followPathCamera.defineNewPathPoint(newPointPosition, newRotation, false));

                    int pointsCount = followPathCamera.pathPositions.Count;
                    if (pointsCount == 2)
                        ItemsDirectorPanelController.instance.addPointsLayout(receivedPointName);

                    ItemsDirectorPanelController.instance.addNewPointButton(receivedPointName, pointsCount - 2);

                    Transform pointTransform = pathContainer.GetChild(pointsCount);
                    GameObject line = pathContainer.GetChild(0).gameObject;
                    LineRenderer lineRenderer = line.GetComponent<LineRenderer>();
                    if (pointsCount <= 2)
                        lineRenderer.SetPosition(pointsCount - 1, pointTransform.position);
                    if (pointsCount > 2)
                    {
                        lineRenderer.positionCount += 1;
                        lineRenderer.SetPosition(pointsCount - 1, pointTransform.position);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("ERROR PARSING ROTATION: " + e.ToString());
        }
    }

    void parsePlayMessage()
    {
        playParsed = true;

        if (receivedPlay == "PLAY")
            DirectorPanelManager.instance.playPath();
        else if (receivedPlay == "STOP")
            DirectorPanelManager.instance.stopPath();
    }

    void parseSpeed()
    {
        speedParsed = true;

        GameObject item = itemsParent.transform.Find(receivedSpeedName).gameObject;
        item.TryGetComponent(out FollowPath followPath);
        item.TryGetComponent(out FollowPathCamera followPathCamera);

        if (followPath != null)
            followPath.changeSpeed(receivedSpeedValue);

        if (followPathCamera != null)
            followPathCamera.changeSpeed(receivedSpeedValue);
    }

    void parseDeletePoint()
    {
        deletePointParsed = true;

        GameObject item = itemsParent.transform.Find(receivedDeletePointName).gameObject;
        item.TryGetComponent(out FollowPath followPath);
        item.TryGetComponent(out FollowPathCamera followPathCamera);

        if (followPath != null)
            followPath.deletePathPoint(receivedDeletePointNum);

        if (followPathCamera != null)
            followPathCamera.deletePathPoint(receivedDeletePointNum);
    }

    void parseDeleteItem()
    {
        deleteItemParsed = true;

        if (!receivedDeleteItemName.Contains("Camera")){
            GameObject item = GameObject.Find(receivedDeleteItemName);
            Destroy(item);

            string[] splittedName = receivedDeleteItemName.Split(" ");
            string itemNum = splittedName[1];
            GameObject pathContainer = GameObject.Find("Path " + itemNum);
        }
    }

    void parseSceneRotationMessage()
    {
        sceneRotationParsed = true;

        float rotationAngle = float.Parse(receivedSceneRotation);
        UDPSender.instance.rotateItemsInScene(rotationAngle);
    }

    void OnDisable()
    {
        // stop thread when object is disabled
        //if (receiveThread != null){
        //    receiveThread.Abort();
              //client.Close();
        //}

        if (assistantToDirectorThread != null)
        {
            assistantToDirectorThread.Abort();
            clientPath.Close();
        }

        if (directorToAssistantThread != null)
        {
            directorToAssistantThread.Abort();
            clientPlay.Close();
        }

        if (receiveSceneRotationThread != null)
        {
            receiveSceneRotationThread.Abort();
            clientRotation.Close();
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        newItemParsed = true;
        pointPositionParsed = true;
        pointRotationParsed = true;
        playParsed = true;
        speedParsed = true;
        deletePointParsed = true;
        deleteItemParsed = true;
        sceneRotationParsed = true;

        // Start thread to listen UDP messages and set it as background
        //receiveThread = new Thread(UDP_ReceieveThread);
        //receiveThread.IsBackground = true;
        //receiveThread.Start();

        // director wants to receive new point paths created by the assistant
        if (ModesManager.instance.role == ModesManager.eRoleType.DIRECTOR)
        {
            assistantToDirectorThread = new Thread(UDP_assistantToDirectorReceive);
            assistantToDirectorThread.IsBackground = true;
            assistantToDirectorThread.Start();

            receiveSceneRotationThread = new Thread(UDP_RotateSceneReceive);
            receiveSceneRotationThread.IsBackground = true;
            receiveSceneRotationThread.Start();
        }

        // assistant wants to receive when director plays or stops the path play, since it controlls the position with network manager
        if (ModesManager.instance.role == ModesManager.eRoleType.ASSISTANT)
        {
            directorToAssistantThread = new Thread(UDP_directorToAssistantReceive);
            directorToAssistantThread.IsBackground = true;
            directorToAssistantThread.Start();
        }

        startPos = ScreenCamera.transform.position;
        //startRot = ScreenCamera.transform.rotation.eulerAngles;
        startRot = ScreenCamera.transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (receivedMessage != null)
        {
            Vector3 remotePosDiff = currPos - remoteStartPos;
            ScreenCamera.transform.position = remotePosDiff + startPos;

            //Vector3 remoteRotDiff = remoteStartRot - currRot;
            Quaternion remoteRotDiff = remoteStartRot * Quaternion.Inverse(currRot);
            //ScreenCamera.transform.rotation = Quaternion.Euler(remoteRotDiff + startRot);
            ScreenCamera.transform.rotation = remoteRotDiff * startRot;
        }

        if (!newItemParsed)
        {
            newItemParsed = true;
            // change item name
            itemsParent.transform.Find(newWrongReceivedName).name = newReceivedName;
            ItemsDirectorPanelController.instance.addNewItemButton(newReceivedName);
        }
        if (!pointPositionParsed)
            parsePointPosition();

        if (!pointRotationParsed)
            parsePointRotation();

        if (!playParsed)
            parsePlayMessage();

        if (!speedParsed)
            parseSpeed();

        if (!deletePointParsed)
            parseDeletePoint();

        if (!deleteItemParsed)
            parseDeleteItem();

        if (!sceneRotationParsed)
            parseSceneRotationMessage();
    }
}