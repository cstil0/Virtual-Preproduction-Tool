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

        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            Transform currButton = gameObject.transform.GetChild(i);
            if (currButton.name.Substring(0, 4) == "Item")
            {
                buttonsCount += 1;
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        currentPage = 0;

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

    // Update is called once per frame
    void Update()
    {
        
    }
}
