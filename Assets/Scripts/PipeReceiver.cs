using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Security.Principal;
using System.Text;
using UnityEditor.PackageManager;
using System.Globalization;

public class PipeReceiver : MonoBehaviour
{
    // HAY UN LIO CON EL NOMBRE DE ESTA VARIABLE Y EL DEL THREAD EN SI!! CAMBIAR!!!
    Thread receiveThread;
    NamedPipeClientStream pipeClient;
    string recievedMessage;
    public Camera ScreenCamera;
    Vector3 startPos;
    Vector3 remoteStartPos;
    Vector3 currPos;

    void recieveThread()
    {
        while (true)
        {
            //Create Client Instance
            NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", "MyCOMApp",
                            PipeDirection.InOut, PipeOptions.None,
                            TokenImpersonationLevel.Impersonation);

            //Connect to server
            pipeClient.Connect();
            //Created stream for reading and writing
            StreamString clientStream = new StreamString(pipeClient);
            //Read from Server
            recievedMessage = clientStream.ReadString();
            string[] splittedMessage = recievedMessage.Split(" ");
            currPos = new Vector3(float.Parse(splittedMessage[0], CultureInfo.InvariantCulture), float.Parse(splittedMessage[1], CultureInfo.InvariantCulture), float.Parse(splittedMessage[2], CultureInfo.InvariantCulture));
            
            if (remoteStartPos != null) {
                remoteStartPos = currPos;
            }

            Debug.Log("Recieved:" + recievedMessage);
            //Close client
            pipeClient.Close();
        }
    }

    void OnDisable()
    {
        // stop thread when object is disabled
        if (receiveThread != null)
            receiveThread.Abort();

        //Close client
        pipeClient.Close();
    }

    // Start is called before the first frame update
    void Start()
    {
        // Start thread to listen UDP messages and set it as background
        receiveThread = new Thread(recieveThread);
        receiveThread.IsBackground = true;
        receiveThread.Start();

        startPos = ScreenCamera.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (recievedMessage != null)
        {
            Vector3 remotePosDiff = currPos - remoteStartPos;
            ScreenCamera.transform.position = remotePosDiff + startPos;
        }
        //if (!receiveThread.IsAlive)
        //{
        //    receiveThread.Start();
        //}
    }
}
