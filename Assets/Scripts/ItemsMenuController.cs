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
    public GameObject itemsParent;


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

    private void OnTriggerExit(Collider other)
    {
        var colors = button.GetComponent<Button>().colors;
        colors.normalColor = Color.white;
        button.GetComponent<Button>().colors = colors;

        triggerOn = false;
    }

    void Start()
    {
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
        // if button is pressed and hand is touching the menu do an action
        if (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger) && triggerOn)
        {
            // do it only once when the button is pressed and after button was released at least once
            if (!buttonDown)
                buttonDown = true;
        }
        else
        {
            if (!triggerOn)
                buttonReleasedOnce = true;

            if (buttonDown && triggerOn && buttonReleasedOnce)
            {
                // if button type is category, change menu to the corresponding one
                if (typeOfButton == eTypeOfButton.Category)
                    ChangeMenu();

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
    }
    public void ChangeMenu()
    {
        // reset current and next menu's pages
        currMenu.TryGetComponent(out ActivateDisablePages currADP);
        newMenu.TryGetComponent(out ActivateDisablePages newADP);

        if (currADP != null)
        {
            currADP.currentPage = 0;
            SubmenusNavigate submenusNavigate = currMenu.GetComponentInChildren<SubmenusNavigate>();
            submenusNavigate.activateDisableButtons();
        }

        if (newADP != null)
        {
            newADP.currentPage = 0;
            SubmenusNavigate submenusNavigate = newMenu.GetComponentInChildren<SubmenusNavigate>();
            submenusNavigate.activateDisableButtons();
        }

        currMenu.SetActive(false);
        newMenu.SetActive(true);
    }

    // to instantiate the object that is passed according to the pressed button in the menu
    public void SpawnObject()
    {
        DefinePath.instance.itemsCount += 1;
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

        objectInstance.name += " " + DefinePath.instance.itemsCount.ToString();
        Vector3 handRotation = handController.transform.rotation.eulerAngles;
        Vector3 handPosition = handController.transform.position;
        // nom�s ens interessa la rotaci� de la y. +180 per qu� quedi com necessitem
        Vector3 handRoty = new Vector3(0.0f, handRotation.y, 0.0f);
        // sumem les dues rotacions
        objectInstance.transform.rotation = Quaternion.Euler(handRoty + rotationScale.rotation);
        objectInstance.transform.localScale = scale;
        // take local position from attachpont because we do not want to take it referent to the parent
        objectInstance.transform.position = new Vector3(handPosition.x, -attachPoint.y, handPosition.z);
        objectInstance.transform.position -= objectInstance.transform.forward * attachPoint.z;

        if (objectInstance.name.Contains("Crowd"))
            spawnCrowdChilds(objectInstance.transform);

        objectInstance.GetComponent<NetworkObject>().Spawn();
        objectInstance.transform.parent = itemsParent.transform;
        UDPSender.instance.sendItemMiddle(objectInstance.name, wrongName);
    }
    

    // to spawn in the network the GameObjects of the same type, groupped and instanced together such as the crowd of extras
    private void spawnCrowdChilds(Transform crowdGO)
    {
        for (int i = 0; i < crowdGO.childCount; i++)
        {
            GameObject currObject = crowdGO.GetChild(i).gameObject;
            currObject.TryGetComponent(out NetworkObject networkObject);

            if (networkObject != null)
                networkObject.Spawn();
        }
    }
}
