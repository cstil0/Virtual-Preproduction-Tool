using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Unity.VisualScripting;

public class PointControlButtons : MonoBehaviour
{
    private bool triggerOn = false;
    private bool triggerButtonDown = false;
    private bool isItemSelectedOriginal = false;
    private bool isPointSelected = false;
    [SerializeField] eButtonType buttonType;

    [SerializeField] Button button;
    private GameObject point;
    private CameraRotationController cameraRotationController;
    private GameObject item;
    private int pointNum;

    private FollowPath followPath;
    private FollowPathCamera followPathCamera;
    private PathSpheresController pathSpheresController;
    enum eButtonType
    {
        TRASH,
        LEVEL
    }

    private void OnTriggerEnter(Collider other)
    {
        // check if collider corresponds to the hand
        if (other.gameObject.layer == 3)
        {
            // Change color to make hover effect
            var colors = button.colors;
            colors.normalColor = Color.blue;
            button.GetComponent<Button>().colors = colors;

            triggerOn = true;

            // change item's selected state to false to avoid creating a new point when pressing the button
            if (followPath != null)
            {
                followPath.isSelectedForPath = false;
            }

            if (followPathCamera != null){
                followPathCamera.isSelectedForPath = false;
            }
        }
    }

    // check if collider corresponds to the hand
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 3)
        {
            // Change color to make hover effect
            var colors = button.colors;
            colors.normalColor = Color.white;
            button.GetComponent<Button>().colors = colors;

            triggerOn = false;

            // change item's selected state back to its original state
            if (followPath != null)
                followPath.isSelectedForPath = followPath.isSelectedForPathOriginal;

            if (followPathCamera != null)
            {
                followPathCamera.isSelectedForPath = followPathCamera.isSelectedForPathOriginal;
            }
        }
    }

    void Start()
    {
        StartCoroutine(Init());
    }

    IEnumerator Init()
    {
        point = gameObject.transform.parent.parent.parent.gameObject;
        pathSpheresController = point.GetComponent<PathSpheresController>();

        // if point corresponds to character or object get the item's reference
        if (pathSpheresController != null)
            item = pathSpheresController.item;
        // if point corresponds to camera get the item and camera rotation controller references
        else
        {
            cameraRotationController = point.GetComponent<CameraRotationController>();
            string pathName = point.transform.parent.parent.gameObject.name;

            // wait until the correct path name has been parsed
            while (pathName.Contains("Clone"))
            {
                pathName = point.transform.parent.parent.gameObject.name;
                yield return null;
            }
            yield return 0;

            string[] splittedName = pathName.Split(" ");
            item = HoverObjects.instance.itemsParent.transform.Find(splittedName[1] + " " + splittedName[2]).gameObject;
        }

        var colors = button.GetComponent<Button>().colors;
        colors.normalColor = Color.white;
        button.GetComponent<Button>().colors = colors;

        // get corresponding follow path script
        if (item != null)
        {
            item.TryGetComponent(out followPath);
            item.TryGetComponent(out followPathCamera);
        }
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
            // action is performed once the button was released to avoid creating a new path point when destroying the current one
            if (triggerButtonDown && triggerOn)
            {
                if (buttonType == eButtonType.TRASH)
                    onTrashPressed();
                if (buttonType == eButtonType.LEVEL)
                    onLevelPressed();
            }

            triggerButtonDown = false;
        }
    }

    void onTrashPressed()
    {
        string[] splittedName = null;
        // delete point from the corresponding follow path script and change the item's state back to selected,
        // since ontrigger exit will not be called after destroying the sphere
        if (followPath != null)
        {
            splittedName = point.name.Split(" ");
            int pointNum = int.Parse(splittedName[1]);
            followPath.deletePathPoint(pointNum, true, true);
            followPath.isSelectedForPath = true;
        }
        else if (followPathCamera != null)
        {
            splittedName = point.transform.parent.name.Split(" ");
            int pointNum = int.Parse(splittedName[1]);
            followPathCamera.deletePathPoint(pointNum, true, true);
            followPathCamera.isSelectedForPath = true;
        }
    }

    void onLevelPressed()
    {
        // eliminate x and z rotation axis to ensure that the minicamera is leveled in the horizontal axis
        Vector3 oldRotation = point.transform.rotation.eulerAngles;
        Vector3 newRotation = new Vector3(0.0f, oldRotation.y, 0.0f);
        point.transform.rotation = Quaternion.Euler(newRotation);
        cameraRotationController.changePointRotation();
    }
}
