using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class SubmenusNavigate : MonoBehaviour
{
    public ActivateDisablePages adp;

    int numPages;
    public int buttonsPerPage = 3;

    bool triggerOn;
    bool buttonDown;
    bool buttonReleasedOnce;

    public GameObject currentMenu;
    public Button previousButton;
    public Button nextButton;
    public bool isEnabled;

    public enum ebuttonType
    {
        PREVIOUS,
        NEXT
    }

    public ebuttonType buttonType;

    private void OnEnable()
    {
        triggerOn = false;
        buttonDown = false;
        buttonReleasedOnce = false;

        if (buttonType == ebuttonType.PREVIOUS)
            isEnabled = false;
        else
            isEnabled = true;

        changeButtonColor(gameObject.GetComponent<Button>(), Color.white, isEnabled);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Change color to make hover effect
        changeButtonColor(gameObject.GetComponent<Button>(), Color.blue, isEnabled);

        triggerOn = true;
    }

    private void OnTriggerExit(Collider other)
    {
        changeButtonColor(gameObject.GetComponent<Button>(), Color.white, isEnabled);

        triggerOn = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        buttonReleasedOnce = false;
        triggerOn = false;
        buttonDown = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger) && triggerOn)
        {
            // do it only once when the button is pressed and after button was released at least once
            if (!buttonDown)
            {
                buttonDown = true;
            }
        }
        else
        {
            if (!triggerOn)
                buttonReleasedOnce = true;

            if (buttonDown && triggerOn && buttonReleasedOnce)
            {
                if (buttonType == ebuttonType.PREVIOUS)
                {
                    if (adp.currentPage - 1 >= 0)
                    {
                        adp.currentPage--;

                        // if it was possible to press previous button, then next one must be enabled
                        changeButtonColor(nextButton, Color.white, true);
                        nextButton.gameObject.GetComponent<SubmenusNavigate>().isEnabled = true;

                        int startButton = adp.currentPage * 3;
                        for (int i = startButton; i < startButton + 3; i++)
                        {
                            if (i < adp.buttonsCount)
                                currentMenu.transform.GetChild(i).gameObject.SetActive(true);
                        }

                        for (int i = startButton + 3; i < startButton + 6; i++)
                        {
                            if (i < adp.buttonsCount)
                                currentMenu.transform.GetChild(i).gameObject.SetActive(false);
                        }
                    }
                    else
                    {
                        isEnabled = false;
                        changeButtonColor(previousButton, Color.white, isEnabled);
                    }
                }
                else if (buttonType == ebuttonType.NEXT)
                {
                    if (adp.currentPage + 1 < adp.buttonsCount / 3)
                    {
                        adp.currentPage++;

                        // if it was possible to press next button, then previous one must be enabled
                        changeButtonColor(previousButton, Color.white, true);
                        previousButton.gameObject.GetComponent<SubmenusNavigate>().isEnabled = true;

                        int startButton = adp.currentPage * 3;
                        if (adp.currentPage - 1 >= 0)
                        {
                            for (int i = startButton - 3; i < startButton; i++)
                            {
                                if (i < adp.buttonsCount)
                                    currentMenu.transform.GetChild(i).gameObject.SetActive(false);
                            }
                        }

                        if (startButton < adp.buttonsCount)
                        {
                            for (int i = startButton; i < startButton + 3; i++)
                            {
                                if (i < adp.buttonsCount)
                                    currentMenu.transform.GetChild(i).gameObject.SetActive(true);
                            }
                        }
                    }
                    else
                    {
                        isEnabled = false;
                        changeButtonColor(nextButton, Color.white, isEnabled);
                    }

                }
            }
            buttonDown = false;
        }
    }

    void changeButtonColor(Button button, Color color, bool isEnabled)
    {
        ColorBlock buttonColors = button.colors;
        if (!isEnabled)
            color.a = 0.3f;

        buttonColors.normalColor = color;
        button.colors = buttonColors;

        button.transform.GetComponentInChildren<Text>().color = color;
    }
}