using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using UnityEngine;
using System.Globalization;
using System;

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
    public bool continueLine;
    // pel cas continu
    public bool startLine;

    // Network
    UdpClient client;
    public int serverPort = 8051;
    public string ipAddress;

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
            GameObject line = Instantiate(linePrefab, Vector3.zero, Quaternion.identity);
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
}