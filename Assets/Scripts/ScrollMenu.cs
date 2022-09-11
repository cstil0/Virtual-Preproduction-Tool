using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollMenu : MonoBehaviour
{
    bool triggerOn;
    public GameObject handController;

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
    }

    // Update is called once per frame
    void Update()
    {
        if (OVRInput.Get(OVRInput.Button.SecondaryHandTrigger) && triggerOn){
            ScrollRect scrollRect = gameObject.GetComponent<ScrollRect>();
            RectTransform rectTrans = gameObject.GetComponent<RectTransform>();
            Vector3 localHandPos = gameObject.transform.InverseTransformPoint(handController.transform.position);
            float panelBottom = gameObject.transform.position.y - rectTrans.rect.height;
            float distance = -localHandPos.y;
            float scrollPos = Mathf.InverseLerp(0.0f, rectTrans.rect.height, distance);
            scrollRect.verticalNormalizedPosition = scrollPos;
        }

        gameObject.GetComponent<ScrollRect>().verticalNormalizedPosition = 0; ;


    }
}
