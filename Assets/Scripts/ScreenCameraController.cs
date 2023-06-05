using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;



public class ScreenCameraController : MonoBehaviour
{
    public Camera screenCamera;
    public Camera mainCamera;
    public Vector3 startPosition;

    void Start()
    {
        gameObject.transform.position = startPosition;
    }

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
