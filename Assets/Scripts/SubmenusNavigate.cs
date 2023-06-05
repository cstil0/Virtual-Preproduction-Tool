using System.Collections;
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

        StartCoroutine(init());
    }

    IEnumerator init()
    {
        while(adp.buttonsCount == 0) { yield return null; }

        if (buttonType == ebuttonType.PREVIOUS)
            isEnabled = false;
        else if (adp.currentPage + 1 < adp.buttonsCount / buttonsPerPage)
            isEnabled = true;
        else
            isEnabled = false;

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
                {
                    // check there is at least one previous page left to go
                    if (adp.currentPage - 1 >= 0)
                    {
                        adp.currentPage--;

                        // if it was possible to press previous button, then next one must be enabled
                        changeButtonColor(nextButton, Color.white, true);
                        nextButton.gameObject.GetComponent<SubmenusNavigate>().isEnabled = true;

                        // check if previous button should be shown as disabled or not
                        if (adp.currentPage - 1 < 0)
                            isEnabled = false;
                        else
                            isEnabled = true;

                        changeButtonColor(previousButton, Color.white, isEnabled);

                        int startButton = adp.currentPage * buttonsPerPage;

                        // iterate through each button in the current and previous pages to determine if it should be enabled or not
                        for (int i = startButton; i < startButton + 3; i++)
                        {
                            if (i < adp.buttonsCount)
                                currentMenu.transform.GetChild(i).gameObject.SetActive(true);
                        }

                        for (int i = startButton + buttonsPerPage; i < startButton + buttonsPerPage * 2; i++)
                        {
                            if (i < adp.buttonsCount)
                                currentMenu.transform.GetChild(i).gameObject.SetActive(false);
                        }
                    }
                    // if there is no previous page left, ensure that the button is shown as disabled
                    else
                    {
                        isEnabled = false;
                        changeButtonColor(previousButton, Color.white, isEnabled);
                    }
                }
                else if (buttonType == ebuttonType.NEXT)
                {
                    // check there is at least one following page left to go
                    if (adp.currentPage + 1 < adp.buttonsCount / buttonsPerPage)
                    {
                        adp.currentPage++;

                        // if it was possible to press next button, then previous one must be enabled
                        changeButtonColor(previousButton, Color.white, true);
                        previousButton.gameObject.GetComponent<SubmenusNavigate>().isEnabled = true;

                        // check if next button should be shown as disabled or not
                        if (adp.currentPage + 1 >= adp.buttonsCount / buttonsPerPage)
                            isEnabled = false;
                        else
                            isEnabled = true;
                        changeButtonColor(nextButton, Color.white, isEnabled);

                        int startButton = adp.currentPage * buttonsPerPage;
                        if (adp.currentPage - 1 >= 0)
                        {
                            for (int i = startButton - buttonsPerPage; i < startButton; i++)
                            {
                                if (i < adp.buttonsCount)
                                    currentMenu.transform.GetChild(i).gameObject.SetActive(false);
                            }
                        }

                        if (startButton < adp.buttonsCount)
                        {
                            // iterate through each button in the current and next pages to determine if it should be enabled or not
                            for (int i = startButton; i < startButton + buttonsPerPage; i++)
                            {
                                if (i < adp.buttonsCount)
                                    currentMenu.transform.GetChild(i).gameObject.SetActive(true);
                            }
                        }
                    }
                    // if there is no following page left, ensure that the button is shown as disabled
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