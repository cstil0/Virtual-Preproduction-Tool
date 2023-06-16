using Microsoft.MixedReality.Toolkit;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class SubmenusNavigate : MonoBehaviour
{
    public ActivateDisablePages adp;

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

        StartCoroutine(init());
    }

    IEnumerator init()
    {
        while(adp.buttonsCount == 0) { yield return null; }

        activateDisableButtons();
        checkPreviousNextButtons();
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

    void Start()
    {
        buttonReleasedOnce = false;
        triggerOn = false;
        buttonDown = false;
    }

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
                    onPreviousButtonPressed();

                else if (buttonType == ebuttonType.NEXT)
                    onNextButtonPressed();
            }
            buttonDown = false;
        }
    }

    public void onPreviousButtonPressed()
    {
        // check there is at least one previous page left to go
        if (adp.currentPage - 1 >= 0)
        {
            adp.currentPage--;

            // if it was possible to press previous button, then next one must be enabled
            changeButtonColor(nextButton, Color.white, true);
            nextButton.gameObject.GetComponent<SubmenusNavigate>().isEnabled = true;

            checkPreviousNextButtons();

            activateDisableButtons();
        }

        if (ModesManager.instance.role == ModesManager.eRoleType.ASSISTANT)
            UDPSender.instance.sendMenuNavigation("PREVIOUS_BUTTON", adp.gameObject.name);
    }

    public void onNextButtonPressed()
    {
        // check there is at least one following page left to go
        if (adp.currentPage + 1 < adp.buttonsCount / buttonsPerPage)
        {
            adp.currentPage++;

            // if it was possible to press next button, then previous one must be enabled
            changeButtonColor(previousButton, Color.white, true);
            previousButton.gameObject.GetComponent<SubmenusNavigate>().isEnabled = true;

            checkPreviousNextButtons();

            activateDisableButtons();
        }

        if (ModesManager.instance.role == ModesManager.eRoleType.ASSISTANT)
            UDPSender.instance.sendMenuNavigation("NEXT_BUTTON", adp.gameObject.name);
    }

    public void activateDisableButtons()
    {
        int startButton = adp.currentPage * buttonsPerPage;
        int endButton = startButton + buttonsPerPage;

        // iterate through each button and determine if it should be enabled or not
        for (int i = 0; i < adp.buttonsCount; i++)
        {
            if (i >= startButton && i < endButton)
                currentMenu.transform.GetChild(i).gameObject.SetActive(true);
            else if ((i >= endButton && i < adp.buttonsCount) || i < startButton )
                currentMenu.transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    void checkPreviousNextButtons()
    {
       if (buttonType == ebuttonType.PREVIOUS)
        {
            // check if previous button should be shown as disabled or not
            if (adp.currentPage - 1 < 0)
                isEnabled = false;
            else
                isEnabled = true;
            
            changeButtonColor(previousButton, Color.white, isEnabled);
        }

        else if (buttonType == ebuttonType.NEXT)
        {
            // check if next button should be shown as disabled or not
            if (adp.currentPage + 1 >= adp.buttonsCount / buttonsPerPage)
                isEnabled = false;
            else
                isEnabled = true;
            changeButtonColor(nextButton, Color.white, isEnabled);
        }
    }

    // change button color according to its corresponding state
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