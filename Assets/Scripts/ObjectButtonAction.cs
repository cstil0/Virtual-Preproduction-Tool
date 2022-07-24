using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ObjectButtonAction : MonoBehaviour
{
    public GameObject button;
    public GameObject handController;
    public GameObject panelText;
    public GameObject canvas;
    public GameObject itemPrefab;

    bool triggerOn;
    bool buttonDown;

    private void OnTriggerEnter(Collider other)
    {
        OVRInput.Update();
        if (other.gameObject == handController) //&& OVRInput.Get(OVRInput.Button.One))
        {
            var colors = button.GetComponent<Button>().colors;
            //colors.normalColor = Color.red;
            colors.normalColor = new Color(149, 149, 149);
            button.GetComponent<Button>().colors = colors;

            if (OVRInput.Get(OVRInput.Button.SecondaryHandTrigger) && !triggerOn)
            {
                if (!buttonDown)
                {
                    buttonDown = true;
                    ItemsMenu itemsMenu = canvas.GetComponent<ItemsMenu>();
                    itemsMenu.ObjectButton(itemPrefab);
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
        Text textComponent = panelText.GetComponent<Text>();
        textComponent.text = debugText;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
