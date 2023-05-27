using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Globalization;
using Unity.VisualScripting;
using UnityEngine.UI;
using System.Collections;

public class UDPReceiver : MonoBehaviour
{
    public static UDPReceiver instance = null;

    // udpclient object
    //UdpClient client;
    UdpClient clientPath;
    UdpClient clientPlay;
    UdpClient clientRotation;
    // separate it in different ports since this is used both for assistant and director,
    // so that it is less messy for me
    [SerializeField] int assistantToDirectorPort = 8051;
    [SerializeField] int directoToAssistantPort = 8052;

    [SerializeField] GameObject itemsParent;

    //Thread receiveThread;
    Thread assistantToDirectorThread;
    Thread directorToAssistantThread;

    private string lastMessageReceived;
    private bool isMessageParsed = true;

    public Camera ScreenCamera;
    Vector3 startPos;
    Vector3 remoteStartPos;
    Vector3 currPos;
    Vector3 newCameraPosition;

    //Vector3 startRot;
    Quaternion startRot;
    //Vector3 remoteStartRot;
    Quaternion remoteStartRot;
    //Vector3 currRot;
    Quaternion currRot;

    public delegate void ChangeItemColor(string itemName, Color color);
    public event ChangeItemColor OnChangeItemColor;
    public delegate void ChangePathColor(string itemName, Color color);
    public event ChangePathColor OnChangePathColor;
    public delegate void ChangePointColor(string itemName, string pointName, Color color);
    public event ChangePointColor OnChangePointColor;

    enum eAssistantToDirectorMessages{
        NEW_ITEM,
        NEW_POINT,
        NEW_ROTATION,
        DELETE_ITEM,
        SCENE_ROTATION,
        SHOW_HIDE_GRID,
        DELETE_POINT,
        CHANGE_ITEM_COLOR,
        CHANGE_PATH_COLOR,
        CHANGE_POINT_COLOR
    }

    enum eDirectorToAssistantMessages
    {
        CHANGE_SPEED,
        PLAY_PATH,
        DELETE_POINT,
        DELETE_ITEM,
        CHANGE_CAMERA,
        CHANGE_SCREEN_DISTANCE,
        SHOW_HIDE_GRID,
        CHANGE_LIGHT_COLOR,
        CHANGE_LIGHT_INTENSITY
    }

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

                lastMessageReceived = Encoding.ASCII.GetString(receivedBytes);
                isMessageParsed = false;
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

                lastMessageReceived = Encoding.ASCII.GetString(receiveBytes);
                isMessageParsed = false;
            }
            catch (Exception e)
            {
                Debug.Log("Exception thrown " + e.Message);
            }
        }
    }

    //void UDP_RotateSceneReceive()
    //{
    //    clientRotation = new UdpClient(rotateScenePort);
    //    // loop needed to keep listening
    //    while (true)
    //    {
    //        try
    //        {
    //            // recieve messages through the end point
    //            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, rotateScenePort);
    //            byte[] receiveBytes = clientRotation.Receive(ref remoteEndPoint);
    //            receivedSceneRotation = Encoding.ASCII.GetString(receiveBytes);
    //        }
    //        catch (Exception e)
    //        {
    //            Debug.Log("Exception thrown " + e.Message);
    //        }
    //    }
    //}

    void parsePointPosition(string itemName, string pointPosition)
    {
        GameObject item = itemsParent.transform.Find(itemName).gameObject;
        int itemNum = int.Parse(itemName.Split(" ")[1]);
        item.TryGetComponent(out FollowPath followPath);
        item.TryGetComponent(out FollowPathCamera followPathCamera);

        Transform pathContainer;
        Transform circlesContainer;

        string[] splittedMessage = pointPosition.Split(" ");
        float posX = float.Parse(splittedMessage[0], CultureInfo.InvariantCulture);
        float posY = -float.Parse(splittedMessage[1], CultureInfo.InvariantCulture);
        float posZ = float.Parse(splittedMessage[2], CultureInfo.InvariantCulture);
        Vector3 newPointPosition = new Vector3(posX, posY, posZ);


        if (followPath != null)
        {
            // change name only for the first time a point from this path is received
            try
            {
                pathContainer = GameObject.Find("PathParent(Clone)").transform;
                circlesContainer = GameObject.Find("CirclesParent(Clone)").transform;
                pathContainer.name = "Path " + itemNum;
                circlesContainer.name = "Circles " + itemNum;
            }
            catch (Exception e) {
                pathContainer = GameObject.Find("Path " + itemNum).transform;
            }

            // find the corresponding path sphere to change its name and assign the corresponding item
            GameObject pathSphere = pathContainer.GetChild(pathContainer.childCount - 1).gameObject;
            pathSphere.name = "Point " + (pathContainer.childCount - 2);
            PathSpheresController spheresController = pathSphere.GetComponent<PathSpheresController>();
            spheresController.item = item;

            followPath.pathContainer = pathContainer.gameObject;

            StartCoroutine(followPath.defineNewPathPoint(newPointPosition, false));
            int pointsCount = followPath.pathPositions.Count;
            if (pointsCount == 1)
                ItemsDirectorPanelController.instance.addPointsLayout(itemName);

            ItemsDirectorPanelController.instance.addNewPointButton(itemName, pointsCount - 1);

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

        if (followPathCamera != null)
        {
            newCameraPosition = newPointPosition;
        }
    }

    void parsePointRotation(string itemName, string pointRotation)
    {
        // handle exceptions in case the item is not found
        try
        {
            GameObject item = itemsParent.transform.Find(itemName).gameObject;
            int itemNum = int.Parse(itemName.Split(" ")[1]);

            Transform pathContainer;
            // change name only for the first time a point from this path is received
            try
            {
                pathContainer = GameObject.Find("PathParent(Clone)").transform;
                pathContainer.name = "Path " + itemName;
            }
            catch(Exception e)
            {
                pathContainer = GameObject.Find("Path " + itemName).transform;
            }

            string[] splittedMessage = pointRotation.Split(" ");
            float rotX = float.Parse(splittedMessage[0], CultureInfo.InvariantCulture);
            float rotY = -float.Parse(splittedMessage[1], CultureInfo.InvariantCulture);
            float rotZ = float.Parse(splittedMessage[2], CultureInfo.InvariantCulture);
            float rotW = float.Parse(splittedMessage[3], CultureInfo.InvariantCulture);
            Quaternion newRotation = new Quaternion(rotX, rotY, rotZ, rotW);

            item.TryGetComponent(out FollowPathCamera followPathCamera);
            if (followPathCamera != null)
            {

                followPathCamera.pathContainer = pathContainer.gameObject;
                StartCoroutine(followPathCamera.defineNewPathPoint(newCameraPosition, newRotation, false));
                int pointsCount = followPathCamera.pathPositions.Count;

                // find the corresponding path sphere to change its name and assign the corresponding item
                GameObject pathSphere = pathContainer.GetChild(pathContainer.childCount - 1).gameObject;
                pathSphere.name = "Point " + (pathContainer.childCount - 2);
                PathSpheresController spheresController = pathSphere.GetComponentInChildren<PathSpheresController>();
                CameraRotationController cameraRotationController = pathSphere.GetComponentInChildren<CameraRotationController>();
                spheresController.item = item;
                spheresController.followPathCamera = followPathCamera;
                cameraRotationController.followPathCamera = followPathCamera;
                cameraRotationController.pointNum = pointsCount -2;

                if (pointsCount == 2)
                    ItemsDirectorPanelController.instance.addPointsLayout(itemName);

                ItemsDirectorPanelController.instance.addNewPointButton(itemName, pointsCount - 2);

                Transform pointTransform = pathContainer.GetChild(pointsCount - 1);
                GameObject line = pathContainer.GetChild(0).gameObject;
                LineRenderer lineRenderer = line.GetComponent<LineRenderer>();
                if (pointsCount == 2)
                {
                    lineRenderer.SetPosition(pointsCount - 2, pointTransform.position);
                    lineRenderer.positionCount += 1;
                    lineRenderer.SetPosition(pointsCount - 1, pointTransform.position);
                }
                if (pointsCount > 2)
                {
                    lineRenderer.positionCount += 1;
                    lineRenderer.SetPosition(pointsCount - 1, pointTransform.position);
                }

                // set camera event from minicamera
                Camera miniCameraComponent = pointTransform.GetChild(1).GetComponent<Camera>();
                Canvas minicameraCanvas = pointTransform.GetChild(2).GetComponent<Canvas>();
                RawImage minicameraImage = pointTransform.GetChild(2).GetComponentInChildren<RawImage>();
                minicameraCanvas.worldCamera = miniCameraComponent;

                RenderTexture miniCameraTexture = new RenderTexture(426, 240, 16, RenderTextureFormat.ARGB32);
                minicameraImage.texture = miniCameraTexture;
                miniCameraComponent.targetTexture = miniCameraTexture;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("ERROR PARSING ROTATION: " + e.ToString());
        }
    }

    void parseDeleteItemDirector(string itemName)
    {
        ItemsDirectorPanelController.instance.removeItemButtons(itemName);
    }

    void parseDeletePointDirector(int pointNum, string itemName)
    {
        GameObject item = itemsParent.transform.Find(itemName).gameObject;

        ItemsDirectorPanelController.instance.deletePointButton(item, itemName, pointNum);
    }

    void parseItemChangeColor(string itemName, string colorHex)
    {
        colorHex = "#" + colorHex;
        UnityEngine.ColorUtility.TryParseHtmlString(colorHex, out Color color);
        OnChangeItemColor(itemName, color);
        Debug.Log("ITEM COLOR: " + itemName + " " + color.ToHexString());
    }

    void parsePathChangeColor(string itemName, string colorHex)
    {
        colorHex = "#" + colorHex;

        UnityEngine.ColorUtility.TryParseHtmlString(colorHex, out Color color);
        OnChangePathColor(itemName, color);
        Debug.Log("PATH COLOR: " + itemName + " " + color.ToHexString());
    }

    IEnumerator parsePointChangeColor(string itemName, string pointName, string colorHex)
    {
        // wait to ensure that the point is created yet
        yield return new WaitForSeconds(0.2f);

        colorHex = "#" + colorHex;

        UnityEngine.ColorUtility.TryParseHtmlString(colorHex, out Color color);
        OnChangePointColor(itemName, pointName, color);
        Debug.Log("POINT COLOR: " + itemName + " " + color.ToHexString() + ", POINT: " + pointName);
    }

    void parsePlayMessage(string playStop)
    {
        if (playStop == "PLAY")
            DirectorPanelManager.instance.playPath();
        else if (playStop == "STOP")
            DirectorPanelManager.instance.stopPath();
    }

    void parseSpeed(string itemName, float speed)
    {
        GameObject item = itemsParent.transform.Find(itemName).gameObject;
        item.TryGetComponent(out FollowPath followPath);
        item.TryGetComponent(out FollowPathCamera followPathCamera);

        if (followPath != null)
            followPath.changeSpeed(speed);

        if (followPathCamera != null)
            followPathCamera.changeSpeed(speed);
    }

    void parseDeletePoint(string itemName, int pointNum)
    {
        GameObject item = itemsParent.transform.Find(itemName).gameObject;
        item.TryGetComponent(out FollowPath followPath);
        item.TryGetComponent(out FollowPathCamera followPathCamera);

        if (followPath != null)
            followPath.deletePathPoint(pointNum);

        if (followPathCamera != null)
            followPathCamera.deletePathPoint(pointNum);
    }

    void parseDeleteItem(string itemName)
    {
        if (!itemName.Contains("Camera")){
            GameObject item = GameObject.Find(itemName);
            Destroy(item);

            string[] splittedName = itemName.Split(" ");
            string itemNum = splittedName[1];
            GameObject pathContainer = GameObject.Find("Path " + itemNum);
        }
    }

    void parseSceneRotationMessage(string sceneRotation)
    {
        float rotationAngle = float.Parse(sceneRotation);
        UDPSender.instance.rotateItemsInScene(rotationAngle);
    }

    void parseChangeCamera(string changeCameraMessage)
    {
        UDPSender.instance.sendChangeCameraScreen(changeCameraMessage);
    }

    void parseChangeScreenDistance(string changeDistanceMessage)
    {
        UDPSender.instance.sendChangeScreenDistanceAssistant(changeDistanceMessage);
    }

    void parseShowHideGrid(bool isShowed)
    {
        DirectorPanelManager.instance.grid.SetActive(isShowed);
    }

    void parseChangeLightColor(string focusName, string colorHex, bool isAccepted)
    {
        GameObject focus = itemsParent.transform.Find(focusName).gameObject;
        LightController lightController = focus.GetComponent<LightController>();

        colorHex = "#" + colorHex;
        UnityEngine.ColorUtility.TryParseHtmlString(colorHex, out Color color);
        lightController.changeLightColor(color, isAccepted);
    }

    void parseChangeLightIntensity(string focusName, float intensity)
    {
        GameObject focus = itemsParent.transform.Find(focusName).gameObject;
        LightController lightController = focus.GetComponent<LightController>();

        lightController.changeLightIntensity(intensity);
    }

    void OnDisable()
    {
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
    }
    // Start is called before the first frame update
    void Start()
    {
        // director wants to receive new point paths created by the assistant
        if (ModesManager.instance.role == ModesManager.eRoleType.DIRECTOR)
        {
            assistantToDirectorThread = new Thread(UDP_assistantToDirectorReceive);
            assistantToDirectorThread.IsBackground = true;
            assistantToDirectorThread.Start();
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
        if (!isMessageParsed)
        {
            string[] splittedMessage = lastMessageReceived.Split(":");
            
            switch (ModesManager.instance.role)
            {
                case ModesManager.eRoleType.DIRECTOR:
                    eAssistantToDirectorMessages message_enumATD = (eAssistantToDirectorMessages)Enum.Parse(typeof(eAssistantToDirectorMessages), splittedMessage[0]);
                    switch (message_enumATD)
                    {
                        case eAssistantToDirectorMessages.NEW_ITEM:
                            string newReceivedName = splittedMessage[1];
                            string newWrongReceivedName = splittedMessage[2];

                            itemsParent.transform.Find(newWrongReceivedName).name = newReceivedName;
                            ItemsDirectorPanelController.instance.addNewItemButton(newReceivedName);
                            break;
                        case eAssistantToDirectorMessages.NEW_POINT:
                            string receivedPointName = splittedMessage[1];
                            string receivedPointPosition = splittedMessage[2];

                            parsePointPosition(receivedPointName, receivedPointPosition);
                            Debug.Log("RECEIVED POSITION");
                            break;
                        case eAssistantToDirectorMessages.NEW_ROTATION:
                            receivedPointName = splittedMessage[1];
                            string receivedPointRotation = splittedMessage[2];

                            parsePointRotation(receivedPointName, receivedPointRotation);
                            Debug.Log("RECEIVED ROTATION");
                            break;
                        case eAssistantToDirectorMessages.DELETE_ITEM:
                            string receivedDeleteItemDirector = splittedMessage[1];
                            parseDeleteItemDirector(receivedDeleteItemDirector);
                            break;
                        case eAssistantToDirectorMessages.SCENE_ROTATION:
                            string receivedSceneRotation = splittedMessage[1];
                            parseSceneRotationMessage(receivedSceneRotation);
                            break;
                        case eAssistantToDirectorMessages.SHOW_HIDE_GRID:
                            bool isShowed = bool.Parse(splittedMessage[1]);
                            parseShowHideGrid(isShowed);
                            break;
                        case eAssistantToDirectorMessages.DELETE_POINT:
                            int pointNum = int.Parse(splittedMessage[1]);
                            string itemName = splittedMessage[2];
                            parseDeletePointDirector(pointNum, itemName);
                            break;
                        case eAssistantToDirectorMessages.CHANGE_ITEM_COLOR:
                            itemName = splittedMessage[1];
                            string colorHex = splittedMessage[2];
                            parseItemChangeColor(itemName, colorHex);
                            break;
                        case eAssistantToDirectorMessages.CHANGE_PATH_COLOR:
                            itemName = splittedMessage[1];
                            colorHex = splittedMessage[2];
                            parsePathChangeColor(itemName, colorHex);
                            break;
                        case eAssistantToDirectorMessages.CHANGE_POINT_COLOR:
                            itemName = splittedMessage[1];
                            string pointName = splittedMessage[2];
                            colorHex = splittedMessage[3];
                            StartCoroutine(parsePointChangeColor(itemName, pointName, colorHex));
                            break;
                    }
                    break;
                case ModesManager.eRoleType.ASSISTANT:
                    eDirectorToAssistantMessages message_enumDTA = (eDirectorToAssistantMessages)Enum.Parse(typeof(eDirectorToAssistantMessages), splittedMessage[0]);
                    switch (message_enumDTA)
                    {
                        case eDirectorToAssistantMessages.PLAY_PATH:
                            string receivedPlay = splittedMessage[1];
                            parsePlayMessage(receivedPlay);
                            break;
                        case eDirectorToAssistantMessages.CHANGE_SPEED:
                            string receivedSpeedName = splittedMessage[1];
                            float receivedSpeedValue = float.Parse(splittedMessage[2]);
                            parseSpeed(receivedSpeedName, receivedSpeedValue);
                            break;
                        case eDirectorToAssistantMessages.DELETE_POINT:
                            string receivedDeletePointName = splittedMessage[1];
                            int receivedDeletePointNum = int.Parse(splittedMessage[2]);
                            parseDeletePoint(receivedDeletePointName, receivedDeletePointNum);
                            break;
                        case eDirectorToAssistantMessages.DELETE_ITEM:
                            string receivedDeleteItemName = splittedMessage[1];
                            parseDeleteItem(receivedDeleteItemName);
                            break;
                        case eDirectorToAssistantMessages.CHANGE_CAMERA:
                            string receivedChangeCamera = splittedMessage[1];
                            parseChangeCamera(receivedChangeCamera);
                            break;
                        case eDirectorToAssistantMessages.CHANGE_SCREEN_DISTANCE:
                            parseChangeScreenDistance(splittedMessage[1]);
                            break;
                        case eDirectorToAssistantMessages.SHOW_HIDE_GRID:
                            bool isShowed = bool.Parse(splittedMessage[1]);
                            parseShowHideGrid(isShowed);
                            break;
                        case eDirectorToAssistantMessages.CHANGE_LIGHT_COLOR:
                            string focusName = splittedMessage[1];
                            string colorHex = splittedMessage[2];
                            bool isAccepted = bool.Parse(splittedMessage[3]);
                            parseChangeLightColor(focusName, colorHex, isAccepted);
                            break;
                        case eDirectorToAssistantMessages.CHANGE_LIGHT_INTENSITY:
                            focusName = splittedMessage[1];
                            float intensity = float.Parse(splittedMessage[2]);
                            parseChangeLightIntensity(focusName, intensity);
                            break;
                    }
                    break;
            }
            isMessageParsed = true;
        }
    }
}