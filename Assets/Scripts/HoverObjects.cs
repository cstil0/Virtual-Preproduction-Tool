using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static HoverObjects;

public class HoverObjects : MonoBehaviour
{
    public static HoverObjects instance = null;

    bool alreadyTriggered;
    public bool itemAlreadySelected = false;
    public bool pointAlreadySelected = false;
    public bool miniCameraAlreadySelected = false;

    //bool alreadySelectedForPath;
    public GameObject currentItemCollider;
    public GameObject currentPointCollider;
    public GameObject currentMiniCameraCollider;
    public GameObject currentSelectedForPath;

    [SerializeField] GameObject itemsParent;

    public delegate void PathPointHovered(int pathNum, int pointNum, Color color);
    public event PathPointHovered OnPathPointHovered;

    private void Awake()
    {
        if (instance)
        {
            if (instance != this)
                Destroy(gameObject);
        }
        else
            instance = this;
    }

    // recursive function that iterates through all materials of the tree and changes their color
    public void changeColorMaterials(GameObject currentParent, Color color)
    {
        Renderer renderer = currentParent.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material parentMaterial = renderer.material;
            parentMaterial.color = color;
        }

        for (int i = 0; i < currentParent.transform.childCount; i++)
        {
            GameObject currChild = currentParent.transform.GetChild(i).gameObject;
            
            // not all childs have materials, so this is why we need to catch errors
            try
            {
                // get all materials and change their color
                renderer = currChild.GetComponent<Renderer>();
                Material[] materials = renderer.materials;

                foreach (Material material in materials)
                {
                    // we do not want to change the color of the number above the cameras
                    if (!material.name.Contains("CameraNum"))
                        material.color = color;
                }
            }
            catch (System.Exception e){}

            // recursive call to check also for childs
            if (currChild.transform.childCount > 0)
                changeColorMaterials(currChild, color);
        }
    }

    private void deselectAllItems()
    {
        for (int i = 0; i < itemsParent.transform.childCount; i++)
        {
            GameObject currItem = itemsParent.transform.GetChild(i).gameObject;

            if (currItem == currentSelectedForPath)
                continue;

            currItem.TryGetComponent(out FollowPath followPath);
            currItem.TryGetComponent(out ObjectsSelector objectsSelector);
            currItem.TryGetComponent(out FollowPathCamera followPathCamera);

            if (followPath != null)
            {
                if (followPath.isSelectedForPath)
                {
                    followPath.isSelectedForPath = false;
                    changeColorMaterials(currItem, Color.white);

                }
            }

            if (objectsSelector != null)
            {
                if (objectsSelector.isSelected)
                {
                    objectsSelector.isSelected = false;
                    changeColorMaterials(currItem, Color.white);
                }
            }

            if (followPathCamera != null)
            {
                if (followPathCamera.isSelectedForPath)
                {
                    followPathCamera.isSelectedForPath = false;
                    changeColorMaterials(currItem, Color.black);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // change the color only to the first object that collided with the controller, only if it is an item
        if (!alreadyTriggered && (other.gameObject.layer == 10 || other.gameObject.layer == 7))
        {
            // check if it was selected to set the correct color
            bool isSelected = false;
            currentItemCollider = other.gameObject;
            if (other.gameObject.layer == 10)
            {
                FollowPath followPath = other.gameObject.GetComponent<FollowPath>();
                ObjectsSelector objectsSelector = other.gameObject.GetComponent<ObjectsSelector>();
                if (followPath != null)
                {
                    followPath.triggerOn = true;
                    isSelected = followPath.isSelectedForPath;
                }

                if (objectsSelector != null)
                {
                    objectsSelector.triggerOn = true;
                    isSelected = objectsSelector.isSelected;
                }
            }
            else if (other.gameObject.layer == 7)
            {
                FollowPathCamera followPath = other.gameObject.GetComponent<FollowPathCamera>();
                if (followPath != null)
                {
                    followPath.triggerOn = true;
                    isSelected = followPath.isSelectedForPath;
                }
            }


            if (!isSelected)
            {
                alreadyTriggered = true;
                changeColorMaterials(currentItemCollider, UnityEngine.Color.blue);

                // if the object has a limit rotation script mark it as selected
                try
                {
                    currentItemCollider.GetComponent<CustomGrabbableCharacters>().objectSelected(gameObject, true);
                }
                catch (System.Exception e) { }

                // if it is camera, change the one that will send UDP
                if (other.gameObject.layer == 7)
                {
                    UDPSender.instance.screenCamera = other.gameObject.GetComponent<Camera>();
                    UDPSender.instance.sendChangeCamera();
                }
            }
        }

        // if sphere path point
        if (!pointAlreadySelected && (other.gameObject.layer == 14))
        {
            pointAlreadySelected = true;
            currentPointCollider = other.gameObject;
            changeColorMaterials(currentPointCollider, Color.blue);
            PathSpheresController pathSpheresController = currentPointCollider.GetComponent<PathSpheresController>();
            pathSpheresController.changeTriggerState(true);

            if (pathSpheresController.followPath != null)
            {
                int pathNum = pathSpheresController.pathNum;
                int pointNum = pathSpheresController.pointNum;
                OnPathPointHovered(pathNum, pointNum, Color.blue);
            }
        }

        // if mini camera
        if (!miniCameraAlreadySelected && (other.gameObject.layer == 15))
        {
            miniCameraAlreadySelected = true;
            currentMiniCameraCollider = other.gameObject;
            currentMiniCameraCollider.GetComponent<CameraRotationController>().triggerOn = true;
            currentMiniCameraCollider.GetComponent<CameraRotationController>().followPathCamera.isMiniCameraOnTrigger = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // selecting the object to define a path can happen whenever the hand is triggering it that's why we check it here
        // if character
        // AIXÒ D'HAVER-HO DE REPETIR TOT PER EL FOLLOWCAMERA NO M'ACABA D'AGRADAR, A VEURE SI TROBO UNA MANERA MÉS MACA
        if (other.gameObject.layer == 10)
        {
            FollowPath followPath = other.gameObject.GetComponent<FollowPath>();
            ObjectsSelector objectsSelector = other.gameObject.GetComponent<ObjectsSelector>();

            //// check if it has a follow path component and if there is no other gameObject in the scene already selected for path, to avoid defining one for several objects at same time
            //if (currentSelectedForPath == null)
            //currentSelectedForPath = other.gameObject;

            if (followPath != null)
            {
                // change color only if selected state has changed to avoid slowing performance since then it would do it for each frame
                if (itemAlreadySelected != followPath.isSelectedForPath)
                {
                    GameObject itemControlMenu = other.transform.Find("ItemControlMenu").gameObject;
                    itemControlMenu.GetComponent<Canvas>().enabled = followPath.isSelectedForPath;

                    currentSelectedForPath = other.gameObject;
                    Color color = followPath.isSelectedForPath ? DefinePath.instance.selectedLineColor : Color.blue;

                    changeColorMaterials(currentItemCollider, color);
                    itemAlreadySelected = followPath.isSelectedForPath;
                    deselectAllItems();
                }
            }

            if (objectsSelector != null)
            {
                if (itemAlreadySelected != objectsSelector.isSelected)
                {
                    currentSelectedForPath = other.gameObject;
                    Color color = objectsSelector.isSelected ? DefinePath.instance.selectedLineColor : Color.blue;
                    changeColorMaterials(currentItemCollider, color);
                    itemAlreadySelected = objectsSelector.isSelected;
                    deselectAllItems();
                }
            }
        }

        if (other.gameObject.layer == 7)
        {
            FollowPathCamera followPathCamera = other.gameObject.GetComponent<FollowPathCamera>();

            //// check if it has a follow path component and if there is no other gameObject in the scene already selected for path, to avoid defining one for several objects at same time
            //if (currentSelectedForPath == null)
            //    currentSelectedForPath = other.gameObject;

            if (followPathCamera != null)
            {
                // change color only if selected state has changed to avoid slowing performance since then it would do it for each frame
                if (itemAlreadySelected != followPathCamera.isSelectedForPath)
                {
                    currentSelectedForPath = other.gameObject;
                    Color color = followPathCamera.isSelectedForPath ? DefinePath.instance.selectedLineColor : Color.black;

                    changeColorMaterials(currentItemCollider, color);
                    itemAlreadySelected = followPathCamera.isSelectedForPath;
                    deselectAllItems();
                }
            }
        }

        if (other.gameObject.layer == 14)
        {
            PathSpheresController pathSpheresController = currentPointCollider.GetComponent<PathSpheresController>();
            bool isSelected = pathSpheresController.isSelected;
            Color color = isSelected ? DefinePath.instance.hoverLineColor : Color.blue;
            changeColorMaterials(currentPointCollider, color);

            if (pathSpheresController.followPath != null)
            {
                int pathNum = pathSpheresController.pathNum;
                int pointNum = pathSpheresController.pointNum;
                OnPathPointHovered(pathNum, pointNum, color);
            }
        }


        if (other.gameObject.layer == 15)
        {
            bool isSelected = currentMiniCameraCollider.GetComponent<CameraRotationController>().isSelected;
            Color color = isSelected ? DefinePath.instance.hoverLineColor : Color.blue;
            changeColorMaterials(currentMiniCameraCollider, color);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // change the color to white to the first collider
        if (other.gameObject == currentItemCollider)
        {
            try
            {
                currentItemCollider.GetComponent<CustomGrabbableCharacters>().objectSelected(gameObject, false);
            }
            catch (System.Exception e)
            {
                //Debug.Log(e.Message);
            }

            if (other.gameObject.layer == 10)
            {
                FollowPath followPath = other.gameObject.GetComponent<FollowPath>();
                ObjectsSelector objectsSelector = other.gameObject.GetComponent<ObjectsSelector>();
                // if the object has a limit rotation script mark it as selected
                bool isSelected = false;
                if (followPath != null)
                {
                    followPath.triggerOn = false;
                    alreadyTriggered = false;
                    // we just want to define a path for a single object
                    if (other.gameObject != currentSelectedForPath)
                        followPath.isSelectedForPath = false;
                    isSelected = followPath.isSelectedForPath;

                }
                if (objectsSelector != null)
                {
                    objectsSelector.triggerOn = false;
                    alreadyTriggered = false;
                    if (other.gameObject != currentSelectedForPath)
                        objectsSelector.isSelected = false;
                    isSelected = objectsSelector.isSelected;
                }
                if (!isSelected)
                {
                    alreadyTriggered = false;
                    //currentCollider = other.gameObject;

                    // check if it is item or camera
                    Color color = new Color();
                    if (other.gameObject.layer == 10)
                        color = Color.white;
                    else if (other.gameObject.layer == 7)
                        color = Color.black;
                    changeColorMaterials(currentItemCollider, color);

                    if (other.gameObject == currentSelectedForPath)
                        currentSelectedForPath = null;
                }
            }
            if (other.gameObject.layer == 7)
            {
                FollowPathCamera followPath = other.gameObject.GetComponent<FollowPathCamera>();
                // if the object has a limit rotation script mark it as selected
                bool isSelected = false;
                if (followPath != null)
                {
                    followPath.triggerOn = false;
                    alreadyTriggered = false;
                    // we just want to define a path for a single object
                    if (other.gameObject != currentSelectedForPath)
                        followPath.isSelectedForPath = false;
                    isSelected = followPath.isSelectedForPath;

                }
                if (!isSelected)
                {
                    alreadyTriggered = false;
                    //currentCollider = other.gameObject;

                    // check if it is item or camera
                    Color color = new Color();
                    if (other.gameObject.layer == 10)
                        color = Color.white;
                    else if (other.gameObject.layer == 7)
                        color = Color.black;
                    changeColorMaterials(currentItemCollider, color);

                    if (other.gameObject == currentSelectedForPath)
                        currentSelectedForPath = null;
                }
            }
        }


        // if sphere path point
        else if (other.gameObject == currentPointCollider)
        {
            pointAlreadySelected = false;

            PathSpheresController spheresController = currentPointCollider.GetComponent<PathSpheresController>();
            // once the hand has exit the trigger at least once, then the point is able to be deleted
            spheresController.isBeingCreated = false;
            spheresController.changeTriggerState(false);
            
            bool isSelected = spheresController.isSelected;
            // check if the parent item is selected to know which is the correct color to use
            Color notHoverColor = DefinePath.instance.defaultLineColor;
            if (spheresController.followPathCamera != null)
            {
                if (spheresController.followPathCamera.isSelectedForPath)
                    notHoverColor = DefinePath.instance.selectedLineColor;
            }

            if (spheresController.followPath != null)
            {
                if (spheresController.followPath.isSelectedForPath)
                    notHoverColor = DefinePath.instance.selectedLineColor;
            }

            Color color = isSelected ? DefinePath.instance.hoverLineColor : notHoverColor;
            changeColorMaterials(currentPointCollider, color);
            currentPointCollider = null;

            if (spheresController.followPath != null)
            {
                int pathNum = spheresController.pathNum;
                int pointNum = spheresController.pointNum;
                OnPathPointHovered(pathNum, pointNum, color);
            }
        }

        else if (other.gameObject == currentMiniCameraCollider)
        {
            miniCameraAlreadySelected = false;

            CameraRotationController cameraRotationController = currentMiniCameraCollider.GetComponent<CameraRotationController>();
            cameraRotationController.triggerOn = false;
            cameraRotationController.followPathCamera.isMiniCameraOnTrigger = false;

            bool isSelected = cameraRotationController.isSelected;
            // check if the parent item is selected to know which is the correct color to use
            Color notHoverColor = DefinePath.instance.defaultLineColor;
            if (cameraRotationController.followPathCamera != null)
            {
                if (cameraRotationController.followPathCamera.isSelectedForPath)
                    notHoverColor = DefinePath.instance.selectedLineColor;
            }

            Color color = isSelected ? DefinePath.instance.hoverLineColor : notHoverColor;
            changeColorMaterials(currentMiniCameraCollider, color);
            currentMiniCameraCollider = null;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        alreadyTriggered = false;
        itemAlreadySelected = false;
        pointAlreadySelected = false;
        currentSelectedForPath = null;
        currentItemCollider = null;
        currentPointCollider = null;
    }

    // Update is called once per frame
    void Update()
    {
    }
}
