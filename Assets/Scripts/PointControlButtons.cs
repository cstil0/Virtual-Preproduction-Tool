using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;

public class PointControlButtons : MonoBehaviour
{
    private bool triggerOn = false;
    private bool triggerButtonDown = false;
    [SerializeField] eButtonType buttonType;

    [SerializeField] Button button;
    private GameObject point;
    private CameraRotationController cameraRotationController;
    private GameObject item;
    private int pointNum;

    private FollowPath followPath;
    private FollowPathCamera followPathCamera;
    enum eButtonType
    {
        TRASH,
        LEVEL
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
            Debug.Log("DISABLING FROM POINTCONTROLBUTTONS");
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

        Debug.Log("ENABLING FROM POINTCONTROLBUTTONS");
    }

    // Start is called before the first frame update
    void Start()
    {
        point = gameObject.transform.parent.parent.parent.gameObject;
        PathSpheresController spheresController = point.GetComponent<PathSpheresController>();

        // if point corresponds to item
        if (spheresController != null)
            item = spheresController.item;
        // if point corresponds to camera
        else
        {
            cameraRotationController = point.GetComponent<CameraRotationController>();
            string pathName = point.transform.parent.parent.gameObject.name;
            string[] splittedName = pathName.Split(" ");
            item = HoverObjects.instance.itemsParent.transform.Find(splittedName[1] + " " + splittedName[2]).gameObject;
        }

        var colors = button.GetComponent<Button>().colors;
        colors.normalColor = Color.white;
        button.GetComponent<Button>().colors = colors;

        //StartCoroutine(assignFollowPath());

        if (item != null)
        {
            item.TryGetComponent(out followPath);
            item.TryGetComponent(out followPathCamera);
        }
    }

    //IEnumerator assignFollowPath()
    //{
    //    while (item == null) yield return null;

    //    item.TryGetComponent(out followPath);
    //    item.TryGetComponent(out followPathCamera);
    //}

    // Update is called once per frame
    void Update()
    {
        if (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger) && triggerOn)
        {
            if (triggerButtonDown == false)
            {
                if (buttonType == eButtonType.TRASH)
                    onTrashPressed();
                if (buttonType == eButtonType.LEVEL)
                    onLevelPressed();

                triggerButtonDown = true;
            }
        }
        else
            triggerButtonDown = false;
    }

    void onTrashPressed()
    {
        string[] splittedName = point.name.Split(" ");
        // if normal point
        if (cameraRotationController == null)
            pointNum = int.Parse(splittedName[1]);
        // if point from camera
        else
            pointNum = int.Parse(splittedName[2]);

        if (followPath != null)
            followPath.deletePathPoint(pointNum);

        if (followPathCamera != null)
            followPathCamera.deletePathPoint(pointNum);
    }

    void onLevelPressed()
    {
        Vector3 oldRotation = point.transform.rotation.eulerAngles;
        Vector3 newRotation = new Vector3(0.0f, oldRotation.y, 0.0f);
        point.transform.rotation = Quaternion.Euler(newRotation);
        cameraRotationController.changePointRotation();
    }
}
