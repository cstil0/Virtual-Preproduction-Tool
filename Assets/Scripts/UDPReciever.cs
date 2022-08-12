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

public class UDPReceiver : MonoBehaviour
{
    // udpclient object
    UdpClient client;
    public int serverPort;
    Thread receiveThread;
    String receivedMessage;

    // main thread that listens to UDP messages through a defined port
    void UDPTest()
    {
        // create client and set the port (HARCODEADO EN EL EDITOR!!-----)
        UdpClient client = new UdpClient(serverPort);
        // loop needed to keep listening
        while (true)
        {
            try
            {
                // recieve messages through the end point
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, serverPort);
                byte[] receiveBytes = client.Receive(ref remoteEndPoint);
                // once the message is recieved, encode it as ASCII
                receivedMessage = Encoding.ASCII.GetString(receiveBytes);
                Debug.Log("Recieved message: " + receivedMessage);
            }
            catch (Exception e)
            {
                print("Exception thrown " + e.Message);
            }
        }
    }
    void OnDisable()
    {
        // stop thread when object is disabled
        if (receiveThread != null)
            receiveThread.Abort();
        client.Close();
    }
    // Start is called before the first frame update
    void Start()
    {
        // Start thread to listen UDP messages and set it as background
        receiveThread = new Thread(UDPTest);
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }
    // Update is called once per frame
    void Update()
    {

    }
}