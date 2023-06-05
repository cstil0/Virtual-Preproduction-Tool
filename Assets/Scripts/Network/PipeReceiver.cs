using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Pipes;
using System.Threading;
using System.Security.Principal;
using System.Globalization;

// pipes library was used previously to send messages from the multi-camera application to the LED screen one, but it blocks the application when executing it if both projects are not running
// therefore, currently both applications communicate sending the message first to the VR one and then re-sending it to the corresponding one
public class PipeReceiver : MonoBehaviour
{
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
            // create Client Instance
            NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", "MyCOMApp",
                            PipeDirection.InOut, PipeOptions.None,
                            TokenImpersonationLevel.Impersonation);

            // connect to server
            pipeClient.Connect();
            // created stream for reading and writing
            StreamString clientStream = new StreamString(pipeClient);
            // read from Server
            recievedMessage = clientStream.ReadString();
            string[] splittedMessage = recievedMessage.Split(" ");
            currPos = new Vector3(float.Parse(splittedMessage[0], CultureInfo.InvariantCulture), float.Parse(splittedMessage[1], CultureInfo.InvariantCulture), float.Parse(splittedMessage[2], CultureInfo.InvariantCulture));
            
            if (remoteStartPos != null) {
                remoteStartPos = currPos;
            }

            Debug.Log("Recieved:" + recievedMessage);
            // close client
            pipeClient.Close();
        }
    }

    void OnDisable()
    {
        // stop thread when object is disabled
        if (receiveThread != null)
            receiveThread.Abort();

        // close client
        pipeClient.Close();
    }

    void Start()
    {
        // start thread to listen to pipe messages and set it as background
        receiveThread = new Thread(recieveThread);
        receiveThread.IsBackground = true;
        receiveThread.Start();

        startPos = ScreenCamera.transform.position;
    }

    void Update()
    {
        if (recievedMessage != null)
        {
            Vector3 remotePosDiff = currPos - remoteStartPos;
            ScreenCamera.transform.position = remotePosDiff + startPos;
        }
    }
}
