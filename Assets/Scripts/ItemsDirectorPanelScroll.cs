using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ItemsDirectorPanelScroll : MonoBehaviour
{
    public int shownItemsNum;
    private int totalItemsNum;
    private float buttonHeight;
    private float buttonSpacing;

    [SerializeField] float speed;
    [SerializeField] Scrollbar scrollBar;
    private RectTransform rectTransform;
    [SerializeField] RectTransform panelRectTransform;
    [SerializeField] Camera panelCamera;
    private float maxDisplacement;
    private float currDisplacement = 1;

    private Vector2 mouseLocalPoint;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        buttonHeight = gameObject.transform.GetChild(0).GetComponent<RectTransform>().rect.height;
        buttonSpacing = gameObject.GetComponent<VerticalLayoutGroup>().spacing;
        updateMaxDisplacement();
        scrollBar.value = 0;
    }

    void Update()
    {
        // get mouse coordinates from screen to local scroll rectangle coordinates
        RectTransformUtility.ScreenPointToLocalPointInRectangle(panelRectTransform, Input.mousePosition, panelCamera, out mouseLocalPoint);

        // if mouse scroll wheel is being moved up and mouse position is inside the items panel rectangle, perform scroll action
        if (Input.GetAxis("Mouse ScrollWheel") > 0f && panelRectTransform.rect.Contains(mouseLocalPoint))
        {
            // compute speed and corresponding displacement of the panel according to its size
            float tempScrollValue = scrollBar.value - speed * scrollBar.size;
            // check if we already reached the start of the panel
            if (tempScrollValue >= 0)
            {
                scrollBar.value = tempScrollValue;
                currDisplacement = Mathf.Lerp(1, maxDisplacement, scrollBar.value);
            }
            else
            {
                scrollBar.value = 0;
                currDisplacement = 1;
            }
            rectTransform.offsetMax = new Vector2(0f, currDisplacement);
        }

        // if mouse scroll wheel is being moved down and mouse position is inside the items panel rectangle, perform scroll action
        if (Input.GetAxis("Mouse ScrollWheel") < 0f && panelRectTransform.rect.Contains(mouseLocalPoint))
        {
            // compute speed and corresponding displacement of the panel according to its size
            float tempScrollValue = scrollBar.value + speed * scrollBar.size;
            if (tempScrollValue <= 1)
            {
                // check if we already reached the ending of the panel
                scrollBar.value = tempScrollValue;
                currDisplacement = Mathf.Lerp(1, maxDisplacement, scrollBar.value);
            }
            else
            {
                scrollBar.value = 1;
                currDisplacement = maxDisplacement;
            }
            rectTransform.offsetMax = new Vector2(0f, currDisplacement);
        }
    }

    // perform scroll action when scroll bar is moved
    public void scroll()
    {
        currDisplacement = Mathf.Lerp(1, maxDisplacement, scrollBar.value);
        rectTransform.offsetMax = new Vector2(0f, currDisplacement);
    }

    // compute the maximum displacement that the scroll can perform according to the number of buttons contained in the panel
    public void updateMaxDisplacement()
    {
        totalItemsNum = gameObject.transform.childCount;
        if (totalItemsNum < shownItemsNum)
            maxDisplacement = 1;
        else
            maxDisplacement = (totalItemsNum - shownItemsNum) * (buttonHeight + buttonSpacing);
        float barSize = (float)shownItemsNum / (float)totalItemsNum;
        scrollBar.size = barSize;
    }
}
