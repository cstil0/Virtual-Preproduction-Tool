using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MenuButtonAction : MonoBehaviour
{
    public GameObject button;
    public GameObject handController;
    public GameObject debugPanelText;
    public GameObject canvas;
    public GameObject currMenuPanel;
    public GameObject newMenuPanel;

    bool triggerOn;
    bool buttonDown;
    bool isFirstTime;

    private void OnTriggerEnter(Collider other)
    {
        var colors = button.GetComponent<Button>().colors;
        colors.normalColor = Color.blue;
        //colors.normalColor = new Color(149, 149, 149);
        button.GetComponent<Button>().colors = colors;

        triggerOn = true;

        //panelText.GetComponent<Text>() = debugText;
    }

    private void OnTriggerExit(Collider other)
    {
        var colors = button.GetComponent<Button>().colors;
        colors.normalColor = Color.white;
        button.GetComponent<Button>().colors = colors;

        triggerOn = false;
        buttonDown = false;


    }
    // Start is called before the first frame update
    void Start()
    {
        // IGUAL NO ÉS NECESSARI FER-HO SI JA HO FA L'ENABLE
        triggerOn = false;
        buttonDown = false;
        isFirstTime = true;

        string debugText = "Debug panel working correctly";
        Text textComponent = debugPanelText.GetComponent<Text>();
        textComponent.text = debugText;
    }
    void OnEnable()
    {
        triggerOn = false;
        buttonDown = false;
        isFirstTime = true;
    }

    // Update is called once per frame
    void Update()
    {
        OVRInput.Update();

        // if button is pressed, controller is touching the menu and at least one time the controller has exit the menu
        if (OVRInput.Get(OVRInput.Button.SecondaryHandTrigger) && triggerOn)
        {
            // do it only once when the button was pressed
            if (!buttonDown && !isFirstTime)
            {
                // change menu
                buttonDown = true;
                ItemsMenu itemsMenu = canvas.GetComponent<ItemsMenu>();
                itemsMenu.change_MenuButton(currMenuPanel, newMenuPanel);

                //// debug
                //string debugText = currMenuPanel.name + " Out\n" + newMenuPanel.name + " In";
                //Text textComponent = debugPanelText.GetComponent<Text>();
                //textComponent.text = debugText;

                // change color
                var colors = button.GetComponent<Button>().colors;
                colors.normalColor = Color.white;
                button.GetComponent<Button>().colors = colors;
            }
        }
        else
        {
            buttonDown = false;
            // used to know if it is the first time that controller exits the menu, to avoid having the controller on the button after changing menu
            isFirstTime = false;
        }

        string debugText = isFirstTime.ToString();
        Text textComponent = debugPanelText.GetComponent<Text>();
        textComponent.text = debugText;

    }
}
