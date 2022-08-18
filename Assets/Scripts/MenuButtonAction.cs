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
    // used to know if the button was already released at least once when comming from previous menu to avoid pressing the new button after the change
    bool buttonReleasedOnce;


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
    }


    // Start is called before the first frame update
    void Start()
    {
        // IGUAL NO ÉS NECESSARI FER-HO SI JA HO FA L'ENABLE
        triggerOn = false;
        buttonDown = false;
        buttonReleasedOnce = false;

        //string debugText = "Debug panel working correctly";
        //Text textComponent = debugPanelText.GetComponent<Text>();
        //textComponent.text = debugText;
    }

    void OnEnable()
    {
        triggerOn = false;
        buttonDown = false;
        buttonReleasedOnce = false;
    }

    // Update is called once per frame
    void Update()
    {
        OVRInput.Update();
        // AIXÒ ESTARIA MÉS BONIC A UN TRIGGER STAY PER ESTALVIARNOS EL TRIGGERON
        // if button is pressed, controller is touching the menu and at least one time the controller has exit the menu
        if (OVRInput.Get(OVRInput.Button.SecondaryHandTrigger) && triggerOn)
        {
            // do it only once when the button was pressed
            if (!buttonDown && buttonReleasedOnce)
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

                // POTSER AIXÒ NO ÉS NECESSARI
                buttonReleasedOnce = false;
            }
            //else if (!buttonDown)
            //    buttonReleasedOnce = true;
        }
        else
        {
            buttonDown = false;
            buttonReleasedOnce = true;
        }

        //string debugText = "Dow: " + buttonDown.ToString() + " Rel: " + buttonReleasedOnce.ToString() + " Trig " + triggerOn.ToString() + "\nMenuButtonAction";
        //Text textComponent = debugPanelText.GetComponent<Text>();
        //textComponent.text = debugText;

    }
}
