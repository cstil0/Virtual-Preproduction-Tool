using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActivateDisableMenu : MonoBehaviour
{
    public GameObject Canvas;
    //[Header("Menus")]
    //public GameObject ItemsMenu_object;
    RotationScale rotationScale;
    // since i am unable to use getKeyDown, only Get
    bool key_down = false;

    public GameObject debugPanelText;

    // start with all menus deactivated until user shows the items menu
    void Start()
    {
        int n_menus = Canvas.transform.childCount;
        for (int i = 0; i < n_menus; i++)
        {
            GameObject curr_child = Canvas.transform.GetChild(i).gameObject;
            curr_child.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        OVRInput.Update();
        if(OVRInput.Get(OVRInput.Button.PrimaryHandTrigger)){
            // do it only if it is the first time it is pressed
            if (!key_down)
            {
                key_down = true;
                Canvas.transform.GetChild(0).gameObject.SetActive(true);
            }
        }
        // once it is up again, set it to false
        else
        {
            key_down = false;
            int n_menus = Canvas.transform.childCount;
            bool any_active = false;
            // iterate through all child menus and set all to inactive
            for (int i = 0; i < n_menus; i++)
            {
                GameObject curr_child = Canvas.transform.GetChild(i).gameObject;
                // if active then at least one element is active
                any_active = curr_child.activeSelf || any_active;
                curr_child.SetActive(false);
            }
        }
    }


}
