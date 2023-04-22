using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemsControlButtons : MonoBehaviour
{
    private bool isLocked = false;
    private bool triggerOn = false;
    private bool triggerButtonDown = false;

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

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 3)
        {
            // Change color to make hover effect
            var colors = button.colors;
            colors.normalColor = Color.blue;
            button.GetComponent<Button>().colors = colors;

            triggerOn = true;

            if (followPath != null)
                followPath.isSelectedForPath = false;

            if (followPathCamera != null)
                followPathCamera.isSelectedForPath = false;
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

            if (followPath != null)
                followPath.isSelectedForPath = true;

            if (followPathCamera != null)
                followPathCamera.isSelectedForPath = true;

        }
    }

    // Start is called before the first frame update
    void Start()
    {
        var colors = button.GetComponent<Button>().colors;
        colors.normalColor = Color.white;
        button.GetComponent<Button>().colors = colors;

        item = gameObject.transform.parent.parent.parent.gameObject;
        item.TryGetComponent(out customGrabbableCharacters);
        item.TryGetComponent(out ovrgrabbable);
        item.TryGetComponent(out followPath);
        item.TryGetComponent(out followPathCamera);
        item.TryGetComponent(out boxCollider);

        lastPosition = item.transform.position;
        if (buttonType == eButtonType.HEIGHT)
            heightText.text = lastPosition.y.ToString("#.00");
    }

    // Update is called once per frame
    void Update()
    {
        if (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger) && triggerOn)
        {
            if (triggerButtonDown == false)
            {
                if (buttonType == eButtonType.TRASH)
                    onTrashPressed();
                if (buttonType == eButtonType.LOCK)
                    onLockPressed();

                triggerButtonDown = true;
            }
        }
        else
            triggerButtonDown = false;

        if (lastPosition != item.transform.position && buttonType == eButtonType.HEIGHT)
        {
            lastPosition = item.transform.position;
            heightText.text = lastPosition.y.ToString("#.00");
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

        if (circlesContainer != null)
        {
            for (int i = pathContainer.transform.childCount - 1; i >= 0; i++)
            {
                Destroy(pathContainer.transform.GetChild(i).gameObject);
                Destroy(circlesContainer.transform.GetChild(i).gameObject);
            }
            Destroy(pathContainer);
            Destroy(circlesContainer);
        }
        else if (pathContainer != null)
        {
            for (int i = pathContainer.transform.childCount - 1; i >= 0; i++)
            {
                Destroy(pathContainer.transform.GetChild(i).gameObject); 
            }
            Destroy(pathContainer);
        }

        Destroy(item);

        UDPSender.instance.sendDeleteItemToDirector(itemName);
    }

    public void onLockPressed()
    {
        if (isLocked)
            buttonImage.sprite = lockImage;
        else
            buttonImage.sprite = unlockImage;

        if (customGrabbableCharacters != null)
            customGrabbableCharacters.enabled = isLocked;
        if (ovrgrabbable != null)
        {
            ovrgrabbable.enabled = isLocked;
            boxCollider.enabled = isLocked;
        }

        isLocked = !isLocked;
    }
}
