using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// this script is not used anymore, as it was created to implement the item's scroll menu in VR which is now handled by pages
public class ScrollMenu : MonoBehaviour
{
    bool triggerOn;
    public GameObject handController;
    Transform menusPanel;
    public float ButtonSize = 18.5f;


    private void OnTriggerEnter(Collider other)
    {
        triggerOn = true;   
    }

    private void OnTriggerExit(Collider other)
    {
        triggerOn = false;
    }
    
    void Start()
    {
        triggerOn = false;
        menusPanel = gameObject.transform.GetChild(0);
    }

    void Update()
    {
        float speed = 1.5f;
        // compute the new scroll position that should be applied to the menu according to the user's input
        if (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger) && triggerOn){
            ScrollRect scrollRect = gameObject.GetComponent<ScrollRect>();
            RectTransform rectTrans = gameObject.GetComponent<RectTransform>();
            Vector3 localHandPos = gameObject.transform.InverseTransformPoint(handController.transform.position);
            float distance = -localHandPos.y;
            float scrollPos = Mathf.InverseLerp(0.0f, rectTrans.rect.height, distance*speed);
            scrollRect.verticalNormalizedPosition = scrollPos;
        }

        // iterate through all menus and get only the active one to apply the scroll
        for (int i = 0; i < menusPanel.childCount; i++)
        {
            GameObject currMenu = menusPanel.GetChild(i).gameObject;
            if (!currMenu.active)
                continue;

            // change the y axis position for the menu according to the scroll that has to be applied
            RectTransform menus_rt = menusPanel.GetComponent<RectTransform>();
            RectTransform currPanel_rt = currMenu.GetComponent<RectTransform>();
            float newHeight = currMenu.transform.childCount * 18.5f;
            float newPosY = 4.5f - 0.33f * (74.0f - newHeight);
            currPanel_rt.anchoredPosition = new Vector3(currPanel_rt.position.x, newPosY, currPanel_rt.position.z);
            menus_rt.sizeDelta = new Vector2(menus_rt.rect.width, newHeight);
        }
    }
}
