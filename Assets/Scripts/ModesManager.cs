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

public class ModesManager : MonoBehaviour
{
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
                
            NetworkManager_go.GetComponent<UnityTransport>().ConnectionData.Address = IPAddress.text;
            SceneManager.LoadScene("MainScene");
        }
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
                GameObject.Find("OVRPlayerController").SetActive(false);
                GameObject.Find("Panel Camera").GetComponent<Camera>().targetDisplay = 0;
            }
            else if (role == eRoleType.ASSISTANT)
            {
                NetworkManager.Singleton.StartHost();
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
            }
            else if (mode == eModeType.VIRTUALREALITY)
            {
                GameObject.Find("Walls").SetActive(true);
                GameObject.Find("Plane").GetComponent<MeshRenderer>().enabled = true;
                GameObject.Find("Big Screen").SetActive(true);
                GameObject.Find("OVRCameraRig").GetComponent<OVRManager>().isInsightPassthroughEnabled = false;
                GameObject.Find("OVRPlayerController").GetComponent<OVRPassthroughLayer>().enabled = false;
            }
        }
    }

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        // by default, if app is being run in PC, it enables the mouse input and disable OVR input
        if (Application.platform == RuntimePlatform.WindowsPlayer || Application.isEditor)
        {
            canvas.GetComponent<EventSystem>().enabled = true;
            UIHelpers.SetActive(false);
            leftHand.SetActive(false);
            rightHand.SetActive(false);
        }
        else
        {
            canvas.GetComponent<EventSystem>().enabled = false;
            UIHelpers.SetActive(true);
            leftHand.SetActive(true);
            rightHand.SetActive(true);
        }

        errorText.SetActive(false);
    }


    // Update is called once per frame
    void Update()
    {
        // pressing D key is used for debug purposes to use Oculus Link and enable OVR input again
        if (Input.GetKeyDown(KeyCode.D))
        {
            canvas.GetComponent<EventSystem>().enabled = false;
            UIHelpers.SetActive(true);
        }
    }
}
