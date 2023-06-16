using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Unity.VisualScripting;

public class DirectorPanelManager : MonoBehaviour
{
    [Header ("GameObjects")]
    public static DirectorPanelManager instance;
    public GameObject multiviewPanel;
    public GameObject aerealviewPanel;
    public GameObject distanceSlider;
    public GameObject distanceText;
    public GameObject PGMView;
    public GameObject grid;
    public GameObject pointsView;
    public GameObject aerialCameraView;
    [SerializeField] GameObject customRightHand;
    [SerializeField] GameObject customLeftHand;

    [Header ("Icons")]
    public Sprite playIcon;
    public Sprite pauseIcon;
    [SerializeField] Sprite gridIcon;
    [SerializeField] Sprite gridCancelIcon;
    [SerializeField] Sprite pointsViewIcon;
    [SerializeField] Sprite aerialViewIcon;

    [Header ("Buttons")]
    public GameObject playPauseButton;
    public GameObject stopButton;
    [SerializeField] GameObject gridButton;
    [SerializeField] GameObject pointsViewButton;
    [SerializeField] Button[] cameraViewButtons;

    [Header ("States")]
    bool isPlaying = false;
    public bool isGridShown = false;
    bool isPointsViewActive = false;
    bool isXbuttonDown = false;
    [SerializeField] int pathPlayPort = 8052;

    public delegate void PlayPath();
    public event PlayPath OnPlayPath;
    public delegate void StopPath();
    public event StopPath OnStopPath;
    public delegate void HideShowGrid(bool isGridShown);
    public event HideShowGrid OnHideShowGrid;

    private void Awake()
    {
        if (instance != null && instance != this)
            Destroy(this);
        else
            instance = this;
    }

    void Start()
    {
        Color selectedColor = ItemsDirectorPanelController.instance.selectedColor;
        selectedColor.a = 0.15f;

        Button firstInput = cameraViewButtons[1];
        ColorBlock buttonColors = firstInput.colors;
        buttonColors.normalColor = selectedColor;
        firstInput.colors = buttonColors;

        float distance = distanceSlider.GetComponent<Slider>().value;
        distanceText.GetComponent<TextMeshProUGUI>().text = "Distance to screen: " + (int)distance;
    }

    void Update()
    {
        if (OVRInput.Get(OVRInput.RawButton.X))
        {
            if (!isXbuttonDown)
            {
                showHideGrid(false);
                UDPSender.instance.sendShowHideGridDirector(isGridShown);
                isXbuttonDown = true;
            }
        }
        else
            isXbuttonDown = false;
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

    public void changePGMCamera(GameObject input)
    {
        // ressign the camera texture to be rendered at the PGM slot
        string[] inputName = input.name.Split(" ");
        int inputNum = int.Parse(inputName[1]);
        Texture cameraView = input.GetComponent<RawImage>().texture;
        PGMView.GetComponent<RawImage>().texture = cameraView;

        // inform of the change of camera
        UDPSender.instance.changeMainCamera(inputNum);
        Color normalColor = ItemsDirectorPanelController.instance.normalColor;
        normalColor.a = 0f;
        Color selectedColor = ItemsDirectorPanelController.instance.selectedColor;
        selectedColor.a = 0.15f;

        // change texture color to visually tell the one that is selected
        for (int i = 0; i < cameraViewButtons.Length; i ++)
        {
            Button currButton = cameraViewButtons[i];

            ColorBlock buttonColors = currButton.colors;
            if (inputNum == i + 1)
                buttonColors.normalColor = selectedColor;
            else
                buttonColors.normalColor = normalColor;

            currButton.colors = buttonColors;
        }
    }

    public void playPath(bool fromDirector = true)
    {
        isPlaying = !isPlaying;
        if (isPlaying)
            playPauseButton.GetComponent<Image>().sprite = pauseIcon;
        else
            playPauseButton.GetComponent<Image>().sprite = playIcon;

        // call play event
        OnPlayPath();

        // if the message is comming from the panel inform that play was pressed to the assistant
        if (fromDirector)
            SendPlayStop("PLAY");
    }

    public void stopPath()
    {
        // all stop event
        OnStopPath();
        playPauseButton.GetComponent<Image>().sprite = playIcon;
        // inform that stop was pressed
        SendPlayStop("STOP");
    }

    // send play/stop message
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

    public void showHideGrid(bool sendMessage)
    {
        isGridShown = !isGridShown;
        // activate or disable grid image
        if (isGridShown)
            gridButton.GetComponent<Image>().sprite = gridCancelIcon;
        else
            gridButton.GetComponent<Image>().sprite = gridIcon;

        grid.SetActive(isGridShown);

        // inform that grid was shown
        if (sendMessage)
            UDPSender.instance.sendShowHideGridAssistant(isGridShown);

        // call show grid event
        OnHideShowGrid(isGridShown);

        // when grid is shown, objects can be only moved with the GUI arrows, so activate / disable OVR grabbers
        customRightHand.GetComponent<OVRGrabber>().enabled = !isGridShown;
        customLeftHand.GetComponent<OVRGrabber>().enabled = !isGridShown;
        //customRightHand.transform.parent.parent.GetComponent<HoverObjects>().enabled = !isGridShown;
    }

    public void showHidePointsView()
    {
        isPointsViewActive = !isPointsViewActive;

        if (isPointsViewActive)
            pointsViewButton.GetComponent<Image>().sprite = aerialViewIcon;
        else
            pointsViewButton.GetComponent<Image>().sprite = pointsViewIcon;

        pointsView.SetActive(isPointsViewActive);
        aerialCameraView.SetActive(!isPointsViewActive);
    }

    public void changePointsViewTexture(string itemPressedName, int pointNumPressed)
    {
        if (itemPressedName.Contains("MainCamera"))
        {
            // show alert message if no point is
            if (pointNumPressed == -1)
            {
                pointsView.GetComponent<RawImage>().texture = null;
                pointsView.GetComponent<RawImage>().color = new Color(1.0f, 0.4352941f, 0.3686275f);
                pointsView.GetComponentInChildren<TextMeshProUGUI>().enabled = true;
            }

            // get the camera point texture and show it in screen
            else
            {
                Transform currPathCamera = GameObject.Find("Path " + itemPressedName).transform;
                Transform currPoint = currPathCamera.GetChild(pointNumPressed + 1);
                GameObject cameraCanvas = currPoint.GetChild(2).gameObject;
                Texture cameraTexture = cameraCanvas.GetComponentInChildren<RawImage>().texture;

                pointsView.GetComponent<RawImage>().texture = cameraTexture;
                pointsView.GetComponent<RawImage>().color = Color.white;
                pointsView.GetComponentInChildren<TextMeshProUGUI>().enabled = false;
            }
        }

        // show the alert message if no camera is selected
        else
        {
            pointsView.GetComponent<RawImage>().texture = null;
            pointsView.GetComponent<RawImage>().color = new Color(1.0f, 0.4352941f, 0.3686275f);
            pointsView.GetComponentInChildren<TextMeshProUGUI>().enabled = true;
        }
    } 

    public void onScreenDistanceChange(Slider distanceSlider)
    {
        float distance = distanceSlider.GetComponent<Slider>().value;
        distanceText.GetComponent<TextMeshProUGUI>().text = "Distance to screen: " + (int)distance;

        UDPSender.instance.sendChangeScreenDistance(distance);
    }
}
