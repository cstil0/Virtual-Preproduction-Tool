using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Security.Principal;
using System.Text;

public class PipeReceiver : MonoBehaviour
{
    Thread receiveThread;
    NamedPipeClientStream pipeClient;
    public string recievedMessage;
    public Camera ScreenCamera;

    void recieveThread()
    {
        while (true)
        {
                //Create Client Instance
                NamedPipeClientStream client = new NamedPipeClientStream(".", "MyCOMApp",
                               PipeDirection.InOut, PipeOptions.None,
                               TokenImpersonationLevel.Impersonation);

                //Connect to server
                client.Connect();
                //Created stream for reading and writing
                StreamString clientStream = new StreamString(client);
                //Read from Server
                recievedMessage = clientStream.ReadString();
                Debug.Log("Recieved:" + recievedMessage);
                //Send Message to Server
                clientStream.WriteString("Bye from client");
                //Close client
                client.Close();
        }
    }

    void OnDisable()
    {
        // stop thread when object is disabled
        if (receiveThread != null)
            receiveThread.Abort();
        pipeClient.Close();
    }

    // Start is called before the first frame update
    void Start()
    {
        // Start thread to listen UDP messages and set it as background
        receiveThread = new Thread(recieveThread);
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    // Update is called once per frame
    void Update()
    {
        if (recievedMessage != null)
        {
            string[] splittedMessage = recievedMessage.Split(" ");
            ScreenCamera.transform.position = new Vector3(int.Parse(splittedMessage[0]), int.Parse(splittedMessage[1]), int.Parse(splittedMessage[2]));
            Debug.Log("Vector3: " + ScreenCamera.transform.position);
        }
    }
}
