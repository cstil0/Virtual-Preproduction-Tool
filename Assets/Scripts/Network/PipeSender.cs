using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Pipes;
using System;
using System.IO;
using System.Text;
using System.Globalization;
using UnityEngine.UI;
using System.Linq.Expressions;

public class PipeSender : MonoBehaviour
{
    // udpclient object
    public Camera screenCamera;
    //UdpClient client;
    public int serverPort;
    public string ipAddress;

    bool resetStart;
    int buttonDown;
    bool positionChanged;

    Vector3 lastPos;
    public float sceneRotation;

    public GameObject OVRPlayer;


    public void sendDist(GameObject distSlider)
    {
        using (NamedPipeServerStream pipeServer = new NamedPipeServerStream("VPT", PipeDirection.Out))
        {
            pipeServer.WaitForConnection();
            using (StreamWriter sw = new StreamWriter(pipeServer))
            {
                sw.AutoFlush = true;
                sw.WriteLine("DIST:" + distSlider.GetComponent<Slider>().value.ToString());
                pipeServer.Close();
            }
        }
    }
    // main thread that listens to UDP messages through a defined port
    //void SendPosRot()
    //{
    //    NamedPipeServerStream server = new NamedPipeServerStream("MyCOMApp", PipeDirection.InOut, 1);

    //    //client.Connect(IPAddress.Parse(ipAddress), serverPort);

    //    //IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), serverPort);

    //    // sending data
    //    server.WaitForConnection();

    //    //Created stream for reading and writing
    //    StreamString serverStream = new StreamString(server);

    //    string message = resetStart.ToString();
    //    serverStream.WriteString(message);

    //    Vector3 cameraPos = screenCamera.transform.position;
    //    string specifier = "G";
    //    //byte[] message = Encoding.ASCII.GetBytes(cameraPos.ToString(specifier, CultureInfo.InvariantCulture) + " " + cameraPos.y.ToString(specifier, CultureInfo.InvariantCulture) + " " + cameraPos.z.ToString(specifier, CultureInfo.InvariantCulture));
    //    message = cameraPos.ToString(specifier, CultureInfo.InvariantCulture);
    //    serverStream.WriteString(message);

    //    //Vector3 cameraRot = screenCamera.transform.rotation.eulerAngles;
    //    Quaternion cameraRot = screenCamera.transform.rotation;
    //    //message = Encoding.ASCII.GetBytes(cameraRot.x.ToString(specifier, CultureInfo.InvariantCulture) + " " + cameraRot.y.ToString(specifier, CultureInfo.InvariantCulture) + " " + cameraRot.z.ToString(specifier, CultureInfo.InvariantCulture));
    //    message = cameraRot.ToString(specifier, CultureInfo.InvariantCulture);
    //    serverStream.WriteString(message);

    //    server.Close();
    //}

    //void sendSceneRotation()
    //{
    //    NamedPipeServerStream server = new NamedPipeServerStream("MyCOMApp", PipeDirection.InOut, 1);

    //    server.WaitForConnection();
    //    StreamString serverStream = new StreamString(server);

    //    Vector3 cameraRotation = screenCamera.transform.rotation.eulerAngles;
    //    Quaternion currentRotation = Quaternion.Euler(0.0f, cameraRotation.y + sceneRotation, 0.0f);
    //    string message = "SCENE_ROTATION:" + currentRotation.ToString();
    //    serverStream.WriteString(message);
    //    server.Close();
    //}

    //void sendCameraType()
    //{
        //NamedPipeServerStream server = new NamedPipeServerStream("MyCOMApp", PipeDirection.InOut, 1);

        //ModesManager modesManager = GameObject.Find("Modes Manager").GetComponent<ModesManager>();
        //if (modesManager.role == ModesManager.eRoleType.ASSISTANT)
        //{
        //    server.WaitForConnection();
        //    StreamString serverStream = new StreamString(server);


        //    if (modesManager.mode == ModesManager.eModeType.MIXEDREALITY)
        //    {
        //        string message = "SEND_DISPLAY";
        //        serverStream.WriteString(message);
        //    }
        //    else if (modesManager.mode == ModesManager.eModeType.VIRTUALREALITY)
        //    {
        //        string message = "SEND_NDI";
        //        serverStream.WriteString(message);
        //    }

        //}
        //server.Close();
    //}

    //IEnumerator sendInitialParameters()
    //{
        //sendCameraType();
        //yield return new WaitForSeconds(5);
        //SendPosRot();
    //}

    // Start is called before the first frame update
    void Start()
    {
        //gameObject.SetActive(true);
        //positionChanged = true;
        //resetStart = true;
        //lastPos = screenCamera.transform.position;
        //sceneRotation = 10;
        //buttonDown = 0;
        //StartCoroutine(sendInitialParameters());
    }

    // Update is called once per frame
    void Update()
    {

        //if (Input.GetKeyDown(KeyCode.L))
        //{
        //    using (NamedPipeServerStream pipeServer = new NamedPipeServerStream("VPT", PipeDirection.Out))
        //    {
        //        pipeServer.WaitForConnection();
        //        using (StreamWriter sw = new StreamWriter(pipeServer))
        //        {
        //            sw.AutoFlush = true;
        //            float message = 10.0f;
        //            sw.WriteLine("DIST:" + message.ToString());
        //            pipeServer.Close();
        //        }
        //    }
        //}
        //int rotation = 0;
        //Vector3 currentPos = screenCamera.transform.position;
        //if (lastPos != currentPos)
        //{
        //    SendPosRot();

        //    if (!positionChanged)
        //        resetStart = true;
        //    else
        //        resetStart = false;

        //    positionChanged = true;
        //    lastPos = currentPos;
        //}

        //else if (positionChanged)
        //{
        //    resetStart = false;
        //    positionChanged = false;
        //}

        //if (OVRInput.Get(OVRInput.Button.Four))
        //{
        //    //if (!buttonDown)
        //    //{
        //    sceneRotation = sceneRotation + 5;
        //    if (sceneRotation >= 360)
        //        sceneRotation = sceneRotation - 360;

        //    sendSceneRotation();

        //    buttonDown = 1;
        //    rotation = 5;

        //    //GameObject[] sceneItems = GameObject.FindGameObjectsWithTag("Items");

        //    //foreach (GameObject item in sceneItems)
        //    //{
        //    //    Vector3 itemPos = item.transform.position;
        //    //    Vector3 itemRot = item.transform.rotation.eulerAngles;

        //    //    item.transform.position = new Vector3(OVRPlayer.transform.position.x, itemPos.y, OVRPlayer.transform.position.z);
        //    //    item.transform.rotation = Quaternion.Euler(new Vector3(0.0f, rotation + itemRot.y , 0.0f));
        //    //    item.transform.position = item.transform.forward * itemPos.z;
        //    //    item.transform.position = item.transform.right * itemPos.x;
        //    //    //item.transform.RotateAround(OVRPlayer.transform.position, Vector3.up, 5*Time.deltaTime);
        //    //}

        //    //    buttonDown = true;
        //    //}
        //}
        //else
        //{
        //    buttonDown = 0;
        //}

    }
}
