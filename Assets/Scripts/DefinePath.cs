using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
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

    public bool isThereCharacterSelected = false;
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
    void Start()
    {
        //generate pre-saved minicamera textures
        for (int i = 0; i < maxCameraPoints; i++)
        {
            miniCameraTextures.Add(new RenderTexture(426, 240, 16, RenderTextureFormat.ARGB32));
        } 
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            playLinePath();
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            stopLinePath();
        }
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
            // instantiate the needed gameobjects
            spherePoint = Instantiate(sphereCameraPrefab);
            GameObject miniCamera = Instantiate(miniCameraPrefab);
            GameObject cameraCanvas = Instantiate(cameraCanvasPrefab);

            spherePoint.transform.position = newPosition;
            spherePoint.transform.rotation = Quaternion.identity;

            GameObject cameraImage = cameraCanvas.transform.GetChild(0).gameObject;
            RawImage rawImage = cameraImage.GetComponent<RawImage>();
            // assign necessary properties
            Camera miniCameraComponent = miniCamera.GetComponent<Camera>();
            miniCameraComponent.targetTexture = miniCameraTextures[pointsCount];
            rawImage.texture = miniCameraTextures[pointsCount];
            cameraCanvas.GetComponent<Canvas>().worldCamera = miniCameraComponent;

            // spawn object in connected clients
            spherePoint.GetComponent<NetworkObject>().Spawn();
            miniCamera.GetComponent<NetworkObject>().Spawn();
            cameraCanvas.GetComponent<NetworkObject>().Spawn();

            miniCamera.transform.SetParent(spherePoint.transform);
            cameraCanvas.transform.SetParent(spherePoint.transform);

            // define the corresponding positions. The values were defined by trial and error
            miniCamera.transform.localPosition = new Vector3(0.0f, 0.15f, 0.0f);
            miniCamera.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            cameraCanvas.transform.localPosition = new Vector3(0.0f, -0.17f, 0.0f);
            cameraCanvas.transform.localScale = new Vector3(0.004f, 0.004f, 0.004f);
            cameraCanvas.transform.localRotation = Quaternion.Euler(new Vector3(0.0f, 180f, 0.0f));
        }
        else
        {
            // instantiate the needed elements
            spherePoint = Instantiate(spherePrefab);
            GameObject circlePoint = Instantiate(circlePrefab);

            spherePoint.transform.position = newPosition;
            spherePoint.transform.rotation = Quaternion.identity;

            // spawn object in connected clients
            spherePoint.GetComponent<NetworkObject>().Spawn();
            circlePoint.GetComponent<NetworkObject>().Spawn();

            // defined with trial and error
            if (startDifferenceY == 0.0f)
                startDifferenceY += 0.001f;

            // define transform properties
            circlePoint.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            circlePoint.transform.position = new Vector3(newPosition.x, startDifferenceY, newPosition.z);
            circlePoint.transform.SetParent(circlesContainer.transform);
            circlePoint.name = "Circle " + pointsCount;

            PathCirclesController pathCirclesController = circlePoint.GetComponent<PathCirclesController>();
            pathCirclesController.pathNum = itemsCount;
            pathCirclesController.pointNum = pointsCount;
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

        // assign path and point num properties
        pathSpheresController.pathNum = itemsCount;
        pathSpheresController.pointNum = pointsCount;

        // save the point as child of the corresponding path container
        spherePoint.transform.name = "Point " + pointsCount;
        spherePoint.transform.SetParent(pathContainer.transform);
    }

    // used to generate the general elements needed for a new path such as the corresponding path container
    public List<GameObject> addPointToNewPath(Vector3 newPosition, Quaternion newRotation, int pointsCount, GameObject item, bool isCamera, float startDifferenceY = 0.0f)
    {
        // intantiate the empty GameObject, line renderer and sphere to show the defined points
        GameObject pathContainer = Instantiate(pathParentPrefab);
        GameObject line = Instantiate(linePrefab);

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
    
        // add the sphere and required elements to visualize the point
        addPointGeneric(pathContainer, newPosition, newRotation, pointsCount, item, isCamera, startDifferenceY, circlesContainer);

        List<GameObject> containers = new List<GameObject>();
        containers.Add(pathContainer);
        if (!isCamera)
            containers.Add(circlesContainer);
        
        return containers;
    }

    // used to define only the point and add it as a child of an existent path container
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

        // add the sphere and required elements to visualize the point
        addPointGeneric(pathContainer, newPosition, newRotation, pointsCount, item, isCamera, startDifferenceY, circlesContainer);
    }

    public void deletePointFromPath(GameObject pathContainer, int pointNum, GameObject circlesContainer = null)
    {
        GameObject line = pathContainer.transform.Find("Line").gameObject;

        // change following points name
        // start in second child since first one corresponds to the line renderer
        for (int i = 1; i < pathContainer.transform.childCount; i++)
        {
            if (i - 1 == pointNum)
            {
                // ensure that no other element is selected since it could not be uselected back
                HoverObjects.instance.currentPointCollider = null;
                HoverObjects.instance.currentMiniCameraCollider = null;
                HoverObjects.instance.pointAlreadySelected = false;
                HoverObjects.instance.miniCameraAlreadySelected = false;

                // destroy minicamera
                if (pathContainer.transform.GetChild(i).tag != "PathPoint")
                {
                    Destroy(pathContainer.transform.GetChild(i).GetChild(1).gameObject);
                    Destroy(pathContainer.transform.GetChild(i).GetChild(2).gameObject);
                }

                GameObject sphere = pathContainer.transform.GetChild(i).gameObject;
                // destroy the corresponding sphere
                Destroy(pathContainer.transform.GetChild(i).gameObject);

                // destroy the corresponding circle
                if (circlesContainer != null)
                    Destroy(circlesContainer.transform.GetChild(i - 1).gameObject);

            }

            // the substraction is due to the fact we are starting at the second position
            else if (i - 1 > pointNum)
            {
                // if is a character point
                if (pathContainer.transform.GetChild(i).tag == "PathPoint")
                {
                    // rename the next points to match their new number in the path
                    GameObject pathSphere = pathContainer.transform.GetChild(i).gameObject;
                    pathSphere.name = "Point " + (i - 2);
                    pathSphere.GetComponent<PathSpheresController>().pointNum = i - 2;

                    GameObject circle = circlesContainer.transform.GetChild(i - 1).gameObject;
                    circle.name = "Circle " + (i - 2);
                    circle.GetComponent<PathCirclesController>().pointNum = i - 2;
                }
                // if is a camera point
                else
                {
                    // rename the next points to match their new number in the path
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

        if (pathContainer.transform.childCount <= 2 && pointNum == 0)
        {
            // destroy line renderer and the container itself
            Destroy(pathContainer.transform.GetChild(0).gameObject);
            Destroy(pathContainer);

            if (circlesContainer != null)
                Destroy(circlesContainer);
        }
    }

    // used when a new camera is selected to reassign the pre-generated canvas to the corresponding points
    public void reassignPathCanvas(int cameraNum)
    {
        GameObject[] pathContainers = GameObject.FindGameObjectsWithTag("PathContainer");

        foreach (GameObject pathContainer in pathContainers)
        {
            if (!pathContainer.name.Contains("MainCamera"))
                continue;

            // get the number of the current path
            string[] splittedName = pathContainer.name.Split(" ");
            int pathNum = int.Parse(splittedName[2]);

            // iterate through all points in the path
            for (int i = 1; i < pathContainer.transform.childCount; i++)
            {
                GameObject pathMiniCamera = pathContainer.transform.GetChild(i).GetChild(1).gameObject;
                Transform pathCameraCanvas = pathContainer.transform.GetChild(i).GetChild(2);
                GameObject rawImage = pathCameraCanvas.GetChild(0).gameObject;

                // if this is the selected camera, assign the pre-generated textures to each corresponding minicamera
                if (pathNum == cameraNum)
                {
                    // reassign camera texture
                    pathMiniCamera.GetComponent<Camera>().targetTexture = miniCameraTextures[i - 1];
                    rawImage.GetComponent<RawImage>().texture = miniCameraTextures[i - 1];
                }
                // else, ensure that the minicamera is not rendering to any texture
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
        // iterate through all points of the path
        for (int i = 0; i < pathContainer.transform.childCount; i++)
        {
            // change the line renderer color
            GameObject currChild = pathContainer.transform.GetChild(i).gameObject;
            if (currChild.name.Contains("Line"))
            {
                Renderer renderer = currChild.GetComponent<Renderer>();
                renderer.material.color = pathColor;
            }
            else if (currChild.name.Contains("Point"))
            {
                // change the minicamera color
                if (pathContainer.name.Contains("MainCamera"))
                {
                    Renderer renderer = currChild.transform.GetChild(0).GetComponent<Renderer>();
                    Material material = renderer.material;
                    material.color = pathColor;

                    GameObject miniCamera = currChild.transform.GetChild(1).gameObject;
                    HoverObjects.instance.changeColorMaterials(miniCamera, pathColor, false);

                    // hide reference view canvas in case the camera is being unseledted
                    currChild.transform.GetChild(2).gameObject.SetActive(isActive);
                }
                else
                {
                    // change the sphere color
                    Renderer renderer = currChild.GetComponent<Renderer>();
                    Material material = renderer.material;
                    material.color = pathColor;

                    // find circles and change their color
                    string pathName = pathContainer.name;
                    string[] splittedName = pathName.Split(" ");
                    int pathNum = int.Parse(splittedName[1]);
                    HoverObjects.instance.callHoverPointEvent(pathNum, i - 1, pathColor);
                }
            }
        }
    }
}
