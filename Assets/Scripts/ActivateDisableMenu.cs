using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ActivateDisableMenu : MonoBehaviour
{
    public GameObject Canvas;
    RotationScale rotationScale;
    // needed to know if key was pressed only at the first frame
    bool key_down = false;

    [SerializeField] eMenuType menuType;

    enum eMenuType
    {
        ITEMS_MENU,
        CONTROLLERS_MAP
    }

    void Start()
    {
        // start with all menus disabled until user shows the items menu
        int n_menus = Canvas.transform.childCount;
        for (int i = 0; i < n_menus; i++)
        {
            GameObject curr_child = Canvas.transform.GetChild(i).gameObject;
            curr_child.SetActive(false);
        }
    }

    void Update()
    {
        OVRInput.Update();
        if (menuType == eMenuType.ITEMS_MENU)
        {
            if(OVRInput.Get(OVRInput.Button.PrimaryHandTrigger)){
                // do it only if it is the first time it is pressed
                // this is needed because I could not make getKeyDown method to work
                if (!key_down)
                {
                    key_down = true;
                    Canvas.transform.GetChild(0).gameObject.SetActive(true);
                }
            }
            else
            {
                // once it is up again, set it to false
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

        // show controllers map using the same logic as the items menu
        else if (menuType == eMenuType.CONTROLLERS_MAP)
        {
            if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger))
            {
                if (!key_down)
                {
                    key_down = true;
                    Canvas.transform.GetChild(0).gameObject.SetActive(true);
                }
            }
            else
            {
                if (key_down)
                {
                    Canvas.transform.GetChild(0).gameObject.SetActive(false);
                    key_down = false;
                }

            }
        }
    }


}
