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
    [SerializeField] ItemsDirectorPanelScroll itemsDirectorPanelScroll;
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

    void Start()
    {
        lastPickedColor = colorPicker.color;
        currPointPressed = -1;
        currItemPressed = "";
    }

    void Update()
    {

    }

    public void onItemsButtonPressed(GameObject itemButtonGO)
    {
        TMP_Text buttonText = itemButtonGO.transform.GetChild(0).GetComponent<TMP_Text>();
        Button itemButton = itemButtonGO.GetComponent<Button>();

        // check if the current selected item was already selected
        bool isAlreadySelected = false;
        if (currItemPressed == buttonText.text)
            isAlreadySelected = true;
        else
            currItemPressed = buttonText.text;

        itemName.text = buttonText.text;
        flexibleColorPicker.gameObject.SetActive(false);

        // activate the item options panel if the item was not selected before, else disable it
        itemsOptionsPanel.SetActive(!isAlreadySelected);

        // iterate the existing points panel and enable the one that corresponds to the current selected item
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
            deselectAllPoints(currItemPressed);
            // get the corresponding follow path script to obtain its speed value
            currItemGO = itemsParent.transform.Find(currItemPressed).gameObject;
            currItemGO.TryGetComponent(out currFollowPath);
            currItemGO.TryGetComponent(out currFollowPathCamera);

            if (currFollowPath != null)
                speedInput.text = currFollowPath.posSpeed.ToString();

            if (currFollowPathCamera != null)
                speedInput.text = currFollowPathCamera.speed.ToString();

            // iterate through all items buttons of the panel to show their corresponding colors
            for (int i = 0; i < panelLayout.transform.childCount; i++)
            {
                GameObject currButton = panelLayout.transform.GetChild(i).gameObject;
                // if button corresponds to selected item, change its color to visualize it as selected
                if (currButton.name == currItemPressed + "Button")
                {
                    ColorBlock buttonColors = currButton.GetComponent<Button>().colors;
                    buttonColors.normalColor = selectedColor;
                    currButton.GetComponent<Button>().colors = buttonColors;
                }

                // if it does not correspond, change its color to visualize it as deselected
                else
                {
                    ColorBlock lastButtonColors = currButton.GetComponent<Button>().colors;
                    lastButtonColors.normalColor = normalBlueColor;
                    currButton.GetComponent<Button>().colors = lastButtonColors;
                }
            }
        }

        // if button was already selected, just change is color to visualize it as deselected
        else
        {
            ColorBlock buttonColors = itemButton.GetComponent<Button>().colors;
            buttonColors.normalColor = normalBlueColor;
            itemButton.GetComponent<Button>().colors = buttonColors;

            currItemPressed = "";
        }
        // show the control properties shown in the items control panel
        showHideElements(buttonText, isAlreadySelected);
    }

    // enable or disable properties shown in the items control panel depending on the type of item that was pressed
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

    // show the light color controls
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

    // save and send new light color
    public void onColorChanged()
    {
        UDPSender.instance.sendChangeLightColor(currItemPressed, colorPicker.color, false);
        currItemGO.GetComponent<LightController>().changeLightColor(colorPicker.color, false);
    }

    // save and send new light intensity
    public void onIntensityChanged()
    {
        float intensity = intensitySlider.value;
        UDPSender.instance.sendChangeLightIntensity(currItemPressed, intensity);
        currItemGO.GetComponent<LightController>().changeLightIntensity(intensity);
    }

    // show the light controls and send new colore after accepting the light color
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

    // show the light controls and send original color after canceling the light color
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

    // hide item control panel
    public void onCloseButtonPressed()
    {
        itemsOptionsPanel.SetActive(false);
        currItemGO = null;
        currItemPressed = null;
        currPointPressed = -1;
    }

    // send and save new speed value
    public void onSpeedPlusPressed()
    {
        float speedChange = 0.5f;

        speedInput.text = (float.Parse(speedInput.text) + speedChange).ToString();
        onSpeedChange(float.Parse(speedInput.text));
    }

    // send and save new speed value
    public void onSpeedMinusPressed()
    {
        float speedChange = 0.5f;

        speedInput.text = (float.Parse(speedInput.text) - speedChange).ToString();
        onSpeedChange(float.Parse(speedInput.text));
    }

    // send and save new speed value
    public void onSpeedInput()
    {
        try
        {
            onSpeedChange(int.Parse(speedInput.text));
        }
        catch (Exception e) { }
    }

    // send and save new speed value
    public void onSpeedChange(float speed)
    {
        UDPSender.instance.sendChangeSpeed(speed, currItemPressed);

        if (currFollowPath != null)
            currFollowPath.posSpeed = speed;

        if (currFollowPathCamera != null)
            currFollowPathCamera.speed = speed;
    }

    // send and delete path point
    public void onTrashPressed()
    {
        if (currPointPressed != -1)
        {
            UDPSender.instance.sendDeletePointToAssistant(currPointPressed, currItemPressed);
            deletePointButton(currItemGO, currItemPressed, currPointPressed);
        }
    }

    public void deletePointButton(GameObject item, string itemName, int pointNum)
    {
        // get the corresponding follow path script to delete point from there
        item.TryGetComponent<FollowPath>(out FollowPath followPath);
        item.TryGetComponent<FollowPathCamera>(out FollowPathCamera followPathCamera);

        if (followPath != null)
            followPath.deletePathPoint(pointNum, false, false);

        if (followPathCamera != null)
            followPathCamera.deletePathPoint(pointNum, false, false);

        // eliminate point button
        Transform itemPointsLayout = pointsPanel.transform.Find(itemName + " Layout");

        // iterate through all buttons and change following ones num
        for (int i = itemPointsLayout.transform.childCount - 1; i >= 0; i--)
        {
            // destroy point button
            if (i == pointNum)
            {
                // force the deleted point to be selected, to then deselect it and ensure that all points are unselected
                currPointPressed = pointNum;
                onPointPressed(itemPointsLayout, itemPointsLayout.transform.GetChild(i).gameObject);
                Destroy(itemPointsLayout.transform.GetChild(i).gameObject);
            }

            // change number for the following ones
            // the substraction is due to the fact we are starting at the second position
            if (i > pointNum)
            {
                Transform pointButton = itemPointsLayout.transform.GetChild(i);
                pointButton.name = "Point " + (i - 1);
                pointButton.GetChild(0).GetComponent<TMP_Text>().text = (i - 1).ToString();
            }
        }

        deselectAllPoints(itemName);
    }

    public void onPointPressed(Transform pointsPanelLayout, GameObject pointButton)
    {
        // get point num based on its name
        string[] splittedName = pointButton.name.Split(" ");
        int pointNum = int.Parse(splittedName[1]);

        Debug.Log("POINT PRESSED!!. CURRENT: " + currPointPressed + ". NEW: " + pointNum);
        // if the point was already pressed, change the pressed one to -1 to represent that none is selected now
        if (pointNum == currPointPressed)
            currPointPressed = -1;
        else
            currPointPressed = pointNum;

        Debug.Log("NOW PRESSED: " + currPointPressed);

        // iterate through all points to change their color according to their current state
        for (int i=0; i < pointsPanelLayout.childCount; i++)
        {
            GameObject currPointButton = pointsPanelLayout.GetChild(i).gameObject;
            splittedName = currPointButton.name.Split(" ");
            int currPointNum = int.Parse(splittedName[1]);

            // if point corresponds to the currently selected one, show it as selected
            if (currPointNum == currPointPressed)
            {
                ColorBlock buttonColors = currPointButton.GetComponent<Button>().colors;
                buttonColors.normalColor = selectedColor;
                currPointButton.GetComponent<Button>().colors = buttonColors;
            }

            // if point does not correspond to the currently selected one, show it as deselected
            else
            {
                ColorBlock buttonColors = currPointButton.GetComponent<Button>().colors;
                buttonColors.normalColor = normalBlueColor;
                currPointButton.GetComponent<Button>().colors = buttonColors;
            }

        }

        // change camera points view according to the new selected point
        DirectorPanelManager.instance.changePointsViewTexture(currItemPressed, currPointPressed);
    }

    public void onRemoveItemPressed()
    {
        // remove all items point buttons and inform of the removed item
        if (!currItemPressed.Contains("Camera"))
        {
            UDPSender.instance.sendDeleteItemToAssistant(currItemPressed);
            removeItemButtons(currItemPressed);

            // update maximum displacement that the items panel can perform according to the new number of item buttons
            itemsDirectorPanelScroll.updateMaxDisplacement();

            onCloseButtonPressed();
            currPointPressed = -1;
        }
    }

    public void removeItemButtons(string itemName)
    {
        // destroy item button and points buttons
        GameObject itemButton = panelLayout.transform.Find(itemName + "Button").gameObject;
        Destroy(itemButton);

        // the item may not have any point created yet, so we need to handle exceptions
        try
        {
            GameObject itemPointsLayout = pointsPanel.transform.Find(itemName + " Layout").gameObject;
            Destroy(itemPointsLayout);
        }
        catch (Exception e) { }
    }

    private void deselectAllPoints(string itemName)
    {
        Transform itemPointsLayout = pointsPanel.transform.Find(itemName + " Layout");

        // iterate through all buttons and change following ones num
        for (int i = 0; i < itemPointsLayout.transform.childCount; i++)
        {
            Transform currPointButton = itemPointsLayout.transform.GetChild(i);
            ColorBlock buttonColors = currPointButton.GetComponent<Button>().colors;
            buttonColors.normalColor = normalBlueColor;
            currPointButton.GetComponent<Button>().colors = buttonColors;
        }

        currPointPressed = -1;
    }

    public void addNewItemButton(string name)
    {
        // instantiate a new item button in the corresponding panel
        GameObject newButton = Instantiate(itemButtonPrefab);
        newButton.transform.parent = panelLayout.transform;
        // ensure correct transform properties
        newButton.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0.0f, 0.0f, 0.0f);
        newButton.GetComponent<RectTransform>().localRotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, 0.0f));
        newButton.GetComponent<RectTransform>().localScale = new Vector3(1.0f, 1.0f, 1.0f);
        
        // change button name and add event listener to perform an action when it is pressed
        newButton.transform.GetChild(0).GetComponent<TMP_Text>().text = name;
        newButton.name = name + "Button";
        newButton.GetComponent<Button>().onClick.AddListener( delegate { onItemsButtonPressed(newButton); });

        // update maximum displacement that the items panel can perform according to the new number of item buttons
        itemsDirectorPanelScroll.updateMaxDisplacement();
    }

    // create points layout when the first point for a character or camera is defined
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

        // iterate through all point layouts to add the new button to the corresponding one
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

        // change button name according to the corresponding point num
        newPointButton.name = "Point " + pointNum;
        newPointButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = pointNum.ToString();
        newPointButton.GetComponent<Button>().onClick.AddListener(delegate { onPointPressed(parentLayout, newPointButton); });
    }
}
