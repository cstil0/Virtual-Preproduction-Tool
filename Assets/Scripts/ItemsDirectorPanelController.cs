using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ItemsDirectorPanelController : MonoBehaviour
{
    public static ItemsDirectorPanelController instance = null;

    [SerializeField] TMP_Text itemName;
    [SerializeField] Button closeButton;
    [SerializeField] Button removeButton;
    [SerializeField] Button speedMinusButton;
    [SerializeField] Button speedPlusButton;
    [SerializeField] TMP_InputField speedInput;
    [SerializeField] Button trashButton;

    [SerializeField] GameObject panelLayout;
    [SerializeField] GameObject itemsOptionsPanel;
    [SerializeField] GameObject pointsPanel;

    [SerializeField] GameObject itemButtonPrefab;
    [SerializeField] GameObject pointButtonPrefab;
    [SerializeField] GameObject pointsLayoutPrefab;

    private int currPointPressed;
    private string currItemPressed;
    private GameObject currItemGO;
    private FollowPath currFollowPath;
    private FollowPathCamera currFollowPathCamera;

    [SerializeField] Color selectedColor;
    [SerializeField] Color normalColor;

    private void Awake()
    {
        if (instance)
        {
            if (instance != this)
                Destroy(gameObject);
        }
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
        
    }

    public void onItemsButtonPressed(GameObject itemButtonGO)
    {
        TMP_Text buttonText = itemButtonGO.transform.GetChild(0).GetComponent<TMP_Text>();
        Button itemButton = itemButtonGO.GetComponent<Button>();

        bool isAlreadySelected = false;
        if (currItemPressed == buttonText.text)
            isAlreadySelected = true;
        else
            currItemPressed = buttonText.text;

        itemName.text = buttonText.text;
        if (buttonText.transform.parent.name.Contains("Camera"))
            removeButton.gameObject.SetActive(false);
        else
            // if it is already selected, we want to hide the remove button. Otherwise, show it
            removeButton.gameObject.SetActive(!isAlreadySelected);

        itemsOptionsPanel.SetActive(!isAlreadySelected);

        for (int i = 0; i < pointsPanel.transform.childCount; i++)
        {
            GameObject currLayout = pointsPanel.transform.GetChild(i).gameObject;
            if (currLayout.name.Contains(buttonText.text))
                currLayout.SetActive(!isAlreadySelected);
            else
                currLayout.SetActive(false);
        }

        if (!isAlreadySelected)
        {
            // get speed value
            currItemGO = GameObject.Find(currItemPressed);
            currItemGO.TryGetComponent<FollowPath>(out currFollowPath);
            currItemGO.TryGetComponent<FollowPathCamera>(out currFollowPathCamera);

            if (currFollowPath != null)
                speedInput.text = currFollowPath.posSpeed.ToString();

            if (currFollowPathCamera != null)
                speedInput.text = currFollowPathCamera.speed.ToString();

            // change color to visualize it as selected
            ColorBlock buttonColors = itemButton.GetComponent<Button>().colors;
            buttonColors.normalColor = normalColor;
            itemButton.GetComponent<Button>().colors = buttonColors;
        }
        else
        {

        }
    }

    public void onCloseButtonPressed()
    {
        itemsOptionsPanel.SetActive(false);
        currItemGO = null;
        currItemPressed = null;
        currPointPressed = -1;
    }

    public void onSpeedPlusPressed()
    {
        speedInput.text = (int.Parse(speedInput.text) + 1).ToString();
        onSpeedChange(int.Parse(speedInput.text));
    }

    public void onSpeedMinusPressed()
    {
        speedInput.text = (int.Parse(speedInput.text) - 1).ToString();
        onSpeedChange(int.Parse(speedInput.text));
    }

    public void onSpeedInput()
    {
        onSpeedChange(int.Parse(speedInput.text));
    }

    public void onSpeedChange(int speed)
    {
        UDPSender.instance.sendChangeSpeed(speed, currItemPressed);

        if (currFollowPath != null)
            currFollowPath.posSpeed = speed;

        if (currFollowPathCamera != null)
            currFollowPathCamera.speed = speed;
    }

    public void onTrashPressed()
    {
        UDPSender.instance.sendDeletePoint(currPointPressed, currItemPressed);

        currItemGO.TryGetComponent<FollowPath>(out FollowPath followPath);
        currItemGO.TryGetComponent<FollowPathCamera>(out FollowPathCamera followPathCamera);

        if (followPath != null)
            followPath.deletePathPoint(currPointPressed);

        if (followPathCamera != null)
            followPathCamera.deletePathPoint(currPointPressed);

        currPointPressed = -1;
    }

    public void onPointPressed(GameObject pointButton)
    {
        string[] splittedName = pointButton.name.Split(" ");
        int pointNum = int.Parse(splittedName[1]);

        if (pointNum == currPointPressed)
        {
            currPointPressed = -1;
            ColorBlock buttonColors = pointButton.GetComponent<Button>().colors;
            buttonColors.normalColor = normalColor;
            pointButton.GetComponent<Button>().colors = buttonColors;
        }
        else
        {
            currPointPressed = pointNum;
            ColorBlock buttonColors = pointButton.GetComponent<Button>().colors;
            buttonColors.selectedColor = selectedColor;
            pointButton.GetComponent<Button>().colors = buttonColors;
        }
    }

    public void onRemoveItemPressed()
    {
        if (!currItemPressed.Contains("Camera"))
        {
            UDPSender.instance.sendDeleteItem(currItemPressed);
            Destroy(currItemGO);
        }
    }

    public void addNewItemButton(string name)
    {
        GameObject newButton = Instantiate(itemButtonPrefab);
        newButton.transform.parent = panelLayout.transform;
        newButton.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0.0f, 0.0f, 0.0f);
        newButton.GetComponent<RectTransform>().localRotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, 0.0f));
        newButton.GetComponent<RectTransform>().localScale = new Vector3(1.0f, 1.0f, 1.0f);
        
        newButton.transform.GetChild(0).GetComponent<TMP_Text>().text = name;
        newButton.name = name + "Button";
        newButton.GetComponent<Button>().onClick.AddListener( delegate { onItemsButtonPressed(newButton); });
    }

    public void addPointsLayout(string name)
    {
        GameObject newLayout = Instantiate(pointsLayoutPrefab);
        newLayout.transform.parent = pointsPanel.transform;
        newLayout.name = name + " Layout";
    }

    public void addNewPointButton(string name, int pointNum)
    {
        GameObject newPointButton = Instantiate(pointButtonPrefab);

        for (int i = 0; i < pointsPanel.transform.childCount; i++)
        {
            GameObject currLayout = pointsPanel.transform.GetChild(i).gameObject;

            if (!currLayout.name.Contains(name))
                continue;

            newPointButton.transform.parent = currLayout.transform;
        }

        newPointButton.name = "Point " + pointNum;
    }
}
