using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// this script handles color change for some panel buttons in which we do not want to mantain the selected color when the pointer exits the button although it was pressed
public class ButtonExitControl : MonoBehaviour, IPointerExitHandler, IPointerEnterHandler
{
    [SerializeField] Button button;

    public void OnPointerEnter(PointerEventData eventData)
    {
        ColorBlock buttonColors = button.colors;
        buttonColors.selectedColor = ItemsDirectorPanelController.instance.selectedColor;
        button.colors = buttonColors;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ColorBlock buttonColors = button.colors;
        buttonColors.selectedColor = ItemsDirectorPanelController.instance.normalColor;
        button.colors = buttonColors;
    }

    void Start()
    {
    }

    void Update()
    {
        
    }
}
