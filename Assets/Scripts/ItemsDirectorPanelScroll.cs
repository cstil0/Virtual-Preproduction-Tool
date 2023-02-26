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
    private Vector2 lastMousePos;

    private bool isPressDown;
    private bool isPressUp;
    private bool isPressConfirm;


    // Start is called before the first frame update
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        buttonHeight = gameObject.transform.GetChild(0).GetComponent<RectTransform>().rect.height;
        buttonSpacing = gameObject.GetComponent<VerticalLayoutGroup>().spacing;
        updateMaxDisplacement();
        scrollBar.value = 0;
    }

    // Update is called once per frame
    void Update()
    {
        //if (isPressUp) VerticalMovement = 1;
        //if (isPressDown) VerticalMovement = -1;
        //if (!isPressUp && !isPressDown) VerticalMovement = 0;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(panelRectTransform, Input.mousePosition, panelCamera, out mouseLocalPoint);

        if (Input.GetAxis("Mouse ScrollWheel") > 0f && panelRectTransform.rect.Contains(mouseLocalPoint))
        {
            float tempScrollValue = scrollBar.value - speed * scrollBar.size;
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
        if (Input.GetAxis("Mouse ScrollWheel") < 0f && panelRectTransform.rect.Contains(mouseLocalPoint))
        {
            float tempScrollValue = scrollBar.value + speed * scrollBar.size;
            if (tempScrollValue <= 1)
            {
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

    public void scroll()
    {
        currDisplacement = Mathf.Lerp(1, maxDisplacement, scrollBar.value);
        rectTransform.offsetMax = new Vector2(0f, currDisplacement);
    }

    void updateMaxDisplacement()
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
