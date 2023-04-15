using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Unity.Netcode;
using UnityEngine;

public class DefinePath : MonoBehaviour
{
    public static DefinePath instance = null;

    public bool isPlaying = false;
    [SerializeField] Light areaLight;

    public GameObject spherePrefab;
    public GameObject sphereCameraPrefab;
    public GameObject circlePrefab;
    [SerializeField] GameObject linePrefab;
    [SerializeField] GameObject emptyPrefab;
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

    public GameObject addPointToNewPath(Vector3 newPosition, Quaternion newRotation, int pointsCount, GameObject item, bool isCamera, float startDifferenceY = 0.0f)
    {
        // intantiate the empty GameObject, line renderer and sphere to show the defined points
        GameObject pathContainer = Instantiate(emptyPrefab);
        GameObject line = Instantiate(linePrefab);

        // can be with or without camera depending on who called this function

        pathContainer.GetComponent<NetworkObject>().Spawn();
        line.GetComponent<NetworkObject>().Spawn();

        GameObject spherePoint;
        if (isCamera)
        {
            spherePoint = Instantiate(sphereCameraPrefab);
            spherePoint.GetComponent<NetworkObject>().Spawn();
        }
        else
        {
            spherePoint = Instantiate(spherePrefab);
            GameObject circlePoint = Instantiate(circlePrefab);
            circlePoint.GetComponent<NetworkObject>().Spawn();
            spherePoint.GetComponent<NetworkObject>().Spawn();

            circlePoint.transform.SetParent(spherePoint.transform);
            circlePoint.transform.position = new Vector3(0.0f, -startDifferenceY, 0.0f);
        }

        // insert sphere and linerenderer inside the path container
        line.transform.SetParent(pathContainer.transform);
        spherePoint.transform.SetParent(pathContainer.transform);

        // set the new point to the line renderer in the 0 index
        LineRenderer currLineRenderer = line.GetComponent<LineRenderer>();
        currLineRenderer.SetPosition(pointsCount, newPosition);

        if (spherePoint.transform.childCount > 0)
        {
            GameObject sphere = spherePoint.transform.GetChild(1).gameObject;
            sphere.GetComponent<PathSpheresController>().item = item;
            sphere.GetComponent<PathSpheresController>().getFollowPath();

            // assign follow path camera to the camera rotation controller as it needs to access it
            FollowPathCamera followPathCamera = sphere.GetComponent<PathSpheresController>().followPathCamera;
            // get mini camera and assign the follow path camera component
            GameObject miniCamera = spherePoint.transform.Find("MiniCamera").gameObject;
            miniCamera.GetComponent<CameraRotationController>().followPathCamera = followPathCamera;
            miniCamera.transform.rotation = newRotation;
        }
        else
        {
            spherePoint.GetComponent<PathSpheresController>().item = item;
            spherePoint.GetComponent<PathSpheresController>().getFollowPath();
        }

        // define position and rotation to the sphere
        spherePoint.transform.position = newPosition;

        // change names according to the counts so that it is easy to identify and search for each point and path
        if (item.name.Contains("MainCamera"))
            pathContainer.transform.name = "Path " + item.name;
        else
            pathContainer.transform.name = "Path " + itemsCount;

        line.transform.name = "Line";
        spherePoint.transform.name = "Point " + pointsCount;

        return pathContainer;
    }

    public void addPointToExistentPath(GameObject pathContainer, Vector3 newPosition, Quaternion newRotation, int pointsCount, GameObject item, bool isCamera, float startDifferenceY = 0.0f)
    {
        GameObject line = pathContainer.transform.GetChild(0).gameObject;
        LineRenderer currLineRenderer = line.GetComponent<LineRenderer>();

        GameObject spherePoint;
        if (isCamera)
        {
            spherePoint = Instantiate(sphereCameraPrefab);
            spherePoint.GetComponent<NetworkObject>().Spawn();
        }
        else
        {
            spherePoint = Instantiate(spherePrefab);
            GameObject circlePoint = Instantiate(circlePrefab);
            circlePoint.GetComponent<NetworkObject>().Spawn();
            spherePoint.GetComponent<NetworkObject>().Spawn();

            circlePoint.transform.SetParent(spherePoint.transform);
            circlePoint.transform.position = new Vector3(0.0f, -startDifferenceY, 0.0f);
        }

        currLineRenderer.positionCount += 1;
        currLineRenderer.SetPosition(pointsCount, newPosition);

        if (spherePoint.transform.childCount > 0)
        {
            GameObject sphere = spherePoint.transform.GetChild(1).gameObject;
            sphere.GetComponent<PathSpheresController>().item = item;
            sphere.GetComponent<PathSpheresController>().getFollowPath();

            // assign follow path camera to the camera rotation controller as it needs to access it. Try is needed if it is null
            FollowPathCamera followPathCamera = sphere.GetComponent<PathSpheresController>().followPathCamera;
            // get mini camera and assign the follow path camera component
            GameObject miniCamera = spherePoint.transform.Find("MiniCamera").gameObject;
            miniCamera.GetComponent<CameraRotationController>().followPathCamera = followPathCamera;
            miniCamera.transform.rotation = newRotation;
        }
        else
        {
            spherePoint.GetComponent<PathSpheresController>().item = item;
            spherePoint.GetComponent<PathSpheresController>().getFollowPath();
        }

        spherePoint.transform.name = "Point " + pointsCount;
        spherePoint.transform.position = newPosition;
        spherePoint.transform.rotation = Quaternion.identity;
        spherePoint.transform.SetParent(pathContainer.transform);
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
        for (int i = 0; i < pathContainer.transform.childCount; i++)
        {
            if (i == pointNum)
                Destroy(pathContainer.transform.GetChild(i + 1).gameObject);
            // the substraction is due to the fact we are starting at the second position
            if (i > pointNum)
                pathContainer.transform.GetChild(i).name = "Point " + (i - 2);
        }
    }

    // NEEDS DEVELOPEMENT
    public void changePathColor(GameObject pathContainer, Color pathColor)
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
                Renderer renderer = currChild.GetComponent<Renderer>();
                Material material = renderer.material;
                material.color = pathColor;
            }
        }
    }

    //public int getItemsCount()
    //{
    //    return itemsMenuController.itemsCount;
    //}
}
