using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DirectorPanelManager : MonoBehaviour
{
    public static DirectorPanelManager instance;
    public GameObject multiviewPanel;
    public GameObject aerealviewPanel;
    public GameObject distanceSlider;
    public GameObject distanceText;
    public GameObject PGMView;
    public delegate void PlayPath();
    public delegate void StopPath();
    public event PlayPath OnPlayPath;
    public event StopPath OnStopPath;

    private void Awake()
    {
        if (instance != null && instance != this)
            Destroy(this);
        else
            instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float distance = distanceSlider.GetComponent<Slider>().value;
        distanceText.GetComponent<TextMeshProUGUI>().text = "Distance to screen: " + (int)distance;
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
