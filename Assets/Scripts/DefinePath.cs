using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DefinePath : MonoBehaviour
{
    public static DefinePath instance = null;

    public bool isPlaying = false;
    [SerializeField] Light areaLight;

    public GameObject spherePrefab;
    public GameObject sphereCameraPrefab;
    public GameObject circlePrefab;
    public GameObject miniCameraPrefab;
    [SerializeField] GameObject linePrefab;
    [SerializeField] GameObject pathParentPrefab;
    [SerializeField] GameObject circlesParentPrefab;
    [SerializeField] GameObject cameraCanvasPrefab;
    [SerializeField] GameObject handController;
    [SerializeField] int pathPointsPort = 8051;

    public Color defaultLineColor;
    public Color hoverLineColor;
    public Color selectedLineColor;
    public Color playLightColor;

    private UdpClient udpClient;
    public bool isThereCharacterSelected = false;
    private bool secondaryIndexTriggerDown = false;
    public int itemsCount;

    public delegate void PathPositionChanged(int pathNum, int pointNum, Vector3 distance);
    public event PathPositionChanged OnPathPositionChanged;

    public List<RenderTexture> miniCameraTextures;
    public int maxCameraPoints;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    private void OnEnable()
    {
        DirectorPanelManager.instance.OnPlayPath += playLinePath;
        DirectorPanelManager.instance.OnStopPath += stopLinePath;
    }
    private void OnDisable()
    {
        DirectorPanelManager.instance.OnPlayPath -= playLinePath;
        DirectorPanelManager.instance.OnStopPath -= stopLinePath;
    }
    // Start is called before the first frame update
    void Start()
    {
        //generate pre-saved minicamera textures
        for (int i = 0; i < maxCameraPoints; i++)
        {
            miniCameraTextures.Add(new RenderTexture(426, 240, 16, RenderTextureFormat.ARGB32));
        } 
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))// || OVRInput.Get(OVRInput.RawButton.X))
        {
            playLinePath();
        }
        else if (Input.GetKeyDown(KeyCode.S))// || OVRInput.Get(OVRInput.RawButton.Y))
        {
            stopLinePath();
        }
        //if (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger) && isThereCharacterSelected)
        //{
        //    if (!secondaryIndexTriggerDown)
        //        drawLine();
        //    secondaryIndexTriggerDown = true;
        //}
        //else
        //{
        //    secondaryIndexTriggerDown = false;
        //    currPointsCount = 0;
        //}
    }

    public void triggerPointPathChanged(int pathNum, int pointNum, Vector3 distance)
    {
        OnPathPositionChanged(pathNum, pointNum, distance);
    }
    
    void playLinePath()
    {
        isPlaying = !isPlaying;
        // change lighting to let the user know we are in play mode
        if (ModesManager.instance.role == ModesManager.eRoleType.ASSISTANT)
            areaLight.color = playLightColor;
    }

    void stopLinePath()
    {
        isPlaying = false;
        if (ModesManager.instance.role == ModesManager.eRoleType.ASSISTANT)
            areaLight.color = Color.white;
    }

    private void addPointGeneric(GameObject pathContainer, Vector3 newPosition, Quaternion newRotation, int pointsCount, GameObject item, bool isCamera, float startDifferenceY = 0.0f, GameObject circlesContainer = null)
    {
        GameObject spherePoint;
        if (isCamera)
        {
            spherePoint = Instantiate(sphereCameraPrefab);
            GameObject miniCamera = Instantiate(miniCameraPrefab);
            GameObject cameraCanvas = Instantiate(cameraCanvasPrefab);

            GameObject cameraImage = cameraCanvas.transform.GetChild(0).gameObject;
            RawImage rawImage = cameraImage.GetComponent<RawImage>();
            // assign necessary properties
            Camera miniCameraComponent = miniCamera.GetComponent<Camera>();
            miniCameraComponent.targetTexture = miniCameraTextures[pointsCount];
            rawImage.texture = miniCameraTextures[pointsCount];
            cameraCanvas.GetComponent<Canvas>().worldCamera = miniCameraComponent;

            miniCamera.GetComponent<NetworkObject>().Spawn();
            spherePoint.GetComponent<NetworkObject>().Spawn();
            cameraCanvas.GetComponent<NetworkObject>().Spawn();

            miniCamera.transform.SetParent(spherePoint.transform);
            cameraCanvas.transform.SetParent(spherePoint.transform);

            miniCamera.transform.localPosition = new Vector3(0.0f, 0.15f, 0.0f);
            miniCamera.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            cameraCanvas.transform.localPosition = new Vector3(0.0f, -0.17f, 0.0f);
            cameraCanvas.transform.localScale = new Vector3(0.004f, 0.004f, 0.004f);
            cameraCanvas.transform.localRotation = Quaternion.Euler(new Vector3(0.0f, 180f, 0.0f));
        }
        else
        {
            spherePoint = Instantiate(spherePrefab);
            GameObject circlePoint = Instantiate(circlePrefab);

            // defined with trial and error
            //float circleOffset = -9.0f;
            if (startDifferenceY == 0.0f)
                startDifferenceY += 0.001f;

            circlePoint.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            circlePoint.transform.position = new Vector3(newPosition.x, startDifferenceY, newPosition.z);
            circlePoint.transform.SetParent(circlesContainer.transform);
            circlePoint.name = "Circle " + pointsCount;

            PathCirclesController pathCirclesController = circlePoint.GetComponent<PathCirclesController>();
            pathCirclesController.pathNum = itemsCount;
            pathCirclesController.pointNum = pointsCount;
            circlePoint.GetComponent<NetworkObject>().Spawn();
        }

        PathSpheresController pathSpheresController;
        if (isCamera)
        {
            GameObject sphere = spherePoint.transform.GetChild(0).gameObject;
            pathSpheresController = sphere.GetComponent<PathSpheresController>();
            pathSpheresController.item = item;
            pathSpheresController.getFollowPath();

            // assign follow path camera to the camera rotation controller as it needs to access it. Try is needed if it is null
            FollowPathCamera followPathCamera = sphere.GetComponent<PathSpheresController>().followPathCamera;
            // get mini camera and assign the follow path camera component
            GameObject miniCamera = spherePoint.transform.GetChild(1).gameObject;
            miniCamera.GetComponent<CameraRotationController>().followPathCamera = followPathCamera;
            miniCamera.GetComponent<CameraRotationController>().pointNum = pointsCount;
            miniCamera.transform.rotation = newRotation;

            miniCamera.transform.name = "MiniCamera " + pointsCount;
            miniCamera.GetComponent<CameraRotationController>().pointNum = pointsCount;
        }
        else
        {
            pathSpheresController = spherePoint.GetComponent<PathSpheresController>();
            pathSpheresController.item = item;
            pathSpheresController.getFollowPath();
        }

        pathSpheresController.pathNum = itemsCount;
        pathSpheresController.pointNum = pointsCount;

        spherePoint.transform.name = "Point " + pointsCount;
        spherePoint.transform.position = newPosition;
        spherePoint.transform.rotation = Quaternion.identity;
        spherePoint.GetComponent<NetworkObject>().Spawn();
        spherePoint.transform.SetParent(pathContainer.transform);
    }

    public List<GameObject> addPointToNewPath(Vector3 newPosition, Quaternion newRotation, int pointsCount, GameObject item, bool isCamera, float startDifferenceY = 0.0f)
    {
        // intantiate the empty GameObject, line renderer and sphere to show the defined points
        GameObject pathContainer = Instantiate(pathParentPrefab);

        GameObject line = Instantiate(linePrefab);

        // can be with or without camera depending on who called this function

        pathContainer.GetComponent<NetworkObject>().Spawn();
        line.GetComponent<NetworkObject>().Spawn();

        GameObject circlesContainer = null; 
        if (!isCamera)
        {
            circlesContainer = Instantiate(circlesParentPrefab);
            circlesContainer.GetComponent<NetworkObject>().Spawn();
            circlesContainer.name = "Circles " + itemsCount;
            pathContainer.transform.name = "Path " + itemsCount;
        }
        else
            pathContainer.transform.name = "Path " + item.name;

        // insert sphere and linerenderer inside the path container
        line.transform.SetParent(pathContainer.transform);

        // set the new point to the line renderer in the 0 index
        LineRenderer currLineRenderer = line.GetComponent<LineRenderer>();
        currLineRenderer.SetPosition(pointsCount, newPosition);

        line.transform.name = "Line";
    
        addPointGeneric(pathContainer, newPosition, newRotation, pointsCount, item, isCamera, startDifferenceY, circlesContainer);

        List<GameObject> containers = new List<GameObject>();
        containers.Add(pathContainer);
        if (!isCamera)
            containers.Add(circlesContainer);
        
        return containers;
    }

    public void addPointToExistentPath(GameObject pathContainer, Vector3 newPosition, Quaternion newRotation, int pointsCount, GameObject item, bool isCamera, float startDifferenceY = 0.0f)
    {
        string pathName = pathContainer.name;
        string[] splittedName = pathName.Split(" ");
        int pathNum = -1;
        if (isCamera)
            pathNum = int.Parse(splittedName[2]);
        else
            pathNum = int.Parse(splittedName[1]);

        GameObject circlesContainer = GameObject.Find("Circles " + pathNum);

        GameObject line = pathContainer.transform.GetChild(0).gameObject;
        LineRenderer currLineRenderer = line.GetComponent<LineRenderer>();

        currLineRenderer.positionCount += 1;
        currLineRenderer.SetPosition(pointsCount, newPosition);

        addPointGeneric(pathContainer, newPosition, newRotation, pointsCount, item, isCamera, startDifferenceY, circlesContainer);
    }

    public void deletePointFromPath(GameObject pathContainer, int pointNum)
    {
        GameObject line = pathContainer.transform.Find("Line").gameObject;
        LineRenderer currLineRenderer = line.GetComponent<LineRenderer>();
        int pointsCount = currLineRenderer.positionCount;
        Vector3[] pathPositionsArray = new Vector3[pointsCount];
        currLineRenderer.GetPositions(pathPositionsArray);
        // we cannot modify a linerenderer point, but we can copy them to a list, modify it and assign the list again
        List<Vector3> pathPositionsList = pathPositionsArray.ToList<Vector3>();
        pathPositionsList.RemoveAt(pointNum);

        pathPositionsArray = pathPositionsList.ToArray();
        currLineRenderer.SetPositions(pathPositionsArray);
        currLineRenderer.positionCount = pointsCount - 1;

        // change following points name
        // start in second child since first one corresponds to the line renderer
        for (int i = 1; i < pathContainer.transform.childCount; i++)
        {
            if (i - 1 == pointNum)
            {
                // destroy minicamera
                if (pathContainer.transform.GetChild(i).tag != "PathPoint")
                    Destroy(pathContainer.transform.GetChild(i).GetChild(1).gameObject);

                Destroy(pathContainer.transform.GetChild(i).gameObject);
            }
            // the substraction is due to the fact we are starting at the second position
            else if (i - 1 > pointNum)
            {
                if (pathContainer.transform.GetChild(i).tag == "PathPoint")
                {
                    GameObject pathSphere = pathContainer.transform.GetChild(i).gameObject;
                    pathSphere.name = "Point " + (i - 2);
                    pathSphere.GetComponent<PathSpheresController>().pointNum = i - 2;
                }
                else
                {
                    GameObject pathSphere = pathContainer.transform.GetChild(i).gameObject;
                    GameObject pathMiniCamera = pathContainer.transform.GetChild(i).GetChild(1).gameObject;
                    Transform pathCameraCanvas = pathContainer.transform.GetChild(i).GetChild(2);
                    GameObject rawImage = pathCameraCanvas.GetChild(0).gameObject;
                    pathSphere.name = "Point " + (i - 2);
                    pathMiniCamera.name = "MiniCamera " + (i - 2);

                    // reassign camera texture
                    pathMiniCamera.GetComponent<Camera>().targetTexture = miniCameraTextures[i - 2];
                    rawImage.GetComponent<RawImage>().texture = miniCameraTextures[i - 2];
                }
            }
        }

        HoverObjects.instance.currentPointCollider = null;
        HoverObjects.instance.currentMiniCameraCollider = null;
        HoverObjects.instance.pointAlreadySelected = false;
        HoverObjects.instance.miniCameraAlreadySelected = false;
    }

    public void reassignPathCanvas(int cameraNum)
    {
        GameObject[] pathContainers = GameObject.FindGameObjectsWithTag("PathContainer");

        foreach (GameObject pathContainer in pathContainers)
        {
            string[] splittedName = pathContainer.name.Split(" ");
            int pathNum = int.Parse(splittedName[2]);
            for (int i = 1; i < pathContainer.transform.childCount; i++)
            {
                GameObject pathMiniCamera = pathContainer.transform.GetChild(i).GetChild(1).gameObject;
                Transform pathCameraCanvas = pathContainer.transform.GetChild(i).GetChild(2);
                GameObject rawImage = pathCameraCanvas.GetChild(0).gameObject;

                if (pathNum == cameraNum)
                {
                    // reassign camera texture
                    pathMiniCamera.GetComponent<Camera>().targetTexture = miniCameraTextures[i - 1];
                    rawImage.GetComponent<RawImage>().texture = miniCameraTextures[i - 1];
                }
                else
                {
                    pathMiniCamera.GetComponent<Camera>().targetTexture = null;
                    rawImage.GetComponent<RawImage>().texture = null;
                }
            }
        }
    }

    public void changePathColor(GameObject pathContainer, Color pathColor, bool isActive)
    {
        for (int i = 0; i < pathContainer.transform.childCount; i++)
        {
            GameObject currChild = pathContainer.transform.GetChild(i).gameObject;
            if (currChild.name.Contains("Line"))
            {
                Renderer renderer = currChild.GetComponent<Renderer>();
                renderer.material.color = pathColor;
            }
            else if (currChild.name.Contains("Point"))
            {
                if (pathContainer.name.Contains("MainCamera"))
                {
                    Renderer renderer = currChild.transform.GetChild(0).GetComponent<Renderer>();
                    Material material = renderer.material;
                    material.color = pathColor;

                    GameObject miniCamera = currChild.transform.GetChild(1).gameObject;
                    HoverObjects.instance.changeColorMaterials(miniCamera, pathColor, false);
                    Canvas minicameraCanvas = currChild.transform.GetChild(2).GetComponent<Canvas>();
                    RawImage minicameraImage = currChild.transform.GetChild(2).GetComponentInChildren<RawImage>();

                    currChild.transform.GetChild(2).gameObject.SetActive(isActive);
                    //minicameraCanvas.enabled = isActive;
                    //minicameraImage.enabled = isActive;
                }
                else
                {
                    Renderer renderer = currChild.GetComponent<Renderer>();
                    Material material = renderer.material;
                    material.color = pathColor;

                    // find circles and change their color for characters
                    string pathName = pathContainer.name;
                    string[] splittedName = pathName.Split(" ");
                    int pathNum = int.Parse(splittedName[1]);
                    HoverObjects.instance.callHoverPointEvent(pathNum, i - 1, pathColor);
                }
            }
        }
    }

    //public int getItemsCount()
    //{
    //    return itemsMenuController.itemsCount;
    //}
}
