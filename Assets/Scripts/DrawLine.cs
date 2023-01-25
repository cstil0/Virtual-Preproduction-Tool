using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using UnityEngine;
using System.Globalization;
using System;
using UnityEngine.Experimental.GlobalIllumination;

public class DrawLine : MonoBehaviour
{
    // singleton
    public static DrawLine instance = null;

    public GameObject linePrefab;
    public GameObject handController;
    private GameObject currentLine;
    private EdgeCollider2D edgeCollider;
    private List<Vector2> fingerPositions = new List<Vector2>();

    private LineRenderer lineRenderer;
    public int countPoints;
    bool buttonDown;
    public bool lineAlreadyInstantiated = false;
    public bool continueLine;
    // pel cas continu
    public bool startLine;
    public int lastPathID = 0;
    public Color defaultLineColor = new Color(0.5176471f, 0.7504352f, 0.8078431f);
    public Color hoverLineColor = new Color(0.6554998f, 0.4750979f, 0.8773585f);
    public Color[] pathColors = { new Color(0.2745098f, 0.09019608f, 0.03137255f),
                                  new Color(0.9372549f, 0.5176471f, 0.1568628f),
                                  new Color(0.7372549f, 0.682353f, 0.5333334f),
                                  new Color(0.5568628f, 0.08235294f, 0.2f),
                                  new Color(0.07058824f, 0.2509804f, 0.2352941f) };

    // Network
    UdpClient client;
    [SerializeField] int serverPort = 8051;
    //public string ipAddress;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }
    private void Start()
    {
        buttonDown = false;
        continueLine = false;
        countPoints = 0;
    }

    void Update()
    {
        //if (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger))
        //{
        //    CreateLine();
        //}

        // VERSIÓN POR PUNTOS
        //if (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger))
        //{
        //    if (!buttonDown)
        //    {
        //        buttonDown = true;

        //        if (continueLine)
        //        {
        //            if (countPoints == 0)
        //            {
        //                GameObject line = Instantiate(linePrefab, Vector3.zero, Quaternion.identity);
        //                lineRenderer = line.GetComponent<LineRenderer>();
        //                lineRenderer.SetPosition(countPoints, handController.transform.position);
        //                lineRenderer.positionCount = 2;
        //                countPoints += 1;
        //            }
        //            else
        //            {
        //                lineRenderer.SetPosition(countPoints, handController.transform.position);
        //                lineRenderer.positionCount += 1;
        //                countPoints += 1;
        //            }
        //        }
        //        else
        //        {
        //            countPoints = 0;
        //        }
        //    }
        //}
        //else
        //{
        //    buttonDown = false;
        //}

        // VERSIÓN CONTINUA
        if (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger) && startLine)
        {
            drawLine(handController.transform.position);
        }
        // in director mode, countPoints is given by UDP receiver
        else if (ModesManager.instance.role == ModesManager.eRoleType.ASSISTANT)
        {
            countPoints = 0;
        }
    }

    public void drawLine(Vector3 newPoint)
    {
        if (countPoints == 0)
        {
            lastPathID += 1;
            GameObject line = Instantiate(linePrefab, Vector3.zero, Quaternion.identity);
            lineAlreadyInstantiated = true;
            line.transform.name = "Path " + lastPathID;
            lineRenderer = line.GetComponent<LineRenderer>();
            lineRenderer.SetPosition(countPoints, newPoint);
            lineRenderer.positionCount = 1;
            countPoints += 1;
        }
        else
        {
            lineRenderer.positionCount += 1;
            lineRenderer.SetPosition(countPoints, newPoint);
            countPoints += 1;
        }
    }

    public void SendPointPath(GameObject character, Vector3 pathPoint)
    {
        try
        {
            client = new UdpClient(serverPort);

            string ipAddress = ModesManager.instance.IPAddress.text;
            // sending data
            IPEndPoint target = new IPEndPoint(IPAddress.Parse(ipAddress), serverPort);

            byte[] message = Encoding.ASCII.GetBytes(character.transform.name);
            client.Send(message, message.Length, target);

            message = Encoding.ASCII.GetBytes(countPoints.ToString());
            client.Send(message, message.Length, target);

            message = Encoding.ASCII.GetBytes(pathPoint.x.ToString(CultureInfo.InvariantCulture) + " " + pathPoint.y.ToString(CultureInfo.InvariantCulture) + " " + pathPoint.z.ToString(CultureInfo.InvariantCulture));
            client.Send(message, message.Length, target);

            client.Close();
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    public Color getPathColor(int pathID)
    {

        return pathColors[pathID - 1];

        //float colorFactor = pathID * 0.1f;

        // this is just to create paths in a more dynamic way
        //if (pathID % 2 == 0)
        //    return new Color(defaultLineColor.r * colorFactor, defaultLineColor.g * colorFactor, defaultLineColor.b);
        //else
        //    return new Color(defaultLineColor.r, defaultLineColor.g * colorFactor, defaultLineColor.b * colorFactor);
    }

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