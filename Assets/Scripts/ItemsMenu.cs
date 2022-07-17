using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemsMenu : MonoBehaviour
{
    public GameObject Canvas;
    //[Header("Menus")]
    public GameObject ItemsMenu_object;
    RotationScale _shortcutAccessRotationScale;
    //public GameObject LightingMenu_object;
    //public GameObject PeopleMenu_object;
    //public GameObject PropsMenu_object;
    //public GameObject VFXMenu_object;

    //[Header("GameObjects")]
    //public GameObject FocusPrefab;

    // ESTARIA GUAI QUE ES POSI EL QUE HA CLICAT EL BOTÓ, PERÒ DE MOMENT EL FAREM PER DRETANS
    //[Header("Controllers")]
    public GameObject RightController;

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

    // Show or hide the current menu and Items menu
    public void change_MenuButton(GameObject curr_menu)
    {
        ItemsMenu_object.SetActive(!ItemsMenu_object.activeSelf);
        curr_menu.SetActive(!curr_menu.activeSelf);
    }

    // to instantiate the object that is passed according to the pressed button in the menu
    public void ObjectButton(GameObject prefab)
    {
        // access the script RotationScale in the prefab
        _shortcutAccessRotationScale = prefab.GetComponentInChildren<RotationScale>();
        Vector3 rotation = _shortcutAccessRotationScale.rotation;
        Vector3 scale = _shortcutAccessRotationScale.scale;

        // instantiate a new object in the hands position and looking forward
        GameObject object_instance = Instantiate(prefab);
        // the attach point always corresponds to the first child
        Transform attach_point = object_instance.transform.GetChild(0);

        object_instance.transform.localScale = scale;
        object_instance.transform.Rotate(rotation);
        // locate the object making the attach point to be on the hand
        Vector3 dif = object_instance.transform.position - attach_point.transform.position;
        object_instance.transform.position = RightController.transform.position;
        object_instance.transform.position += dif;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))// || Input.GetJoystickNames(Button.Three))
        {
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

            // if there were no active menus then set the items menu to active
            if (!any_active)
                Canvas.transform.GetChild(0).gameObject.SetActive(true);
        }
    }
}
