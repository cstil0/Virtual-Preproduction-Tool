using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SearchService;

public class ModesManager : MonoBehaviour
{
    // It is necessary to separe this script from the initial menu one so that when loading the new scene we can still use it without needing to conserve all the canvas

    public eRoleType role = eRoleType.NOT_DEFINED;
    public eModeType mode = eModeType.NOT_DEFINED;


    public GameObject NetworkManager_go;

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
        if (role != eRoleType.NOT_DEFINED && mode != eModeType.NOT_DEFINED)
        {
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
                NetworkManager.Singleton.StartClient();
            else if (role == eRoleType.ASSISTANT)
                NetworkManager.Singleton.StartHost();

            if (mode == eModeType.MIXEDREALITY)
            {
                GameObject.Find("Walls").SetActive(false);
                GameObject.Find("Plane").SetActive(false);
                GameObject.Find("OVRCameraRig").GetComponent<OVRManager>().isInsightPassthroughEnabled = true;
                NetworkManager_go.GetComponent<UnityTransport>().ConnectionData.Address = "192.168.0.12";
                GameObject.Find("OVRCameraRig").GetComponent<OVRPassthroughLayer>().gameObject.SetActive(true);
            }
            else if (mode == eModeType.VIRTUALREALITY)
            {
                GameObject.Find("Walls").SetActive(true);
                GameObject.Find("Plane").SetActive(true);
                NetworkManager_go.GetComponent<UnityTransport>().ConnectionData.Address = "192.168.0.12";
                GameObject.Find("OVRCameraRig").GetComponent<OVRManager>().isInsightPassthroughEnabled = false;
                GameObject.Find("OVRPlayerController").GetComponent<OVRPassthroughLayer>().gameObject.SetActive(false);
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
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
