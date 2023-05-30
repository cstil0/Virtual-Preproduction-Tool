using Klak.Ndi.Interop;
using Microsoft.MixedReality.Toolkit;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ItemsDirectorPanelController : MonoBehaviour
{
    public static ItemsDirectorPanelController instance = null;

    [Header ("Buttons")]
    [SerializeField] Button closeButton;
    [SerializeField] Button removeButton;
    [SerializeField] Button speedMinusButton;
    [SerializeField] Button speedPlusButton;
    [SerializeField] Button trashButton;
    [SerializeField] Slider intensitySlider;
    [SerializeField] Button changeColorButton;
    [SerializeField] Button acceptColorButton;
    [SerializeField] Button cancelColorButton;

    [Header ("Texts")]
    [SerializeField] TMP_Text itemName;
    [SerializeField] TMP_InputField speedInput;
    [SerializeField] TMP_Text speedText;
    [SerializeField] TMP_Text intensityText;

    [Header ("Panels")]
    [SerializeField] GameObject panelLayout;
    [SerializeField] GameObject itemsOptionsPanel;
    [SerializeField] GameObject pointsPanel;
    [SerializeField] GameObject flexibleColorPicker;

    [Header ("UI Prefabs")]
    [SerializeField] GameObject itemButtonPrefab;
    [SerializeField] GameObject pointButtonPrefab;
    [SerializeField] GameObject pointsLayoutPrefab;

    [Header ("Selected Items")]
    private int currPointPressed;
    private string currItemPressed;
    private GameObject currItemGO;
    private FollowPath currFollowPath;
    private FollowPathCamera currFollowPathCamera;

    [Header ("Others")]
    [SerializeField] GameObject itemsParent;
    public Color selectedColor;
    public Color normalColor;
    public Color normalBlueColor;
    [SerializeField] FlexibleColorPicker colorPicker;
    private Color lastPickedColor;

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
        lastPickedColor = colorPicker.color;
    }

    // Update is called once per frame
    void Update()
    {
        //if (colorPicker.color != lastPickedColor)
        //{
        //    lastPickedColor = colorPicker.color;
        //    onColorChanged();
        //}
    }

    public void onItemsButtonPressed(GameObject itemButtonGO)
    {
        TMP_Text buttonText = itemButtonGO.transform.GetChild(0).GetComponent<TMP_Text>();
        Button itemButton = itemButtonGO.GetComponent<Button>();

        //string lastItemPressed = "";
        bool isAlreadySelected = false;
        if (currItemPressed == buttonText.text)
            isAlreadySelected = true;
        else
        {
            //if (currItemPressed != null)
                //lastItemPressed = currItemPressed;
            currItemPressed = buttonText.text;
        }

        itemName.text = buttonText.text;
        // common property
        flexibleColorPicker.gameObject.SetActive(false);

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
            currItemGO = itemsParent.transform.Find(currItemPressed).gameObject;
            currItemGO.TryGetComponent(out currFollowPath);
            currItemGO.TryGetComponent(out currFollowPathCamera);

            if (currFollowPath != null)
                speedInput.text = currFollowPath.posSpeed.ToString();

            if (currFollowPathCamera != null)
                speedInput.text = currFollowPathCamera.speed.ToString();

            for (int i = 0; i < panelLayout.transform.childCount; i++)
            {
                GameObject currButton = panelLayout.transform.GetChild(i).gameObject;
                if (currButton.name == currItemPressed + "Button")
                {
                    // change color to visualize it as selected
                    ColorBlock buttonColors = currButton.GetComponent<Button>().colors;
                    buttonColors.normalColor = selectedColor;
                    currButton.GetComponent<Button>().colors = buttonColors;
                }
                else
                {
                    ColorBlock lastButtonColors = currButton.GetComponent<Button>().colors;
                    lastButtonColors.normalColor = normalBlueColor;
                    currButton.GetComponent<Button>().colors = lastButtonColors;
                }
            }
        }
        else
        {
            ColorBlock buttonColors = itemButton.GetComponent<Button>().colors;
            buttonColors.normalColor = normalBlueColor;
            itemButton.GetComponent<Button>().colors = buttonColors;

            currItemPressed = "";
        }
        showHideElements(buttonText, isAlreadySelected);
    }

    private void showHideElements(TMP_Text buttonText, bool isAlreadySelected)
    {
        if (buttonText.transform.parent.name.Contains("Camera"))
        {
            pointsPanel.SetActive(!isAlreadySelected);
            removeButton.gameObject.SetActive(false);
            speedInput.gameObject.SetActive(!isAlreadySelected);
            speedText.gameObject.SetActive(!isAlreadySelected);
            speedMinusButton.gameObject.SetActive(!isAlreadySelected);
            speedPlusButton.gameObject.SetActive(!isAlreadySelected);
            trashButton.gameObject.SetActive(!isAlreadySelected);
            intensityText.gameObject.SetActive(false);
            intensitySlider.gameObject.SetActive(false);
            changeColorButton.gameObject.SetActive(false);
            acceptColorButton.gameObject.SetActive(false);
            cancelColorButton.gameObject.SetActive(false);
        }
        else if (buttonText.transform.parent.name.Contains("Focus"))
        {
            intensitySlider.value = currItemGO.GetComponent<LightController>().getIntensity();

            pointsPanel.SetActive(false);
            removeButton.gameObject.SetActive(!isAlreadySelected);
            speedInput.gameObject.SetActive(false);
            speedText.gameObject.SetActive(false);
            speedMinusButton.gameObject.SetActive(false);
            speedPlusButton.gameObject.SetActive(false);
            trashButton.gameObject.SetActive(false);
            intensityText.gameObject.SetActive(!isAlreadySelected);
            intensitySlider.gameObject.SetActive(!isAlreadySelected);
            changeColorButton.gameObject.SetActive(!isAlreadySelected);
            acceptColorButton.gameObject.SetActive(false);
            cancelColorButton.gameObject.SetActive(false);
        }
        else
        {
            // if it is already selected, we want to hide the remove button. Otherwise, show it
            pointsPanel.SetActive(!isAlreadySelected);
            removeButton.gameObject.SetActive(!isAlreadySelected);
            speedInput.gameObject.SetActive(!isAlreadySelected);
            speedText.gameObject.SetActive(!isAlreadySelected);
            speedMinusButton.gameObject.SetActive(!isAlreadySelected);
            speedPlusButton.gameObject.SetActive(!isAlreadySelected);
            trashButton.gameObject.SetActive(!isAlreadySelected);
            intensityText.gameObject.SetActive(false);
            intensitySlider.gameObject.SetActive(false);
            changeColorButton.gameObject.SetActive(false);
            acceptColorButton.gameObject.SetActive(false);
            cancelColorButton.gameObject.SetActive(false);
        }
    }

    public void onChangeColorButtonPressed()
    {
        colorPicker.color = currItemGO.GetComponent<LightController>().getLightColor();

        removeButton.gameObject.SetActive(false);
        speedInput.gameObject.SetActive(false);
        speedText.gameObject.SetActive(false);
        speedMinusButton.gameObject.SetActive(false);
        speedPlusButton.gameObject.SetActive(false);
        trashButton.gameObject.SetActive(false);
        intensityText.gameObject.SetActive(false);
        intensitySlider.gameObject.SetActive(false);
        changeColorButton.gameObject.SetActive(false);

        acceptColorButton.gameObject.SetActive(true);
        cancelColorButton.gameObject.SetActive(true);
        flexibleColorPicker.gameObject.SetActive(true);
    }


    public void onColorChanged()
    {
        UDPSender.instance.sendChangeLightColor(currItemPressed, colorPicker.color, false);

        currItemGO.GetComponent<LightController>().changeLightColor(colorPicker.color, false);
    }

    public void onIntensityChanged()
    {
        float intensity = intensitySlider.value;
        UDPSender.instance.sendChangeLightIntensity(currItemPressed, intensity);

        currItemGO.GetComponent<LightController>().changeLightIntensity(intensity);
    }

    public void onAcceptColorButtonPressed()
    {
        UDPSender.instance.sendChangeLightColor(currItemPressed, colorPicker.color, true);
        currItemGO.GetComponent<LightController>().changeLightColor(colorPicker.color, true);

        removeButton.gameObject.SetActive(true);
        intensityText.gameObject.SetActive(true);
        intensitySlider.gameObject.SetActive(true);
        changeColorButton.gameObject.SetActive(true);
        acceptColorButton.gameObject.SetActive(false);
        cancelColorButton.gameObject.SetActive(false);
        flexibleColorPicker.gameObject.SetActive(false);
    }

    public void onCancelColorButtonPressed()
    {
        LightController lightController = currItemGO.GetComponent<LightController>();
        Color originalColor = lightController.getOriginalLightColor();
        colorPicker.color = originalColor; 
        lightController.changeLightColor(originalColor, true);
        UDPSender.instance.sendChangeLightColor(currItemPressed, originalColor, true);

        removeButton.gameObject.SetActive(true);
        intensitySlider.gameObject.SetActive(true);
        changeColorButton.gameObject.SetActive(true);
        acceptColorButton.gameObject.SetActive(false);
        cancelColorButton.gameObject.SetActive(false);
        flexibleColorPicker.gameObject.SetActive(false);
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
        float speedChange = 1.0f;

        if (currItemPressed.Contains("MainCamera"))
            speedChange = 0.01f;

        speedInput.text = (float.Parse(speedInput.text) + speedChange).ToString();
        onSpeedChange(float.Parse(speedInput.text));
    }

    public void onSpeedMinusPressed()
    {
        float speedChange = 1.0f;

        if (currItemPressed.Contains("MainCamera"))
            speedChange = 0.01f;

        speedInput.text = (float.Parse(speedInput.text) - speedChange).ToString();
        onSpeedChange(float.Parse(speedInput.text));
    }

    public void onSpeedInput()
    {
        try
        {
            onSpeedChange(int.Parse(speedInput.text));
        }
        catch (Exception e) { }
    }

    public void onSpeedChange(float speed)
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

        deletePointButton(currItemGO, currItemPressed, currPointPressed);
        currPointPressed = -1;
    }

    public void deletePointButton(GameObject item, string itemName, int pointNum)
    {
        item.TryGetComponent<FollowPath>(out FollowPath followPath);
        item.TryGetComponent<FollowPathCamera>(out FollowPathCamera followPathCamera);

        if (followPath != null)
            followPath.deletePathPoint(pointNum, false);

        if (followPathCamera != null)
            followPathCamera.deletePathPoint(pointNum, false);

        // eliminate point button
        Transform itemPointsLayout = pointsPanel.transform.Find(itemName + " Layout");

        for (int i = 0; i < itemPointsLayout.transform.childCount; i++)
        {
            if (i == pointNum)
                Destroy(itemPointsLayout.transform.GetChild(i).gameObject);
            // the substraction is due to the fact we are starting at the second position
            if (i > pointNum)
            {
                Transform pointButton = itemPointsLayout.transform.GetChild(i);
                pointButton.name = "Point " + (i - 1);
                pointButton.GetChild(0).GetComponent<TMP_Text>().text = (i - 1).ToString();
            }
        }
    }

    public void onPointPressed(Transform pointsPanelLayout, GameObject pointButton)
    {
        string[] splittedName = pointButton.name.Split(" ");
        int pointNum = int.Parse(splittedName[1]);

        if (pointNum == currPointPressed)
            currPointPressed = -1;
        else
            currPointPressed = pointNum;

        for (int i=0; i < pointsPanelLayout.childCount; i++)
        {
            GameObject currPointButton = pointsPanelLayout.GetChild(i).gameObject;
            splittedName = currPointButton.name.Split(" ");
            int currPointNum = int.Parse(splittedName[1]);

            if (currPointNum == currPointPressed)
            {
                ColorBlock buttonColors = currPointButton.GetComponent<Button>().colors;
                buttonColors.normalColor = selectedColor;
                currPointButton.GetComponent<Button>().colors = buttonColors;
            }
            else
            {
                ColorBlock buttonColors = currPointButton.GetComponent<Button>().colors;
                buttonColors.normalColor = normalBlueColor;
                currPointButton.GetComponent<Button>().colors = buttonColors;
            }

        }

        DirectorPanelManager.instance.changePointsViewTexture(currItemPressed, currPointPressed);
    }

    public void onRemoveItemPressed()
    {
        if (!currItemPressed.Contains("Camera"))
        {
            UDPSender.instance.sendDeleteItemToAssistant(currItemPressed);

            removeItemButtons(currItemPressed);
        }
    }

    public void removeItemButtons(string itemName)
    {
        GameObject itemButton = panelLayout.transform.Find(itemName + "Button").gameObject;
        Destroy(itemButton);

        // the item may not have any point created yet, so we need to handle exceptions
        try
        {
            GameObject itemPointsLayout = pointsPanel.transform.Find(itemName + " LAYOUT").gameObject;
            Destroy(itemPointsLayout);
        }
        catch (Exception e) { }
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

        RectTransform rTrans = newLayout.GetComponent<RectTransform>();
        rTrans.offsetMin = new Vector2(10, 7);
        rTrans.offsetMax = new Vector2(-10, -7);
        rTrans.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        rTrans.localRotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, 0.0f));
        rTrans.localPosition = new Vector3(rTrans.localPosition.x, rTrans.localPosition.y, 0);
    }

    public void addNewPointButton(string name, int pointNum)
    {
        GameObject newPointButton = Instantiate(pointButtonPrefab);
        Transform parentLayout = null;

        for (int i = 0; i < pointsPanel.transform.childCount; i++)
        {
            GameObject currLayout = pointsPanel.transform.GetChild(i).gameObject;

            if (!currLayout.name.Contains(name))
                continue;

            newPointButton.transform.parent = currLayout.transform;
            parentLayout = currLayout.transform;
            RectTransform rTrans = newPointButton.GetComponent<RectTransform>();
            rTrans.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            rTrans.localRotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, 0.0f));
            rTrans.localPosition = new Vector3(rTrans.localPosition.x, rTrans.localPosition.y, 0);
        }

        newPointButton.name = "Point " + pointNum;
        newPointButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = pointNum.ToString();
        newPointButton.GetComponent<Button>().onClick.AddListener(delegate { onPointPressed(parentLayout, newPointButton); });
    }
}
