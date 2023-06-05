using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;
using UnityEngine.EventSystems;
using System;
using System.Net;
using System.Net.Sockets;

public class ModesManager : MonoBehaviour
{
    public static ModesManager instance = null;

    // It is necessary to separate this script from the initial menu one so that when loading the new scene we can still use it without needing to conservate all the canvas
    public eRoleType role = eRoleType.NOT_DEFINED;
    public eModeType mode = eModeType.NOT_DEFINED;

    public TMP_InputField IPAddress;
    public GameObject errorText;

    public GameObject NetworkManager_go;
    public GameObject UIHelpers;
    public GameObject canvas;
    public GameObject leftHand;
    public GameObject rightHand;

    [SerializeField] RenderTexture OVRCameraTexture;

    public enum eRoleType
    {
        NOT_DEFINED,
        DIRECTOR,
        ASSISTANT
    }

    public enum eModeType
    {
        NOT_DEFINED,
        MIXEDREALITY,
        VIRTUALREALITY,
        DEBUG
    }

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);

        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    public void loadMainScene()
    { 
        // check that both role and mode are selected to load the main scene
        if (role != eRoleType.NOT_DEFINED && mode != eModeType.NOT_DEFINED)
        {

            // check if the IP has the required format. If not, show an error message and avoid loading the main scene
            string[] ipArray = IPAddress.text.Split(".");
            // first, check that there are 4 elements separated by dots
            if (ipArray.Length != 4)
            {
                errorText.gameObject.SetActive(true);
                return;
            }

            // if the length of the array is correct, let's check that all of them are integers from 0-255
            for (int i=0; i<ipArray.Length; i++)
            {
                try
                {
                    int ipNum = int.Parse(ipArray[i]);
                    if (ipNum > 0 && ipNum > 255)
                    {
                        errorText.gameObject.SetActive(true);
                        return;
                    }
                }
                catch (Exception e)
                {
                    errorText.gameObject.SetActive(true);
                    return;
                }
            }

            // get the IP corresponding to the host for the multiplayer system
            if (role == eRoleType.DIRECTOR)
                NetworkManager_go.GetComponent<UnityTransport>().ConnectionData.Address = IPAddress.text;
            else if (role == eRoleType.ASSISTANT)
                NetworkManager_go.GetComponent<UnityTransport>().ConnectionData.Address = getLocalIPV4();

            SceneManager.LoadScene("MainScene");
        }
    }

    // get current device's IPV4
    private string getLocalIPV4()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        return "";
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode sceneMode)
    {
        if (scene.name == "MainScene")
        {
            // activate and disable the needed components for each corresponding application
            if (role == eRoleType.DIRECTOR)
            {
                // multi-camera system application acts as the client
                NetworkManager.Singleton.StartClient();

                Camera OVRCamera = GameObject.Find("CenterEyeAnchor").GetComponent<Camera>();
                OVRCamera.targetDisplay = 1;
                GameObject.Find("Panel Camera").GetComponent<Camera>().targetDisplay = 0;
                GameObject.Find("UDP Sender").SetActive(true);
                GameObject.Find("NDI Receiver").SetActive(true);
                GameObject.Find("UDP Receiver").SetActive(true);

                // enable the event system from the multi-camera canvas to enable PC events
                GameObject eventSytem = GameObject.Find("EventSystem");
                GameObject directorCanvas = GameObject.FindGameObjectWithTag("DirectorPanel");
                directorCanvas.GetComponent<EventSystem>().enabled = true;
            }
            else if (role == eRoleType.ASSISTANT)
            {
                // VR application acts as the host
                NetworkManager.Singleton.StartHost();

                GameObject.Find("CenterEyeAnchor").GetComponent<Camera>().targetDisplay = 0;
                GameObject.Find("Panel Camera").GetComponent<Camera>().targetDisplay = 1;
                GameObject.Find("UDP Receiver").SetActive(true);

                GameObject UDPSender = GameObject.Find("UDP Sender");
                UDPSender.SetActive(true);
                // if we are in assistant mode, the ip of the screen camera corresponds to the one the user inputs
                UDPSender.GetComponent<UDPSender>().ipAddress = IPAddress.text;

                GameObject.Find("NDI Receiver").SetActive(true);

                // disable event system from multi-camera canvas, as we will only need those corresponding to VR
                GameObject eventSytem = GameObject.Find("EventSystem");
                GameObject directorCanvas = GameObject.FindGameObjectWithTag("DirectorPanel");
                directorCanvas.GetComponent<EventSystem>().enabled = false;
            }

            // deactivate all structural elements of the set when using mixed reality mode
            if (mode == eModeType.MIXEDREALITY)
            {
                GameObject.Find("Walls").SetActive(false);
                // we cannot deactivate the floor, since then the OVR player falls down, but we can deactivate its renderer
                GameObject.Find("Plane").GetComponent<MeshRenderer>().enabled = false;
                GameObject.Find("Big Screen").SetActive(false);
                GameObject.Find("OVRCameraRig").GetComponent<OVRManager>().isInsightPassthroughEnabled = true;
                GameObject.Find("OVRCameraRig").GetComponent<OVRPassthroughLayer>().enabled = true;

                // Desactivate thumbstick movement
                OVRPlayerController playerController = GameObject.Find("OVRPlayerController").GetComponent<OVRPlayerController>();
                playerController.EnableLinearMovement = false;
                playerController.EnableRotation = false;
            }
            // ensure that all structural elements of the set are shown
            else if (mode == eModeType.VIRTUALREALITY)
            {
                GameObject.Find("Walls").SetActive(true);
                GameObject.Find("Plane").GetComponent<MeshRenderer>().enabled = true;
                GameObject.Find("Big Screen").SetActive(true);
                GameObject.Find("OVRCameraRig").GetComponent<OVRManager>().isInsightPassthroughEnabled = false;
                GameObject.Find("OVRPlayerController").GetComponent<OVRPassthroughLayer>().enabled = false;

                // Activate thumbstick movement
                OVRPlayerController playerController = GameObject.Find("OVRPlayerController").GetComponent<OVRPlayerController>();
                playerController.EnableLinearMovement = true;
                playerController.EnableRotation = false;    
            }
        }
    }

    void Start()
    {
        // by default, if app is being run in a PC, enable the mouse input and disable OVR input
        if (Application.platform == RuntimePlatform.WindowsPlayer || Application.isEditor)
        {
            role = eRoleType.DIRECTOR;

            canvas.GetComponent<EventSystem>().enabled = true;
            UIHelpers.SetActive(false);
            leftHand.SetActive(false);
            rightHand.SetActive(false);

            IPAddress.text = "Set Headset IP";
        }
        else
        {
            role = eRoleType.ASSISTANT;

            canvas.GetComponent<EventSystem>().enabled = false;
            UIHelpers.SetActive(true);
            leftHand.SetActive(true);
            rightHand.SetActive(true);

            IPAddress.text = "Set Director's PC IP";
        }

        errorText.SetActive(false);
    }


    void Update()
    {
        // pressing D key is used for debug purposes by using Oculus Link, so we need to enable OVR input again
        if (Input.GetKeyDown(KeyCode.D))
        {
            role = eRoleType.ASSISTANT;
            mode = eModeType.VIRTUALREALITY;

            canvas.GetComponent<EventSystem>().enabled = false;
            UIHelpers.SetActive(true);
            leftHand.SetActive(true);
            rightHand.SetActive(true);

            IPAddress.text = "192.168.0.12";

            loadMainScene();
        }

        // pending to implement creating all items and points references and change buttons names when client reconnects to host
        if (Input.GetKeyDown(KeyCode.R))
        {

        }
    }
}
