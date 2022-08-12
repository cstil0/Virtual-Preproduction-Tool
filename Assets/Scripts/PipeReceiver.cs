using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;

public class PipeReceiver : MonoBehaviour
{
    Thread receiveThread;
    NamedPipeClientStream pipeClient;
    void recieveThread()
    {
        while (true)
        {
            using (pipeClient = new NamedPipeClientStream(".", "testpipe", PipeDirection.In))
            {
                // Connect to the pipe or wait until the pipe is available.
                Console.Write("Attempting to connect to pipe...");
                pipeClient.Connect();

                Console.WriteLine("Connected to pipe.");
                Console.WriteLine("There are currently {0} pipe server instances open.",
                   pipeClient.NumberOfServerInstances);
                using (StreamReader sr = new StreamReader(pipeClient))
                {
                    // Display the read text to the console
                    string temp;
                    while ((temp = sr.ReadLine()) != null)
                    {
                        Console.WriteLine("Received from server: {0}", temp);
                    }
                }
            }
            Console.Write("Press Enter to continue...");
            Console.ReadLine();
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
        
    }
}
