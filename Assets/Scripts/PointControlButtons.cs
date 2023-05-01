using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using static UnityEditor.Progress;

public class PointControlButtons : MonoBehaviour
{
    private bool triggerOn = false;
    private bool triggerButtonDown = false;
    [SerializeField] eButtonType buttonType;

    [SerializeField] Button button;
    private GameObject point;
    private GameObject item;
    private int pointNum;

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
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        point = gameObject.transform.parent.parent.parent.gameObject;
        PathSpheresController spheresController = point.GetComponent<PathSpheresController>();
        item = spheresController.item;

        var colors = button.GetComponent<Button>().colors;
        colors.normalColor = Color.white;
        button.GetComponent<Button>().colors = colors;
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
        pointNum = int.Parse(splittedName[1]);

        item.TryGetComponent(out FollowPath followPath);
        item.TryGetComponent(out FollowPathCamera followPathCamera);

        if (followPath != null)
            followPath.deletePathPoint(pointNum);

        if (followPathCamera != null)
            followPathCamera.deletePathPoint(pointNum);
    }

    void onLevelPressed()
    {
        Vector3 newRotation = new Vector3(0.0f, point.transform.rotation.y, 0.0f);
        point.transform.rotation = Quaternion.Euler(newRotation);
    }
}
