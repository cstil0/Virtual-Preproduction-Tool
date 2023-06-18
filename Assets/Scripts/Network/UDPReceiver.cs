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
using System.Linq;
using System.Collections.Generic;
using UnityEditor.Rendering;

// this scripts listens to UDP messages and trigger the corresponding actions when one is received
public class UDPReceiver : MonoBehaviour
{
    public static UDPReceiver instance = null;

    UdpClient clientPath;
    UdpClient clientPlay;
    UdpClient clientRotation;
    // separate it in different ports since this is used both for assistant and director,
    // so that it is less messy for me
    [SerializeField] int assistantToDirectorPort = 8051;
    [SerializeField] int directorToAssistantPort = 8052;

    [SerializeField] GameObject itemsParent;

    Thread assistantToDirectorThread;
    Thread directorToAssistantThread;

    // queue is needed to ensure that all received messages are parsed
    private Queue<string> lastMessages = new Queue<string>();

    public Camera ScreenCamera;
    Vector3 startPos;
    Vector3 remoteStartPos;
    Vector3 currPos;
    Vector3 newCameraPosition;

    Quaternion startRot;
    Quaternion remoteStartRot;
    Quaternion currRot;

    [SerializeField] GameObject itemsMenu;

    public delegate void ChangeItemColor(string itemName, Color color);
    public event ChangeItemColor OnChangeItemColor;
    public delegate void ChangePathColor(string itemName, Color color);
    public event ChangePathColor OnChangePathColor;
    public delegate void ChangePointColor(string itemName, string pointName, Color color);
    public event ChangePointColor OnChangePointColor;
    public delegate void ChangeMiniCameraColor(string cameraName, string pointName, Color color);
    public event ChangeMiniCameraColor OnChangeMiniCameraColor;

    // -- Messages definition --
    enum eAssistantToDirectorMessages
    {
        NEW_ITEM,
        NEW_POINT,
        NEW_ROTATION,
        DELETE_ITEM,
        SCENE_ROTATION,
        SHOW_HIDE_GRID,
        DELETE_POINT,
        CHANGE_ITEM_COLOR,
        CHANGE_PATH_COLOR,
        CHANGE_POINT_COLOR,
        CHANGE_MINICAMERA_COLOR,
        RELOCATE_POINT,
        RELOCATE_CAMERA_POINTS,
        ITEMS_MENU_NAVIGATION
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

    enum eItemsMenuActions
    {
        SHOW_HIDE_MENU,
        CATEGORY,
        BACK,
        NEXT_BUTTON,
        PREVIOUS_BUTTON
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

    // thread to listen to VR user messages
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

                lastMessages.Enqueue(Encoding.ASCII.GetString(receivedBytes));
            }
            catch (Exception e)
            {
                Debug.Log("Exception thrown " + e.Message);
            }
        }
    }

    // thread to listen to multi-camera user messages
    void UDP_directorToAssistantReceive()
    {
        clientPlay = new UdpClient(directorToAssistantPort);
        // loop needed to keep listening
        while (true)
        {
            try
            {
                // recieve messages through the end point
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, directorToAssistantPort);
                byte[] receiveBytes = clientPlay.Receive(ref remoteEndPoint);

                lastMessages.Enqueue(Encoding.ASCII.GetString(receiveBytes));
            }
            catch (Exception e)
            {
                Debug.Log("Exception thrown " + e.Message);
            }
        }
    }

    // executed when a new point is created in the scene
    IEnumerator parsePointPosition(string itemName, string pointPosition)
    {
        Transform itemTransform = itemsParent.transform.Find(itemName);
        while (itemTransform == null)
        {
            itemTransform = itemsParent.transform.Find(itemName);
            yield return null;
        }
        yield return 0;

        // get corresponding follow path script and containers
        GameObject item = itemTransform.gameObject;
        int itemNum = int.Parse(itemName.Split(" ")[1]);
        item.TryGetComponent(out FollowPath followPath);
        item.TryGetComponent(out FollowPathCamera followPathCamera);

        Transform pathContainer = null;
        Transform circlesContainer = null;

        string[] splittedMessage = pointPosition.Split(" ");
        float posX = float.Parse(splittedMessage[0], CultureInfo.InvariantCulture);
        float posY = float.Parse(splittedMessage[1], CultureInfo.InvariantCulture);
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
            // if it is not the first received point, it will have the correct name already
            catch (Exception e) {
                pathContainer = GameObject.Find("Path " + itemNum).transform;
                circlesContainer = GameObject.Find("Circles " + itemNum).transform;
            }

            // find the corresponding path sphere to change its name and assign the corresponding item
            GameObject pathSphere = pathContainer.GetChild(pathContainer.childCount - 1).gameObject;
            GameObject circle = circlesContainer.GetChild(circlesContainer.childCount - 1).gameObject;

            // rename and store needed references
            pathSphere.name = "Point " + (pathContainer.childCount - 2);
            circle.name = "Circle " + (circlesContainer.childCount - 1);
            PathCirclesController circlesController = circle.GetComponent<PathCirclesController>();
            circlesController.pointNum = circlesContainer.childCount - 1;
            circlesController.pathNum = itemNum;

            PathSpheresController pathSpheresController = pathSphere.GetComponent<PathSpheresController>();
            pathSpheresController.item = item;
            pathSpheresController.pointNum = pathContainer.childCount - 2;
            pathSpheresController.pathNum = itemNum;
            pathSpheresController.getFollowPath();

            followPath.pathContainer = pathContainer.gameObject;
            followPath.circlesContainer = circlesContainer.gameObject;

            StartCoroutine(followPath.defineNewPathPoint(newPointPosition, false));
            int pointsCount = followPath.pathPositions.Count;

            // create a new points layout if it is the first point of the character's path
            if (pointsCount == 1)
                ItemsDirectorPanelController.instance.addPointsLayout(itemName);

            // crate new point button
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

        // if camera, store its position to parse it along with the rotation
        if (followPathCamera != null)
            newCameraPosition = newPointPosition;
    }

    // executed when a new camera position and rotation is created
    void parsePointRotation(string itemName, string pointRotation)
    {
        Transform transformItem = itemsParent.transform.Find(itemName);
        if (transformItem == null)
            return;

        GameObject item = transformItem.gameObject;
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

        // parse each axis rotation
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
            spheresController.pointNum = pointsCount - 2;
            spheresController.pathNum = itemNum;
            spheresController.followPathCamera = followPathCamera;
            cameraRotationController.followPathCamera = followPathCamera;
            cameraRotationController.pointNum = pointsCount -2;

            // create new points layout if it is the first point of the camera path
            if (pointsCount == 2)
                ItemsDirectorPanelController.instance.addPointsLayout(itemName);

            // instantiate a new point button
            ItemsDirectorPanelController.instance.addNewPointButton(itemName, pointsCount - 2);

            Transform pointTransform = pathContainer.GetChild(pointsCount - 1);

            // set camera event from minicamera
            Camera miniCameraComponent = pointTransform.GetChild(1).GetComponent<Camera>();
            Canvas minicameraCanvas = pointTransform.GetChild(2).GetComponent<Canvas>();
            RawImage minicameraImage = pointTransform.GetChild(2).GetComponentInChildren<RawImage>();
            minicameraCanvas.worldCamera = miniCameraComponent;

            // assign corresponding view texure 
            RenderTexture miniCameraTexture = new RenderTexture(426, 240, 16, RenderTextureFormat.ARGB32);
            minicameraImage.texture = miniCameraTexture;
            miniCameraComponent.targetTexture = miniCameraTexture;
        }
    }

    // executed when an item is deleted in VR side
    void parseDeleteItemDirector(string itemName)
    {
        ItemsDirectorPanelController.instance.removeItemButtons(itemName);

        GameObject line = GameObject.FindGameObjectWithTag("Line");
        Destroy(line);
    }

    // executed when a point is deleted in VR side
    void parseDeletePointDirector(int pointNum, string itemName)
    {
        GameObject item = itemsParent.transform.Find(itemName).gameObject;

        ItemsDirectorPanelController.instance.deletePointButton(item, itemName, pointNum);
    }

    void parseItemChangeColor(string itemName, string colorHex)
    {
        colorHex = "#" + colorHex;
        UnityEngine.ColorUtility.TryParseHtmlString(colorHex, out Color color);
        // call event to change the corresponding item's color
        OnChangeItemColor(itemName, color);
    }

    void parsePathChangeColor(string itemName, string colorHex)
    {
        colorHex = "#" + colorHex;

        UnityEngine.ColorUtility.TryParseHtmlString(colorHex, out Color color);
        OnChangePathColor(itemName, color);
    }

    IEnumerator parsePointChangeColor(string itemName, string pointName, string colorHex)
    {
        // wait to ensure that the point is created yet
        yield return new WaitForSeconds(0.2f);

        colorHex = "#" + colorHex;

        UnityEngine.ColorUtility.TryParseHtmlString(colorHex, out Color color);
        OnChangePointColor(itemName, pointName, color);
    }

    IEnumerator parseMiniCameraChangeColor(string itemName, string pointName, string colorHex)
    {
        // wait to ensure that the point is created yet
        yield return new WaitForSeconds(0.2f);

        colorHex = "#" + colorHex;

        UnityEngine.ColorUtility.TryParseHtmlString(colorHex, out Color color);
        OnChangeMiniCameraColor(itemName, pointName, color);
    }

    void parsePlayMessage(string playStop)
    {
        if (playStop == "PLAY")
            DirectorPanelManager.instance.playPath();
        else if (playStop == "STOP")
            DirectorPanelManager.instance.stopPath();
    }

    // executed when new speed is received
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
            followPath.deletePathPoint(pointNum, false, true);

        if (followPathCamera != null)
            followPathCamera.deletePathPoint(pointNum, false, true);
    }

    void parseDeleteItem(string itemName)
    {
        if (!itemName.Contains("Camera")){
            GameObject item = itemsParent.transform.Find(itemName).gameObject;
            DefinePath.instance.deleteItem(item, false);
        }
    }

    void parseSceneRotationMessage(string sceneRotation)
    {
        float rotationAngle = float.Parse(sceneRotation);
        UDPSender.instance.rotateItemsInScene(rotationAngle);
    }

    // executed when new main camera is selected to inform the screen project
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
        DirectorPanelManager.instance.showHideGrid(false);
    }

    // executed when a path point is relocated
    void parsePointRelocation(string itemName, int pointNum, string direction, string directionInv)
    {
        // parse relocation direction
        string[] directionSplitted = direction.Split(" ");
        float dirX = float.Parse(directionSplitted[0], CultureInfo.InvariantCulture);
        float dirY = float.Parse(directionSplitted[1], CultureInfo.InvariantCulture);
        float dirZ = float.Parse(directionSplitted[2], CultureInfo.InvariantCulture);
        Vector3 directionVec = new Vector3(dirX, dirY, dirZ);

        // inverse direction is needed because of the way that cinemachine points are stored
        Vector3 directionInvVec = new Vector3();
        // first check if it was set, since characters do not need it
        if (directionInv != "")
        {
            string[] directionInvSplitted = direction.Split(" ");
            float dirInvX = float.Parse(directionInvSplitted[0], CultureInfo.InvariantCulture);
            float dirInvY = float.Parse(directionInvSplitted[1], CultureInfo.InvariantCulture);
            float dirInvZ = float.Parse(directionInvSplitted[2], CultureInfo.InvariantCulture);
            directionInvVec = new Vector3(dirInvX, dirInvY, dirInvZ);
        }

        // find the corresponding follow path script and relocate the point
        GameObject item = itemsParent.transform.Find(itemName).gameObject;
        item.TryGetComponent(out FollowPath followPath);
        item.TryGetComponent(out FollowPathCamera followPathCamera);

        if (followPath != null)
            followPath.relocatePoint(pointNum, directionVec, false);

        if (followPathCamera != null)
            followPathCamera.relocatePoint(pointNum, directionVec, false, directionInvVec);
    }

    // executed when a camera is relocated, to relocate all of its points and mantain their position, since otherwise they follow the camera's new position
    void parseCameraPointsRelocation(string cameraName, string startPosition)
    {
        GameObject item = itemsParent.transform.Find(cameraName).gameObject;

        // parse start position of the camera
        string[] startPositionSplitted = startPosition.Split(" ");
        float startPosX = float.Parse(startPositionSplitted[0], CultureInfo.InvariantCulture);
        float startPosY = float.Parse(startPositionSplitted[1], CultureInfo.InvariantCulture);
        float startPosZ = float.Parse(startPositionSplitted[2], CultureInfo.InvariantCulture);
        Vector3 startPositionVec = new Vector3(startPosX, startPosY, startPosZ);

        // find the corresponding follow path script and relocate all of its points in cinemachine component and linerenderer
        item.TryGetComponent(out FollowPathCamera followPathCamera);

        followPathCamera.startPosition = startPositionVec;

        followPathCamera.relocateCinemachinePoints(followPathCamera.cinemachineSmoothPath, startPositionVec);
        LineRenderer lineRenderer = followPathCamera.pathContainer.GetComponentInChildren<LineRenderer>();
        followPathCamera.relocateAllBezierPointsLineRenderer(lineRenderer, followPathCamera.cinemachineSmoothPath);
    }

    // executed when a new state or action is performed in the items menu, to replicate it in the client side
    void parseItemsMenuNavigation(string action, string button)
    {
        eItemsMenuActions menuActionEnum = (eItemsMenuActions)Enum.Parse(typeof(eItemsMenuActions), action);

        switch (menuActionEnum)
        {
            case eItemsMenuActions.SHOW_HIDE_MENU:
                bool show = bool.Parse(button);
                itemsMenu.GetComponent<ActivateDisableMenu>().showHideMenu(show);
                break;
            case eItemsMenuActions.CATEGORY:
                Transform categoryButtonsContainer = itemsMenu.transform.GetChild(0).GetChild(0);
                GameObject categoryButton = categoryButtonsContainer.Find(button).gameObject;
                ItemsMenuController itemsMenuController = categoryButton.GetComponent<ItemsMenuController>();
                itemsMenuController.ChangeMenu();
                break;
            case eItemsMenuActions.BACK: 
                Transform categoriesContainer = itemsMenu.transform.GetChild(0);
                Transform itemButtonsContainer = categoriesContainer.Find(button);
                GameObject backButton = itemButtonsContainer.Find("Back Button").gameObject;
                itemsMenuController = backButton.GetComponent<ItemsMenuController>();
                itemsMenuController.ChangeMenu();
                break;
            case eItemsMenuActions.NEXT_BUTTON:
                categoriesContainer = itemsMenu.transform.GetChild(0);
                itemButtonsContainer = categoriesContainer.Find(button);
                GameObject nextButton = itemButtonsContainer.Find("Next Button").gameObject;
                SubmenusNavigate submenusNavigate = nextButton.GetComponent<SubmenusNavigate>();
                submenusNavigate.onNextButtonPressed();
                break;
            case eItemsMenuActions.PREVIOUS_BUTTON:
                categoriesContainer = itemsMenu.transform.GetChild(0);
                itemButtonsContainer = categoriesContainer.Find(button);
                GameObject previousButton = itemButtonsContainer.Find("Previous Button").gameObject;
                submenusNavigate = previousButton.GetComponent<SubmenusNavigate>();
                submenusNavigate.onPreviousButtonPressed();
                break;
        }
    }

    // executed when light color changes
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

    void Start()
    {
        // listen to assistant messages if director role is set
        if (ModesManager.instance.role == ModesManager.eRoleType.DIRECTOR)
        {
            assistantToDirectorThread = new Thread(UDP_assistantToDirectorReceive);
            assistantToDirectorThread.IsBackground = true;
            assistantToDirectorThread.Start();
        }

        // listen to director messages if assistant role is set
        if (ModesManager.instance.role == ModesManager.eRoleType.ASSISTANT)
        {
            directorToAssistantThread = new Thread(UDP_directorToAssistantReceive);
            directorToAssistantThread.IsBackground = true;
            directorToAssistantThread.Start();
        }

        startPos = ScreenCamera.transform.position;
        startRot = ScreenCamera.transform.rotation;
    }

    void Update()
    {
        // check if there are messages left to be parsed and check their type
        if (lastMessages.Count > 0)
        {
            // get oldest message stored in the queue and parse it
            string lastMessage = lastMessages.Dequeue();
            string[] splittedMessage = lastMessage.Split(":");

            switch (ModesManager.instance.role)
            {
                // parse director messages
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

                            StartCoroutine(parsePointPosition(receivedPointName, receivedPointPosition));
                            break;
                        case eAssistantToDirectorMessages.NEW_ROTATION:
                            receivedPointName = splittedMessage[1];
                            string receivedPointRotation = splittedMessage[2];

                            parsePointRotation(receivedPointName, receivedPointRotation);
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
                        case eAssistantToDirectorMessages.CHANGE_MINICAMERA_COLOR:
                            string cameraName = splittedMessage[1];
                            pointName = splittedMessage[2];
                            colorHex = splittedMessage[3];
                            StartCoroutine(parseMiniCameraChangeColor(cameraName, pointName, colorHex));
                            break;
                        case eAssistantToDirectorMessages.RELOCATE_POINT:
                            string pathNum = splittedMessage[1];
                            pointNum = int.Parse(splittedMessage[2]);
                            string direction = splittedMessage[3];

                            string directionInv = "";
                            if (splittedMessage.Length == 5)
                                directionInv = splittedMessage[4];
                            parsePointRelocation(pathNum, pointNum, direction, directionInv);
                            break;
                        case eAssistantToDirectorMessages.RELOCATE_CAMERA_POINTS:
                            cameraName = splittedMessage[1];
                            string startPosition = splittedMessage[2];
                            parseCameraPointsRelocation(cameraName, startPosition);
                            break;
                        case eAssistantToDirectorMessages.ITEMS_MENU_NAVIGATION:
                            string action = splittedMessage[1];
                            string button = splittedMessage[2];
                            parseItemsMenuNavigation(action, button);
                            break;
                    }
                    break;

                // parse assistant messages
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
        }
    }
}