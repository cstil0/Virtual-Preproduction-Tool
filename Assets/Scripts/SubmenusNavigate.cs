using System.Collections;
using System.Collections.Generic;
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

        var colors = gameObject.GetComponent<Button>().colors;
        colors.normalColor = Color.white;
        gameObject.GetComponent<Button>().colors = colors;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Change color to make hover effect
        var colors = gameObject.GetComponent<Button>().colors;
        colors.normalColor = Color.blue;
        gameObject.GetComponent<Button>().colors = colors;

        triggerOn = true;
    }

    private void OnTriggerExit(Collider other)
    {
        var colors = gameObject.GetComponent<Button>().colors;
        colors.normalColor = Color.white;
        gameObject.GetComponent<Button>().colors = colors;

        triggerOn = false;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // MIRAR COM ES POT ESCURÇAR
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
            if (buttonDown && triggerOn && buttonReleasedOnce)
            {
                if (buttonType == ebuttonType.PREVIOUS && adp.currentPage - 1 >= 0)
                {
                    adp.currentPage--;

                    int startButton = adp.currentPage * 3;
                    for (int i = startButton; i < startButton + 3; i++)
                    {
                        if (i < adp.buttonsCount)
                        {
                            currentMenu.transform.GetChild(i).gameObject.SetActive(true);
                        }
                    }
                    //currentMenu.transform.GetChild(startButton - 1).gameObject.SetActive(false);
                    //currentMenu.transform.GetChild(startButton - 2).gameObject.SetActive(false);
                    //currentMenu.transform.GetChild(startButton - 3).gameObject.SetActive(false);

                    for (int i = startButton + 3; i < startButton + 6; i++)
                    {
                        if (i < adp.buttonsCount)
                        {
                            currentMenu.transform.GetChild(i).gameObject.SetActive(false);
                        }
                    }
                    //currentMenu.transform.GetChild(startButton + 1).gameObject.SetActive(true);
                    //currentMenu.transform.GetChild(startButton + 2).gameObject.SetActive(true);
                    //currentMenu.transform.GetChild(startButton + 3).gameObject.SetActive(true);
                }

                else if (buttonType == ebuttonType.NEXT && adp.currentPage + 1 < adp.buttonsCount)
                {
                    adp.currentPage++;

                    int startButton = adp.currentPage * 3;
                    if (adp.currentPage - 1 >= 0)
                    {
                        for (int i = startButton - 3; i < startButton; i++)
                        {
                            if (i < adp.buttonsCount)
                            {
                                currentMenu.transform.GetChild(i).gameObject.SetActive(false);
                            }
                        }
                        //currentMenu.transform.GetChild(startButton - 2).gameObject.SetActive(true);
                        //currentMenu.transform.GetChild(startButton - 3).gameObject.SetActive(true);
                    }

                    if (startButton < adp.buttonsCount)
                    {
                        for (int i = startButton; i < startButton + 3; i++)
                        {
                            if (i < adp.buttonsCount)
                            {
                                currentMenu.transform.GetChild(i).gameObject.SetActive(true);
                            }
                        }

                        //currentMenu.transform.GetChild(startButton + 1).gameObject.SetActive(false);
                        //currentMenu.transform.GetChild(startButton + 2).gameObject.SetActive(false);
                        //currentMenu.transform.GetChild(startButton + 3).gameObject.SetActive(false);
                    }
                }
                buttonDown = false;
            }
        }
        buttonReleasedOnce = true;
    }
}