using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    
    // Start is called before the first frame update
    void Start()
    {
        triggerOn = false;
        menusPanel = gameObject.transform.GetChild(0);
    }

    // Update is called once per frame
    void Update()
    {
        float speed = 1.5f;
        if (OVRInput.Get(OVRInput.Button.SecondaryHandTrigger) && triggerOn){
            ScrollRect scrollRect = gameObject.GetComponent<ScrollRect>();
            RectTransform rectTrans = gameObject.GetComponent<RectTransform>();
            Vector3 localHandPos = gameObject.transform.InverseTransformPoint(handController.transform.position);
            float panelBottom = gameObject.transform.position.y - rectTrans.rect.height;
            float distance = -localHandPos.y;
            float scrollPos = Mathf.InverseLerp(0.0f, rectTrans.rect.height, distance*speed);
            scrollRect.verticalNormalizedPosition = scrollPos;
        }

        //gameObject.GetComponent<ScrollRect>().verticalNormalizedPosition = 1;

        // ESTARÍA BIEN HACER ESTO SOLO CUANDO SE CAMBIE DE MENÚ
        for (int i = 0; i < menusPanel.childCount; i++)
        {
            GameObject currMenu = menusPanel.GetChild(i).gameObject;
            if (!currMenu.active)
                continue;

            RectTransform rt = menusPanel.GetComponent<RectTransform>();
            float newHeight = currMenu.transform.childCount * 18.5f;
            rt.sizeDelta = new Vector2(rt.rect.width, newHeight);
        }
    }
}
