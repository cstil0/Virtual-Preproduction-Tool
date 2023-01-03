using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine.UI;
using System.IO;
using System.Globalization;

public class UDPReceiver : MonoBehaviour
{
    // udpclient object
    UdpClient client;
    UdpClient clientPath;
    public int serverPort = 8050;
    public int pathPointsPort = 8051;

    Thread receiveThread;
    Thread receivePathPointsThread;

    bool pointParsed;
    String receivedMessage;
    String receivedName;
    int receivedCount;
    double receivedPointX;
    double receivedPointY;
    double receivedPointZ;

    public Camera ScreenCamera;
    Vector3 startPos;
    Vector3 remoteStartPos;
    Vector3 currPos;

    //Vector3 startRot;
    Quaternion startRot;
    //Vector3 remoteStartRot;
    Quaternion remoteStartRot;
    //Vector3 currRot;
    Quaternion currRot;

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

    void UDP_PathPointsReceive() 
    {
        UdpClient clientPath = new UdpClient(pathPointsPort);
        // loop needed to keep listening
        while (true)
        {
            try
            {
                // recieve messages through the end point
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, pathPointsPort);
                byte[] receiveBytes = clientPath.Receive(ref remoteEndPoint);
                receivedName = Encoding.ASCII.GetString(receiveBytes);

                receiveBytes = clientPath.Receive(ref remoteEndPoint);
                receivedCount = int.Parse(Encoding.ASCII.GetString(receiveBytes));

                receiveBytes = clientPath.Receive(ref remoteEndPoint);
                receivedPointX = BitConverter.ToDouble(receiveBytes);

                receiveBytes = clientPath.Receive(ref remoteEndPoint);
                receivedPointY = BitConverter.ToDouble(receiveBytes);

                receiveBytes = clientPath.Receive(ref remoteEndPoint);
                receivedPointZ = BitConverter.ToDouble(receiveBytes);

                pointParsed = false;
            }
            catch (Exception e)
            {
                Debug.Log("Exception thrown " + e.Message);
            }
        }
    }

    void parsePoint()
    {
        pointParsed = true;
        GameObject character = GameObject.Find(receivedName);

        //string[] splittedMessage = receivedPoint.Split(" ");
        //Vector3 newPoint = new Vector3(float.Parse(splittedMessage[0]), float.Parse(splittedMessage[1]), float.Parse(splittedMessage[2]));

        Vector3 newPoint = new Vector3((float)receivedPointX, (float)receivedPointY, (float)receivedPointZ);

        character.GetComponent<FollowPath>().pathPositions.Add(newPoint);

        // reset count points to instantiate a new line
        if (receivedCount == 1)
            DrawLine.instance.countPoints = receivedCount - 1;

        DrawLine.instance.drawLine(newPoint);
    }

    void OnDisable()
    {
        // stop thread when object is disabled
        if (receiveThread != null)
            receiveThread.Abort();

        if (receivePathPointsThread != null)
            receivePathPointsThread.Abort();

        client.Close();
        clientPath.Close();
    }
    // Start is called before the first frame update
    void Start()
    {
        // Start thread to listen UDP messages and set it as background
        receiveThread = new Thread(UDP_ReceieveThread);
        receiveThread.IsBackground = true;
        receiveThread.Start();

        receivePathPointsThread = new Thread(UDP_PathPointsReceive);
        receivePathPointsThread.IsBackground = true;
        receivePathPointsThread.Start();

        startPos = ScreenCamera.transform.position;
        //startRot = ScreenCamera.transform.rotation.eulerAngles;
        startRot = ScreenCamera.transform.rotation;

        pointParsed = true;
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

        if (!pointParsed)
            parsePoint();
    }
}