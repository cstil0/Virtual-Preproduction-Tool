using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
