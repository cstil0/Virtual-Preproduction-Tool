using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Globalization;

public class UDPReceiver : MonoBehaviour
{
    // udpclient object
    //UdpClient client;
    UdpClient clientPath;
    UdpClient clientPlay;
    UdpClient clientRotation;
    // separate it in different ports since this is used both for assistant and director,
    // so that it is less messy for me
    [SerializeField] int serverPort = 8050;
    [SerializeField] int assistantToDirectorPort = 8051;
    [SerializeField] int pathPlayPort = 8052;
    [SerializeField] int rotateScenePort = 8053;

    //Thread receiveThread;
    Thread assistantToDirectorThread;
    Thread receivePlayPathThread;
    Thread receiveSceneRotationThread;

    bool pointParsed;
    bool newItemParsed;
    bool playParsed;
    bool rotationParsed;

    String receivedMessage;
    String receivedName;
    String newReceivedName;
    int receivedCount;
    string receivedPoint;
    string receivedPlay;
    string receivedRotation;
    //double receivedPointX;
    //double receivedPointY;
    //double receivedPointZ;

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


    public GameObject hermione;
    public GameObject harry;

    enum assistantToDirectorMessages{
        NEW_ITEM,
        NEW_POINT
    }

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
                byte[] receiveBytes = clientPath.Receive(ref remoteEndPoint);

                string receivedString = Encoding.ASCII.GetString(receiveBytes);
                string[] splittedMessage = receivedString.Split(":");
                assistantToDirectorMessages message_enum = (assistantToDirectorMessages)Enum.Parse(typeof(assistantToDirectorMessages), splittedMessage[0]);
                string message = splittedMessage[1];

                switch (message_enum)
                {
                    case assistantToDirectorMessages.NEW_ITEM:
                        newReceivedName = message;
                        newItemParsed = false;
                        break;
                    case assistantToDirectorMessages.NEW_POINT:
                        receivedName = message;
                        receiveBytes = clientPath.Receive(ref remoteEndPoint);
                        receivedPoint = Encoding.ASCII.GetString(receiveBytes);

                        pointParsed = false;
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.Log("Exception thrown " + e.Message);
            }
        }
    }

    void UDP_PlayPathReceive()
    {
        clientPlay = new UdpClient(pathPlayPort);
        // loop needed to keep listening
        while (true)
        {
            try
            {
                // recieve messages through the end point
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, pathPlayPort);
                byte[] receiveBytes = clientPlay.Receive(ref remoteEndPoint);
                receivedPlay = Encoding.ASCII.GetString(receiveBytes);

                playParsed = false;
            }
            catch (Exception e)
            {
                Debug.Log("Exception thrown " + e.Message);
            }
        }
    }

    void UDP_RotateSceneReceive()
    {
        clientRotation = new UdpClient(rotateScenePort);
        // loop needed to keep listening
        while (true)
        {
            try
            {
                // recieve messages through the end point
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, rotateScenePort);
                byte[] receiveBytes = clientRotation.Receive(ref remoteEndPoint);
                receivedRotation = Encoding.ASCII.GetString(receiveBytes);

                rotationParsed = false;
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

        string[] splittedMessage = receivedPoint.Split(" ");
        float posX = float.Parse(splittedMessage[0], CultureInfo.InvariantCulture);
        float posY = - float.Parse(splittedMessage[1], CultureInfo.InvariantCulture);
        float posZ = float.Parse(splittedMessage[2], CultureInfo.InvariantCulture);
        Vector3 newPoint = new Vector3(posX, posY, posZ);

        //Vector3 newPoint = new Vector3((float)receivedPointX, (float)receivedPointY, (float)receivedPointZ);

        character.GetComponent<FollowPath>().defineNewPathPoint(newPoint);
    }

    void parsePlayMessage()
    {
        playParsed = true;

        if (receivedPlay == "PLAY")
            DirectorPanelManager.instance.playPath();
        else if (receivedPlay == "STOP")
            DirectorPanelManager.instance.stopPath();
    }

    void parseRotationMessage()
    {
        rotationParsed = true;

        float rotationAngle = float.Parse(receivedRotation);
        UDPSender.instance.rotateItemsInScene(rotationAngle);
    }

    void OnDisable()
    {
        // stop thread when object is disabled
        //if (receiveThread != null){
        //    receiveThread.Abort();
              //client.Close();
        //}

        if (assistantToDirectorThread != null)
        {
            assistantToDirectorThread.Abort();
            clientPath.Close();
        }

        if (receivePlayPathThread != null)
        {
            receivePlayPathThread.Abort();
            clientPlay.Close();
        }

        if (receiveSceneRotationThread != null)
        {
            receiveSceneRotationThread.Abort();
            clientRotation.Close();
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        // Start thread to listen UDP messages and set it as background
        //receiveThread = new Thread(UDP_ReceieveThread);
        //receiveThread.IsBackground = true;
        //receiveThread.Start();

        // director wants to receive new point paths created by the assistant
        if (ModesManager.instance.role == ModesManager.eRoleType.DIRECTOR)
        {
            assistantToDirectorThread = new Thread(UDP_assistantToDirectorReceive);
            assistantToDirectorThread.IsBackground = true;
            assistantToDirectorThread.Start();

            receiveSceneRotationThread = new Thread(UDP_RotateSceneReceive);
            receiveSceneRotationThread.IsBackground = true;
            receiveSceneRotationThread.Start();
        }

        // assistant wants to receive when director plays or stops the path play, since it controlls the position with network manager
        if (ModesManager.instance.role == ModesManager.eRoleType.ASSISTANT)
        {
            receivePlayPathThread = new Thread(UDP_PlayPathReceive);
            receivePlayPathThread.IsBackground = true;
            receivePlayPathThread.Start();
        }

        startPos = ScreenCamera.transform.position;
        //startRot = ScreenCamera.transform.rotation.eulerAngles;
        startRot = ScreenCamera.transform.rotation;

        newItemParsed = true;
        pointParsed = true;
        playParsed = true;
        rotationParsed = true;
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

        if (!newItemParsed)
        {
            ItemsDirectorPanelController.instance.addNewItemButton(newReceivedName);
            newItemParsed = true;
        }
        if (!pointParsed)
            parsePoint();

        if (!playParsed)
            parsePlayMessage();
        if (!rotationParsed)
            parseRotationMessage();
    }
}