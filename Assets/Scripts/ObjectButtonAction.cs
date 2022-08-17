using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ObjectButtonAction : MonoBehaviour
{
    public GameObject button;
    public GameObject handController;
    public GameObject debugPanelText;
    public GameObject canvas;
    public GameObject itemPrefab;


    bool triggerOn;
    bool buttonDown;
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
        // POTSER NO ÉS NECESSARI FER-HO SI JA HO ESTÀ FENT EL ENABLE
        triggerOn = false;
        buttonDown = false;
        buttonReleasedOnce = false;


        string debugText = "Debug panel working correctly";
        Text textComponent = debugPanelText.GetComponent<Text>();
        textComponent.text = debugText;
    }

    private void OnEnable()
    {
        triggerOn = false;
        buttonDown = false;
        buttonReleasedOnce = false;
    }

    // Update is called once per frame
    void Update()
    {
        OVRInput.Update();
        // if button is pressed and hand is touching the menu, instantiate the object
        if (OVRInput.Get(OVRInput.Button.SecondaryHandTrigger) && triggerOn)
        {
            // do it only once when the button is pressed and after button was released at least once
            if (!buttonDown && buttonReleasedOnce)
            {
                buttonDown = true;
                //isFirstTime = false;
                ItemsMenu itemsMenu = canvas.GetComponent<ItemsMenu>();
                itemsMenu.ObjectButton(itemPrefab, handController);

                // AIXÒ POTSER NO CAL
                buttonReleasedOnce = false;
            }
        }
        else
        {
            buttonDown = false;
            buttonReleasedOnce = true;
        }

        string debugText = "Dow: " + buttonDown.ToString() + " Rel: " + buttonReleasedOnce.ToString() + " Trig " + triggerOn.ToString() + "\nObjectButtonAction";
        Text textComponent = debugPanelText.GetComponent<Text>();
        textComponent.text = debugText;

    }
}
