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

    // Start is called before the first frame update
    void Start()
    {
        Color selectedColor = ItemsDirectorPanelController.instance.selectedColor;
        selectedColor.a = 0.15f;

        Button firstInput = cameraViewButtons[0];
        ColorBlock buttonColors = firstInput.colors;
        buttonColors.normalColor = selectedColor;
        firstInput.colors = buttonColors;
    }

    // Update is called once per frame
    void Update()
    {
        float distance = distanceSlider.GetComponent<Slider>().value;
        distanceText.GetComponent<TextMeshProUGUI>().text = "Distance to screen: " + (int)distance;

        if (OVRInput.Get(OVRInput.RawButton.X))
        {
            if (!isXbuttonDown)
            {
                isGridShown = !isGridShown;
                grid.SetActive(isGridShown);
                UDPSender.instance.sendShowHideGridDirector(isGridShown);
                OnHideShowGrid(isGridShown);
                isXbuttonDown = true;
            }
        }
        else
        {
            isXbuttonDown = false;
        }
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
        string[] inputName = input.name.Split(" ");
        int inputNum = int.Parse(inputName[1]);
        Texture cameraView = input.GetComponent<RawImage>().texture;

        PGMView.GetComponent<RawImage>().texture = cameraView;
        UDPSender.instance.changeMainCamera(inputNum);
        Color normalColor = ItemsDirectorPanelController.instance.normalColor;
        normalColor.a = 0f;
        Color selectedColor = ItemsDirectorPanelController.instance.selectedColor;
        selectedColor.a = 0.15f;

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

    public void showHideGrid()
    {
        isGridShown = !isGridShown;
        if (isGridShown)
            gridButton.GetComponent<Image>().sprite = gridCancelIcon;
        else
            gridButton.GetComponent<Image>().sprite = gridIcon;

        grid.SetActive(isGridShown);
        UDPSender.instance.sendShowHideGridAssistant(isGridShown);

        OnHideShowGrid(isGridShown);

        customRightHand.GetComponent<OVRGrabber>().enabled = isGridShown;
        customLeftHand.GetComponent<OVRGrabber>().enabled = isGridShown;
        customRightHand.transform.parent.parent.GetComponent<HoverObjects>().enabled = isGridShown;
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
        // get the camera point texture and show it in screen
        if (itemPressedName.Contains("MainCamera"))
        {
            if (pointNumPressed == -1)
            {
                pointsView.GetComponent<RawImage>().texture = null;
                pointsView.GetComponent<RawImage>().color = new Color(1.0f, 0.4352941f, 0.3686275f);
                pointsView.GetComponentInChildren<TextMeshProUGUI>().enabled = true;
            }
            else
            {
                Transform currPathCamera = GameObject.Find("Path " + itemPressedName).transform;
                Debug.Log("CHANGING POINT VIEW: " + pointNumPressed);
                Debug.Log("CURRENT POINT PATH: " + currPathCamera.name);
                Transform currPoint = currPathCamera.GetChild(pointNumPressed + 1);
                GameObject cameraCanvas = currPoint.GetChild(2).gameObject;
                Texture cameraTexture = cameraCanvas.GetComponentInChildren<RawImage>().texture;

                pointsView.GetComponent<RawImage>().texture = cameraTexture;
                pointsView.GetComponent<RawImage>().color = Color.white;
                pointsView.GetComponentInChildren<TextMeshProUGUI>().enabled = false;

                //aerealviewPanel.SetActive(false);
                //pointsView.SetActive(true);
            }
        }

        else
        {
            pointsView.GetComponent<RawImage>().texture = null;
            pointsView.GetComponent<RawImage>().color = new Color(1.0f, 0.4352941f, 0.3686275f);
            pointsView.GetComponentInChildren<TextMeshProUGUI>().enabled = true;

            //aerealviewPanel.SetActive(true);
            //pointsView.SetActive(false);
        }
    } 
}
