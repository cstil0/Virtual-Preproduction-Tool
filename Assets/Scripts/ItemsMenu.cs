using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemsMenu : MonoBehaviour
{
    public GameObject Canvas;
    //[Header("Menus")]
    //public GameObject ItemsMenu_object;
    RotationScale rotationScale;
    // since i am unable to use getKeyDown, only Get
    bool key_down = false;

    //public GameObject LightingMenu_object;
    //public GameObject PeopleMenu_object;
    //public GameObject PropsMenu_object;
    //public GameObject VFXMenu_object;

    //[Header("GameObjects")]
    //public GameObject FocusPrefab;

    // ESTARIA GUAI QUE ES POSI EL QUE HA CLICAT EL BOTÓ, PERÒ DE MOMENT EL FAREM PER DRETANS
    //[Header("Controllers")]
    public GameObject RightController;
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

    // Show or hide the current menu and Items menu
    public void change_MenuButton(GameObject curr_menu, GameObject new_menu)
    {
        curr_menu.SetActive(false);
        new_menu.SetActive(true);
    }

    // to instantiate the object that is passed according to the pressed button in the menu
    public void SpawnObject(GameObject prefab, GameObject handController)
    {
        // access the script RotationScale in the prefab
        rotationScale = prefab.GetComponentInChildren<RotationScale>();
        Vector3 rotation = rotationScale.rotation;
        Vector3 scale = rotationScale.scale;

        GameObject objectInstance = Instantiate(prefab);
        objectInstance.transform.position = RightController.transform.position;
        //objectInstance.transform.rotation = Quaternion.Euler(rotation);
        objectInstance.transform.localScale = scale;

        //// instantiate a new object in the hands position and looking forward
        //GameObject object_instance = Instantiate(prefab);
        //// the attach point always corresponds to the first child
        //Transform attach_point = object_instance.transform.GetChild(0);


        //object_instance.transform.localScale = scale;

        //// locate the object making the attach point to be on the hand
        ////object_instance.transform.position = handController.transform.position;


        ////object_instance.transform.position = handController.transform.position;
        //Vector3 dif = attach_point.position - handController.transform.position;

        //string debugText = "DIF: " + dif + " CON: " + handController.transform.position;
        ////object_instance.transform.Translate(dif, handController.transform);

        //Vector3 finalPos = new Vector3(handController.transform.forward + dif.x, handController.transform.up + dif.y, handController.transform.right + dif.z);
        //object_instance.transform.position += handController.t
        //debugText += " OB: " + object_instance.transform.position;
        //Text textComponent = debugPanelText.GetComponent<Text>();
        //textComponent.text = debugText;



        //object_instance.transform.rotation = Quaternion.Euler(rotation);//+ handController.transform.rotation.eulerAngles);

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
            //if (OVRInput.GetDown(OVRInput.Button.One)) { 
            //if (Input.GetKeyDown(KeyCode.M)) || Input.GetJoystickNames(Button.Three))
            //{
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
