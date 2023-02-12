using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using static UnityEditor.Progress;

public class DefinePath : MonoBehaviour
{
    public static DefinePath instance = null;

    [SerializeField] GameObject spherePrefab;
    [SerializeField] GameObject linePrefab;
    [SerializeField] GameObject emptyPrefab;
    [SerializeField] GameObject handController;
    [SerializeField] int pathPointsPort = 8051;

    public Color defaultLineColor = new Color(0.5176471f, 0.7504352f, 0.8078431f);
    public Color hoverLineColor = new Color(0.5176471f, 0.7504352f, 0.8078431f);
    public Color selectedLineColor = new Color(0.6554998f, 0.4750979f, 0.8773585f);

    public int pathsCount = 0;
    private UdpClient udpClient;
    public bool isThereCharacterSelected = false;
    private bool secondaryIndexTriggerDown = false;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
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

    public GameObject addPointToNewPath(Vector3 newPosition, int pointsCount, GameObject item)
    {
        // intantiate the empty GameObject, line renderer and sphere to show the defined points
        GameObject pathContainer = Instantiate(emptyPrefab);
        GameObject line = Instantiate(linePrefab);
        GameObject spherePoint = Instantiate(spherePrefab);

        // insert sphere and linerenderer inside the path container
        line.transform.SetParent(pathContainer.transform);
        spherePoint.transform.SetParent(pathContainer.transform);
        // set the new point to the line renderer in the 0 index
        LineRenderer currLineRenderer = line.GetComponent<LineRenderer>();
        currLineRenderer.SetPosition(pointsCount, newPosition);

        spherePoint.GetComponent<PathSpheresController>().item = item;
        spherePoint.GetComponent<PathSpheresController>().getFollowPath();

        // define position and rotation to the sphere
        spherePoint.transform.position = newPosition;
        spherePoint.transform.rotation = Quaternion.identity;

        // change names according to the counts so that it is easy to identify and search for each point and path
        pathContainer.transform.name = "Path " + pathsCount;
        line.transform.name = "Line";
        spherePoint.transform.name = "Point " + pointsCount;

        pathsCount++;

        return pathContainer;
    }

    public void addPointToExistentPath(GameObject pathContainer, Vector3 newPosition, int pointsCount, GameObject item)
    {
        // add a new position
        GameObject spherePoint = Instantiate(spherePrefab);
        GameObject line = pathContainer.transform.Find("Line").gameObject;
        LineRenderer currLineRenderer = line.GetComponent<LineRenderer>();

        currLineRenderer.positionCount += 1;
        currLineRenderer.SetPosition(pointsCount, newPosition);

        spherePoint.GetComponent<PathSpheresController>().item = item;
        spherePoint.GetComponent<PathSpheresController>().getFollowPath();

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
        List<Vector3> pathPositionsList = pathPositionsArray.ToList<Vector3>();
        pathPositionsList.RemoveAt(pointNum);

        pathPositionsArray = pathPositionsList.ToArray();
        currLineRenderer.SetPositions(pathPositionsArray);
        currLineRenderer.positionCount = pointsCount - 1;

        // change following points name
        // start in second child since first one corresponds to the line renderer
        for (int i = 1; i < pathContainer.transform.childCount; i++)
        {
            // the substraction is due to the fact we are starting at the second position
            if (i - 1 > pointNum)
                pathContainer.transform.GetChild(i).name = "Point " + (i - 2);
        }
    }

    public void sendPointPath(GameObject character, Vector3 pathPoint)
    {
        try
        {
            udpClient = new UdpClient(pathPointsPort);

            string ipAddress = ModesManager.instance.IPAddress.text;
            // sending data
            IPEndPoint target = new IPEndPoint(IPAddress.Parse(ipAddress), pathPointsPort);

            byte[] message = Encoding.ASCII.GetBytes(character.transform.name);
            udpClient.Send(message, message.Length, target);

            message = Encoding.ASCII.GetBytes(pathPoint.x.ToString(CultureInfo.InvariantCulture) + " " + pathPoint.y.ToString(CultureInfo.InvariantCulture) + " " + pathPoint.z.ToString(CultureInfo.InvariantCulture));
            udpClient.Send(message, message.Length, target);

            udpClient.Close();
        }
        catch (System.Exception e) { }
    }

    // NEEDS DEVELOPEMENT
    public void changePathColor(int pathID, Color pathColor)
    {
        GameObject[] lines = GameObject.FindGameObjectsWithTag("Line");

        foreach (GameObject line in lines)
        {
            if (!line.name.Contains("Path " + pathID))
                continue;

            Renderer renderer = line.GetComponent<Renderer>();
            renderer.material.color = pathColor;
        }

        GameObject[] points = GameObject.FindGameObjectsWithTag("PathPoint");
        foreach (GameObject point in points)
        {
            Renderer renderer = point.GetComponent<Renderer>();
            Material material = renderer.material;
            material.color = pathColor;
        }
    }
}
