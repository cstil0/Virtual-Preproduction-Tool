using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DirectorPanelManager : MonoBehaviour
{
    public GameObject multiviewPanel;
    public GameObject aerealviewPanel;
    public GameObject PGMView;
    public delegate void PlayPath();
    public delegate void StopPath();
    public event PlayPath OnPlayPath;
    public event StopPath OnStopPath;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void goToAerialView()
    {
        multiviewPanel.SetActive(false);
        aerealviewPanel.SetActive(true);
    }
    public void goToMainPView()
    {
        multiviewPanel.SetActive(true);
        aerealviewPanel.SetActive(false);
    }

    public void changePGMCamera(Material cameraView)
    {
        PGMView.GetComponent<Image>().material = cameraView;
    } 

    public void playPath()
    {
        OnPlayPath();
    }

    public void stopPath()
    {
        OnStopPath();
    }
}
