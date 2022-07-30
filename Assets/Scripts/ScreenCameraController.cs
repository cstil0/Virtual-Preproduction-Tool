using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;



public class ScreenCameraController : MonoBehaviour
{
    public Camera screenCamera;
    public Camera mainCamera;
    public Vector3 startPosition;

    // Start is called before the first frame update
    void Start()
    {
        //screenCamera.aspect = 2/1;
        gameObject.transform.position = startPosition;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        mainCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
    }
}
