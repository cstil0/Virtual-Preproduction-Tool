using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ItemsMenuController : MonoBehaviour
{
    public GameObject button;
    public GameObject handController;
    public GameObject debugPanelText;
    public GameObject canvas;
    RotationScale rotationScale;


    // SERIA GUAI PODER MOSTRAR LES VARIABLES SEGONS EL ENUM SELECCIONAT PERÒ S'HA DE CREAR UN NOU EDITOR I ÉS UNA MICA LIADA DE MOMENT
    [Header("Category Button Parameters")]
    // Used if the button type is category
    public GameObject currMenu;
    public GameObject newMenu;

    [Header("Object Button Parameters")]
    // Used if the button type is object
    public GameObject itemPrefab;
    public enum eTypeOfButton
    {
        Category,
        Object
    }
    public eTypeOfButton typeOfButton;

    // TREURE TRIGGER ON
    bool triggerOn;
    bool primaryButtonDown;
    bool secondaryButtonDown;
    bool buttonReleasedOnce;

    private void OnTriggerEnter(Collider other)
    {
        // Change color to make hover effect
        var colors = button.GetComponent<Button>().colors;
        colors.normalColor = Color.blue;
        button.GetComponent<Button>().colors = colors;

        triggerOn = true;
    }

    //// S'HA DE PROVAR SI FUNCIONA
    //private void OnTriggerStay(Collider other)
    //{
    //    OVRInput.Update();
    //    // if button is pressed while hand is touching the menu, instantiate the object
    //    if (OVRInput.Get(OVRInput.Button.SecondaryHandTrigger))
    //    {
    //        // do it only once when the button is pressed and after button was released at least once
    //        if (!buttonDown && buttonReleasedOnce)
    //        {
    //buttonDown = true;
    //// if button type is category, change menu to the corresponding one
    //if (typeOfButton == eTypeOfButton.Category)
    //{
    //    ItemsMenu itemsMenu = canvas.GetComponent<ItemsMenu>();
    //    itemsMenu.change_MenuButton(currMenuPanel, newMenuPanel);

    //    // HO HE POSAT A L'ENABLE PER TANT JA NO HAURIA DE FER FALTA
    //    //// change color to avoid having the hover one when comming back
    //    //var colors = button.GetComponent<Button>().colors;
    //    //colors.normalColor = Color.white;
    //    //button.GetComponent<Button>().colors = colors;
    //}

    //// if button type is objet, spawn the corresponding item
    //else if (typeOfButton == eTypeOfButton.Object)
    //{
    //    //isFirstTime = false;
    //    ItemsMenu itemsMenu = canvas.GetComponent<ItemsMenu>();
    //    itemsMenu.SpawnObject(itemPrefab, handController);
    //}
    //        }
    //    }
    //    else
    //    {
    //        buttonDown = false;
    //        buttonReleasedOnce = true;
    //    }
    //}

    private void OnTriggerExit(Collider other)
    {
        var colors = button.GetComponent<Button>().colors;
        colors.normalColor = Color.white;
        button.GetComponent<Button>().colors = colors;

        triggerOn = false;
    }

    void Start()
    {
        // POTSER NO ÉS NECESSARI FER-HO SI JA HO ESTÀ FENT EL ENABLE
        triggerOn = false;
        primaryButtonDown = false;
        secondaryButtonDown = false;
        buttonReleasedOnce = false;

        // start with all menus deactivated until user shows the items menu
        int n_menus = canvas.transform.childCount;
        for (int i = 0; i < n_menus; i++)
        {
            GameObject curr_child = canvas.transform.GetChild(i).gameObject;
            curr_child.SetActive(false);
        }
    }

    private void OnEnable()
    {
        triggerOn = false;
        primaryButtonDown = false;
        secondaryButtonDown = false;
        buttonReleasedOnce = false;

        var colors = button.GetComponent<Button>().colors;
        colors.normalColor = Color.white;
        button.GetComponent<Button>().colors = colors;
    }

    // Update is called once per frame
    void Update()
    {
        OVRInput.Update();
        if (OVRInput.Get(OVRInput.Button.PrimaryHandTrigger))
        {
            // do it only if it is the first time it is pressed
            if (!primaryButtonDown)
            {
                primaryButtonDown = true;
                canvas.transform.GetChild(0).gameObject.SetActive(true);
            }
            //if (OVRInput.GetDown(OVRInput.Button.One)) { 
            //if (Input.GetKeyDown(KeyCode.M)) || Input.GetJoystickNames(Button.Three))
            //{
        }
        // once it is up again, set it to false
        else
        {
            primaryButtonDown = false;
            int n_menus = canvas.transform.childCount;
            bool any_active = false;
            // iterate through all child menus and set all to inactive
            for (int i = 0; i < n_menus; i++)
            {
                GameObject curr_child = canvas.transform.GetChild(i).gameObject;
                // if active then at least one element is active
                any_active = curr_child.activeSelf || any_active;
                curr_child.SetActive(false);
            }
        }

        // CREC QUE ESTARIA BÉ POSAR LES DUES ACCIONS EN AQUEST MATEIX SCRIPT, PERÒ NO M'HE ATREVIT A CANVIAR-HO PER ARA JA QUE HE FET MOLTS CANVIS
        // TAMBÉ S'HA DE FER ENCARA EL CANVI DE MENUBUTTONACTION A AQUEST
        // if button is pressed and hand is touching the menu do an action
        if (OVRInput.Get(OVRInput.Button.SecondaryHandTrigger) && triggerOn)
        {
            // do it only once when the button is pressed and after button was released at least once
            if (!secondaryButtonDown && buttonReleasedOnce)
            {
                secondaryButtonDown = true;
                // if button type is category, change menu to the corresponding one
                if (typeOfButton == eTypeOfButton.Category)
                {
                    ChangeMenu();

                    // HO HE POSAT A L'ENABLE PER TANT JA NO HAURIA DE FER FALTA
                    //// change color to avoid having the hover one when comming back
                    //var colors = button.GetComponent<Button>().colors;
                    //colors.normalColor = Color.white;
                    //button.GetComponent<Button>().colors = colors;
                }

                // if button type is objet, spawn the corresponding item
                else if (typeOfButton == eTypeOfButton.Object)
                {
                    //isFirstTime = false;
                    SpawnObject(itemPrefab, handController);
                }

                // AIXÒ POTSER NO CAL
                buttonReleasedOnce = false;
            }
        }
        else
        {
            secondaryButtonDown  = false;
            buttonReleasedOnce = true;
        }
    }
    public void ChangeMenu()
    {
        currMenu.SetActive(false);
        newMenu.SetActive(true);
    }

    // to instantiate the object that is passed according to the pressed button in the menu
    public void SpawnObject(GameObject prefab, GameObject handController)
    {
        // access the script RotationScale in the prefab
        rotationScale = prefab.GetComponentInChildren<RotationScale>();
        Vector3 rotation = rotationScale.rotation;
        Vector3 scale = rotationScale.scale;

        GameObject objectInstance = Instantiate(prefab);
        objectInstance.transform.position = handController.transform.position;
        //objectInstance.transform.rotation = Quaternion.Euler(rotation);
        objectInstance.transform.localScale = scale;

    }
}
