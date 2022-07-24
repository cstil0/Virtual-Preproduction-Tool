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

    private void OnTriggerEnter(Collider other)
    {
        OVRInput.Update();
        if (other.gameObject == handController) //&& OVRInput.Get(OVRInput.Button.One))
        {
            var colors = button.GetComponent<Button>().colors;
            colors.normalColor = Color.red;
            //colors.normalColor = new Color(149, 149, 149);
            button.GetComponent<Button>().colors = colors;

            OVRInput.Update();
            if (OVRInput.Get(OVRInput.Button.SecondaryHandTrigger) && !triggerOn)
            {
                if (!buttonDown)
                {
                    // change menu
                    buttonDown = true;
                    ItemsMenu itemsMenu = canvas.GetComponent<ItemsMenu>();
                    itemsMenu.change_MenuButton(currMenuPanel, newMenuPanel);

                    // debug
                    string debugText = currMenuPanel.name + " Out\n" + newMenuPanel.name + " In";
                    Text textComponent = debugPanelText.GetComponent<Text>();
                    textComponent.text = debugText;

                    // change color
                    var colors = button.GetComponent<Button>().colors;
                    colors.normalColor = Color.white;
                    button.GetComponent<Button>().colors = colors;
                }
            }
            else
            {
                buttonDown = false;
            }
        }

        triggerOn = true;

        //panelText.GetComponent<Text>() = debugText;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == handController)
        {
            var colors = button.GetComponent<Button>().colors;
            colors.normalColor = Color.white;
            button.GetComponent<Button>().colors = colors;
        }

        triggerOn = false;
    }
    // Start is called before the first frame update
    void Start()
    {
        triggerOn = false;
        buttonDown = false;

        string debugText = "Debug panel working correctly";
        Text textComponent = debugPanelText.GetComponent<Text>();
        textComponent.text = debugText;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
