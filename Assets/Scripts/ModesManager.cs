using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SearchService;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.InputSystem.UI;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Net;
using System.Net.Sockets;

public class ModesManager : MonoBehaviour
{
    public static ModesManager instance = null;

    // It is necessary to separe this script from the initial menu one so that when loading the new scene we can still use it without needing to conserve all the canvas

    public eRoleType role = eRoleType.NOT_DEFINED;
    public eModeType mode = eModeType.NOT_DEFINED;

    public TMP_InputField IPAddress;
    public GameObject errorText;

    public GameObject NetworkManager_go;
    public GameObject UIHelpers;
    public GameObject canvas;
    public GameObject leftHand;
    public GameObject rightHand;

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
        // check that both role and mode are selected
        if (role != eRoleType.NOT_DEFINED && mode != eModeType.NOT_DEFINED)
        {

            string[] ipArray = IPAddress.text.Split(".");
            if (ipArray.Length != 4)
            {
                errorText.gameObject.SetActive(true);
                return;
            }

            // if the length of the array is correct, let's check that all of them are integers from 0-255
            // PENDENT -- PER ALGUN MOTIU L'ULTIM CARACTER NO EL RECONEIX COM A INTEGER, I EN ALGUN LLOC HE VIST QUE S'AFEGEIX ALGUN CARACTER AMAGAT
            // DE MOMENT LLEGEIXO FINS EL PENÚLTIM, NO ÉS IMPORTANT
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

            if (role == eRoleType.DIRECTOR)
            {
                NetworkManager_go.GetComponent<UnityTransport>().ConnectionData.Address = IPAddress.text;
            }
            else if (role == eRoleType.ASSISTANT)
            {
                NetworkManager_go.GetComponent<UnityTransport>().ConnectionData.Address = getLocalIPV4();
            }

            SceneManager.LoadScene("MainScene");
        }
    }

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
        Debug.Log("OnSceneLoaded: " + scene.name);
        Debug.Log(mode);

        if (scene.name == "MainScene")
        {
            if (role == eRoleType.DIRECTOR)
            {
                NetworkManager.Singleton.StartClient();
                //Display.displays[1].Activate();
                // TOT AIXÒ SERIA MÉS MACO GESTIONAR-HO AMB EVENTS!! ARA QUE SÉ COM FUNCIONEN:)
                // O BUENO NO SÉ SI SERÀ POSSIBLE JA QUE NO SÓN SCRIPTS MEUS I PER TANT HO HAURIA DE POSAR EN ALGUN ALTRE DINS EL MATEIX OBJECTE UNA MICA AMB COLA POTSER
                GameObject.Find("CenterEyeAnchor").GetComponent<Camera>().targetDisplay = 1;
                GameObject.Find("Panel Camera").GetComponent<Camera>().targetDisplay = 0;
                GameObject.Find("UDP Sender").SetActive(false);
                GameObject.Find("NDI Receiver").SetActive(true);
                GameObject.Find("UDP Receiver").SetActive(true);

                GameObject eventSytem = GameObject.Find("EventSystem");
                //eventSytem.GetComponent<EventSystem>().enabled = false;
                GameObject directorCanvas = GameObject.FindGameObjectWithTag("DirectorPanel");
                directorCanvas.GetComponent<EventSystem>().enabled = true;
            }
            else if (role == eRoleType.ASSISTANT)
            {
                NetworkManager.Singleton.StartHost();

                GameObject.Find("CenterEyeAnchor").GetComponent<Camera>().targetDisplay = 0;
                GameObject.Find("Panel Camera").GetComponent<Camera>().targetDisplay = 1;
                GameObject.Find("UDP Receiver").SetActive(true);

                // ip address to send path points to director
                //DrawLine.instance.ipAddress = IPAddress.text;

                GameObject UDPSender = GameObject.Find("UDP Sender");
                UDPSender.SetActive(true);
                // if we are in assistant mode, the ip of the screen camera corresponds to the one the user inputs
                UDPSender.GetComponent<UDPSender>().ipAddress = IPAddress.text;

                GameObject.Find("NDI Receiver").SetActive(true);

                GameObject eventSytem = GameObject.Find("EventSystem");
                //eventSytem.GetComponent<EventSystem>().enabled = true;
                GameObject directorCanvas = GameObject.FindGameObjectWithTag("DirectorPanel");
                directorCanvas.GetComponent<EventSystem>().enabled = false;


                //RotationScale rotationScale = HarryPrefab.GetComponentInChildren<RotationScale>();

                //GameObject objectInstance = Instantiate(HarryPrefab);
                //objectInstance.transform.position = new Vector3(0.0f, 0.0f, -10f);
                //objectInstance.GetComponent<NetworkObject>().Spawn();

                //Display.displays[0].Activate();
            }

            if (mode == eModeType.MIXEDREALITY)
            {
                GameObject.Find("Walls").SetActive(false);
                // we cannot desactivate the floor, since then the OVR player falls down
                GameObject.Find("Plane").GetComponent<MeshRenderer>().enabled = false;
                GameObject.Find("Big Screen").SetActive(false);
                GameObject.Find("OVRCameraRig").GetComponent<OVRManager>().isInsightPassthroughEnabled = true;
                GameObject.Find("OVRCameraRig").GetComponent<OVRPassthroughLayer>().enabled = true;

                // Desactivate thumbstick movement
                OVRPlayerController playerController = GameObject.Find("OVRPlayerController").GetComponent<OVRPlayerController>();
                playerController.EnableLinearMovement = false;
                playerController.EnableRotation = false;
            }
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
                playerController.EnableRotation = true;
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // by default, if app is being run in PC, it enables the mouse input and disable OVR input
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


    // Update is called once per frame
    void Update()
    {
        // pressing D key is used for debug purposes to use Oculus Link and enable OVR input again
        if (Input.GetKeyDown(KeyCode.D))
        {
            role = eRoleType.ASSISTANT;

            canvas.GetComponent<EventSystem>().enabled = false;
            UIHelpers.SetActive(true);

            IPAddress.text = "Set Screen Scene IP";
        }
    }
}
