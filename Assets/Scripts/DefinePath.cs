using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class DefinePath : MonoBehaviour
{
    public static DefinePath instance = null;

    [SerializeField] GameObject spherePrefab;
    [SerializeField] GameObject linePrefab;
    [SerializeField] GameObject emptyPrefab;
    [SerializeField] GameObject handController;
    [SerializeField] int pathPointsPort = 8051;

    public Color defaultLineColor = new Color(0.5176471f, 0.7504352f, 0.8078431f);
    public Color selectedLineColor = new Color(0.5176471f, 0.7504352f, 0.8078431f);
    public Color hoverLineColor = new Color(0.6554998f, 0.4750979f, 0.8773585f);

    private LineRenderer currLineRenderer;
    private GameObject currEmptyGO;
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

    public void addPathPositon(Vector3 newPosition, int pointsCount)
    {
        if (pointsCount == 0)
        {
            currEmptyGO = Instantiate(emptyPrefab);
            GameObject line = Instantiate(linePrefab);
            currLineRenderer = line.GetComponent<LineRenderer>();

            line.transform.SetParent(currEmptyGO.transform);
        }
        // add a new position
        else
            currLineRenderer.positionCount += 1;

        currLineRenderer.SetPosition(pointsCount, newPosition);

        GameObject spherePoint = Instantiate(spherePrefab);
        spherePoint.transform.position = newPosition;
        spherePoint.transform.rotation = Quaternion.identity;
        spherePoint.transform.SetParent(currEmptyGO.transform);
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
    }
}
