using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ItemsMenuController : MonoBehaviour
{
    public GameObject button;
    public GameObject handController;
    public GameObject canvas;
    RotationScale rotationScale;
    private int itemsCount = 0;
    public GameObject itemsParent;


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
    bool buttonDown;
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
        buttonDown = false;
        buttonReleasedOnce = false;
    }

    private void OnEnable()
    {
        triggerOn = false;
        buttonDown = false;
        buttonReleasedOnce = false;

        var colors = button.GetComponent<Button>().colors;
        colors.normalColor = Color.white;
        button.GetComponent<Button>().colors = colors;
    }

    // Update is called once per frame
    void Update()
    {
        // CREC QUE ESTARIA BÉ POSAR LES DUES ACCIONS EN AQUEST MATEIX SCRIPT, PERÒ NO M'HE ATREVIT A CANVIAR-HO PER ARA JA QUE HE FET MOLTS CANVIS
        // TAMBÉ S'HA DE FER ENCARA EL CANVI DE MENUBUTTONACTION A AQUEST
        // if button is pressed and hand is touching the menu do an action
        if (OVRInput.Get(OVRInput.Button.SecondaryHandTrigger))
        {
            //    // do it only once when the button is pressed and after button was released at least once
            //    if (!buttonDown && buttonReleasedOnce)
            //    {
            //        buttonDown = true;
            //        // if button type is category, change menu to the corresponding one
            //        if (typeOfButton == eTypeOfButton.Category)
            //        {
            //            ChangeMenu();

            //            // HO HE POSAT A L'ENABLE PER TANT JA NO HAURIA DE FER FALTA
            //            //// change color to avoid having the hover one when comming back
            //            //var colors = button.GetComponent<Button>().colors;
            //            //colors.normalColor = Color.white;
            //            //button.GetComponent<Button>().colors = colors;
            //        }

            //        // if button type is objet, spawn the corresponding item
            //        else if (typeOfButton == eTypeOfButton.Object)
            //        {
            //            //isFirstTime = false;
            //            SpawnObject();
            //        }

            //        // AIXÒ POTSER NO CAL
            //        buttonReleasedOnce = false;
            //    }
            //}
            //else
            //{
            //    buttonDown  = false;
            //    buttonReleasedOnce = true;
            //}
            // do it only once when the button is pressed and after button was released at least once
            if (!buttonDown)
            {
                buttonDown = true;
            }
        }
        else
        {
            if (buttonDown && triggerOn && buttonReleasedOnce)
            {
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
                    // we need a try/catch since if we are debugging and host or client are not connected it will rise an error and
                    // multiple characters appear since button down never gets false
                    try
                    {
                        SpawnObject();
                    }
                    catch (Exception e){
                        Debug.Log("Spawn Error: " + e.Message);
                    }
                }
            }
            buttonDown = false;
        }
        buttonReleasedOnce = true;


    }
    public void ChangeMenu()
    {
        currMenu.SetActive(false);
        newMenu.SetActive(true);
    }

    // to instantiate the object that is passed according to the pressed button in the menu
    public void SpawnObject()
    {
        itemsCount += 1;
        Vector3 attachPoint = itemPrefab.transform.GetChild(0).localPosition;
        // access the script RotationScale in the prefab
        rotationScale = itemPrefab.GetComponentInChildren<RotationScale>();
        Vector3 scale = new Vector3(rotationScale.scale, rotationScale.scale, rotationScale.scale);

        GameObject objectInstance = Instantiate(itemPrefab);
        string wrongName = objectInstance.name;
        if (wrongName.Contains("(Clone)"))
        {
            string[] splittedName = objectInstance.name.Split("(Clone)");
            objectInstance.name = splittedName[0];
        }

        objectInstance.name += itemsCount.ToString();
        Vector3 handRotation = handController.transform.rotation.eulerAngles;
        Vector3 handPosition = handController.transform.position;
        // només ens interessa la rotació de la y. +180 per què quedi com necessitem
        Vector3 handRoty = new Vector3(0.0f, handRotation.y, 0.0f);
        //objectInstance.transform.position = handController.transform.position;
        // sumem les dues rotacions
        objectInstance.transform.rotation = Quaternion.Euler(handRoty + rotationScale.rotation);
        objectInstance.transform.localScale = scale;
        // take local position from attachpont because we do not want to take it referent to the parent
        objectInstance.transform.position = new Vector3(handPosition.x, -attachPoint.y, handPosition.z) /*+ rotationScale.rotation*/;
        objectInstance.transform.position -= objectInstance.transform.forward * attachPoint.z;
        //objectInstance.transform.rotation = Quaternion.Euler(rotation);
        // translate trasllada desde la posició a la que estem tantes unitats
        //objectInstance.transform.Translate(-attachPoint, handController.transform);

        objectInstance.GetComponent<NetworkObject>().Spawn();
        objectInstance.transform.parent = itemsParent.transform;
        UDPSender.instance.sendItemMiddle(objectInstance.name, wrongName);
    }
}
