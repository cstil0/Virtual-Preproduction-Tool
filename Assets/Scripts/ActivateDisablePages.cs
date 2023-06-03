using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateDisablePages : MonoBehaviour
{
    public int currentPage;
    public int buttonsCount;

    private void OnEnable()
    {
        currentPage = 0;
        buttonsCount = 0;

        // iterate through all the buttons on the current category to count them
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            Transform currButton = gameObject.transform.GetChild(i);
            if (currButton.name.Substring(0, 4) == "Item")
                buttonsCount += 1;
        }
    }

    void Start()
    {
        currentPage = 0;

        // enable only the buttons corresponding to the first page and disable the rest
        for (int i = 0; i < buttonsCount; i++)
        {
            if (i < 3)
            {
                gameObject.transform.GetChild(i).gameObject.SetActive(true);
            }
            else
            {
                gameObject.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }

    void Update()
    {
        
    }
}
