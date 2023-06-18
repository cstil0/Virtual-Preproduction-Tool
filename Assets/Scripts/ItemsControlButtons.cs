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
    private Collider grabPoint;

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

        // if the item has an ovrgrabbable script, get its grab point, since we will remove it to disable grabbing action when locked
        if (ovrgrabbable != null)
            grabPoint = ovrgrabbable.grabPoints[0];

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
        DefinePath.instance.deleteItem(item, true);
    }

    public void onLockPressed()
    {
        isLocked = !isLocked;

        // change button shape according to the new state
        if (isLocked)
            buttonImage.sprite = lockImage;
        else
            buttonImage.sprite = unlockImage;

        // disable custom grabbable to avoid grabbing the character
        if (customGrabbableCharacters != null)
            customGrabbableCharacters.enabled = !isLocked;

        // if the item contains an ovrgrabbable script, remove its grab point when locked to disable it
        if (ovrgrabbable != null && !isLocked)
            ovrgrabbable.removeGrabPoint();
        // when unlocking, set its grab point back to enable it againg
        else if (grabPoint != null && isLocked)
            ovrgrabbable.setGrabPoint(grabPoint);

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
