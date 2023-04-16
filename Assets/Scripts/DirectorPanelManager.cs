using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;
using System.Net;
using System.Text;

public class DirectorPanelManager : MonoBehaviour
{
    public static DirectorPanelManager instance;
    public GameObject multiviewPanel;
    public GameObject aerealviewPanel;
    public GameObject distanceSlider;
    public GameObject distanceText;
    public GameObject PGMView;
    public delegate void PlayPath();
    public event PlayPath OnPlayPath;
    public delegate void StopPath();
    public event StopPath OnStopPath;

    public Sprite playIcon;
    public Sprite pauseIcon;
    public GameObject playPauseButton;
    public GameObject stopButton;

    bool isPlaying = false;
    [SerializeField] int pathPlayPort = 8052;

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
        isPlaying = !isPlaying;
        if (isPlaying)
            playPauseButton.GetComponent<Image>().sprite = pauseIcon;
        else
            playPauseButton.GetComponent<Image>().sprite = playIcon;

        //playPauseButton.GetComponent<Button>().colors;
        OnPlayPath();
        SendPlayStop("PLAY");
    }

    public void stopPath()
    {
        OnStopPath();
        playPauseButton.GetComponent<Image>().sprite = playIcon;
        SendPlayStop("STOP");
    }

    public void SendPlayStop(string playMessage)
    {
        try
        {
            UdpClient client = new UdpClient(pathPlayPort);

            string ipAddress = ModesManager.instance.IPAddress.text;

            // sending data
            IPEndPoint target = new IPEndPoint(IPAddress.Parse(ipAddress), pathPlayPort);

            byte[] message = Encoding.ASCII.GetBytes("PLAY_PATH:" + playMessage);
            client.Send(message, message.Length, target);

            client.Close();
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

}
