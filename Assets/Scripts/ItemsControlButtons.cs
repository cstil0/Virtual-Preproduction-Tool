using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ItemsControlButtons : MonoBehaviour
{
    private bool isLocked = false;
    private bool triggerOn = false;
    private bool triggerButtonDown = false;
    private bool isItemSelectedOriginal = false;

    [SerializeField] Button button;
    [SerializeField] Image buttonImage;
    [SerializeField] eButtonType buttonType;
    [SerializeField] Sprite lockImage;
    [SerializeField] Sprite unlockImage;
    [SerializeField] TextMeshProUGUI heightText;

    private GameObject item;
    private CustomGrabbableCharacters customGrabbableCharacters;
    private OVRGrabbable ovrgrabbable;
    private FollowPath followPath;
    private FollowPath followPathCamera;
    private BoxCollider boxCollider;

    private Vector3 lastPosition;

    enum eButtonType
    {
        TRASH,
        LOCK,
        HEIGHT
    }

    private void OnEnable()
    {
        DirectorPanelManager.instance.OnHideShowGrid += showHideHeight;
    }
    private void OnDisable()
    {
        DirectorPanelManager.instance.OnHideShowGrid -= showHideHeight;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 3)
        {
            // Change color to make hover effect
            var colors = button.colors;
            colors.normalColor = Color.blue;
            button.GetComponent<Button>().colors = colors;

            triggerOn = true;
            // get corresponding follow path script and set its selected property to false to avoid defining a new point when pressing the button
            if (followPath != null)
            {
                followPath.isSelectedForPath = false;
            }

            if (followPathCamera != null)
            {
                followPathCamera.isSelectedForPath = false;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 3)
        {
            // Change color to make hover effect
            var colors = button.colors;
            colors.normalColor = Color.white;
            button.GetComponent<Button>().colors = colors;

            triggerOn = false;

            // get corresponding follow path script and set its selected property back to its original state
            if (followPath != null)
                followPath.isSelectedForPath = followPath.isSelectedForPathOriginal;

            if (followPathCamera != null)
                followPathCamera.isSelectedForPath = followPathCamera.isSelectedForPathOriginal;
        }
    }

    void Start()
    {  
        // get the corresponding scripts and components if they are attached to the item
        item = gameObject.transform.parent.parent.parent.gameObject;
        item.TryGetComponent(out customGrabbableCharacters);
        item.TryGetComponent(out ovrgrabbable);
        item.TryGetComponent(out followPath);
        item.TryGetComponent(out followPathCamera);
        item.TryGetComponent(out boxCollider);

        if (buttonType == eButtonType.HEIGHT)
        {
            lastPosition = item.transform.position;
            heightText.text = lastPosition.y.ToString("#0.00");
        }
        // ensure the correct color in the button
        else
        {
            var colors = button.GetComponent<Button>().colors;
            colors.normalColor = Color.white;
            button.GetComponent<Button>().colors = colors;
        }

        // show height reference if grid is also enabled
        showHideHeight(DirectorPanelManager.instance.isGridShown);
    }

    void Update()
    {
        if (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger) && triggerOn)
        {
            if (triggerButtonDown == false)
                triggerButtonDown = true;
        }
        else
        {
            // perform the corresponding action
            if (triggerButtonDown && triggerOn)
            {
                if (buttonType == eButtonType.TRASH)
                    onTrashPressed();
                if (buttonType == eButtonType.LOCK)
                    onLockPressed();
            }

            triggerButtonDown = false;
        }

        if (buttonType == eButtonType.HEIGHT)
        {
            // change height reference if the item's position changed
            if(lastPosition != item.transform.position)
            {
                lastPosition = item.transform.position;
                heightText.text = lastPosition.y.ToString("#0.00");
            }
        }
    }

    public void onTrashPressed()
    {
        // get parent item to destroy it
        string itemName = item.name;
        string[] splittedName = itemName.Split(" ");
        string itemNum = splittedName[1];
        GameObject pathContainer = GameObject.Find("Path " + itemNum);
        GameObject circlesContainer = GameObject.Find("Circles " + itemNum);

        // destroy the corresponding points and circles as well as the whole containers if it is a character
        if (circlesContainer != null)
        {
            for (int i = pathContainer.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(pathContainer.transform.GetChild(i).gameObject);
                // circles have one less child in the circles container than path container, since those also store the line renderer
                if (i < pathContainer.transform.childCount - 1)
                    Destroy(circlesContainer.transform.GetChild(i).gameObject);
            }
            Destroy(pathContainer);
            Destroy(circlesContainer);
        }

        // destroy the points and path container if it is a camera
        else if (pathContainer != null)
        {
            for (int i = pathContainer.transform.childCount - 1; i >= 0; i++)
            {
                Destroy(pathContainer.transform.GetChild(i).gameObject); 
            }
            Destroy(pathContainer);
        }

        // destroy the item and its references
        Destroy(item);

        if (followPath != null)
            followPath.isSelectedForPath = true;

        if (followPathCamera != null)
            followPathCamera.isSelectedForPath = true;

        HoverObjects.instance.currentItemCollider = null;
        HoverObjects.instance.itemAlreadySelected = false;

        UDPSender.instance.sendDeleteItemToDirector(itemName);
    }

    public void onLockPressed()
    {
        // change button shape according to the new state
        if (isLocked)
            buttonImage.sprite = lockImage;
        else
            buttonImage.sprite = unlockImage;

        // disable grabbable to avoid grabbing the item
        if (customGrabbableCharacters != null)
            customGrabbableCharacters.enabled = isLocked;
        if (ovrgrabbable != null)
        {
            ovrgrabbable.enabled = isLocked;
            boxCollider.enabled = isLocked;
        }

        isLocked = !isLocked;
    }

    public void showHideHeight(bool isGridShown)
    {
        // enable numerical height reference
        if (buttonType == eButtonType.HEIGHT)
        {
            GameObject heightGO = heightText.transform.parent.gameObject;

            heightText.enabled = isGridShown;
            heightGO.GetComponent<Image>().enabled = isGridShown;
        }
    }
}
