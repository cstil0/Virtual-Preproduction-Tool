using ClipperLib;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

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

    public GameObject itemsParent;

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
    public void changeColorMaterials(GameObject currentParent, Color color, bool sendChangeColor = true)
    {
        // only VR application sends the message to inform of a change of color.
        // boolean is needed to ensure that the recursive call does not send the message again.
        if (sendChangeColor && ModesManager.instance.role == ModesManager.eRoleType.ASSISTANT)
            UDPSender.instance.sendChangeItemColor(currentParent.name, UnityEngine.ColorUtility.ToHtmlStringRGBA(color));

        // get the renderer component and change its color
        Renderer renderer = currentParent.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material parentMaterial = renderer.material;
            parentMaterial.color = color;
        }

        for (int i = 0; i < currentParent.transform.childCount; i++)
        {
            GameObject currChild = currentParent.transform.GetChild(i).gameObject;
            
            // not all childs have materials, so we need to handle errors
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
                changeColorMaterials(currChild, color, false);
        }
    }

    private void deselectAllItems()
    {
        // iterate through all the instantiated items
        for (int i = 0; i < itemsParent.transform.childCount; i++)
        {
            GameObject currItem = itemsParent.transform.GetChild(i).gameObject;

            // if it matches the current selected one, skip it to avoid unselecting it
            if (currItem == currentSelectedForPath)
                continue;

            currItem.TryGetComponent(out FollowPath followPath);
            currItem.TryGetComponent(out ObjectsSelector objectsSelector);
            currItem.TryGetComponent(out FollowPathCamera followPathCamera);

            // if it is a character, change is color and path, disable its buttons and set it as unselected
            if (followPath != null)
            {
                if (followPath.isSelectedForPath)
                {
                    followPath.isSelectedForPath = false;
                    changeColorMaterials(currItem, Color.white);
                    followPath.changePathColor();

                    GameObject itemControlMenu = currItem.transform.Find("ItemControlMenu").gameObject;
                    itemControlMenu.GetComponent<Canvas>().enabled = false;
                }
            }

            // if it is a normal object, just change its color and set it as unselected
            if (objectsSelector != null)
            {
                if (objectsSelector.isSelected)
                {
                    objectsSelector.isSelected = false;
                    changeColorMaterials(currItem, Color.white);
                }
            }

            // if it is a camera change its color and path, and set it as unselected
            if (followPathCamera != null)
            {
                if (followPathCamera.isSelectedForPath)
                {
                    followPathCamera.isSelectedForPath = false;
                    changeColorMaterials(currItem, Color.black);
                    followPathCamera.changePathColor();
                }
            }
        }

        // unselect also all of the points in the scene
        deselectAllPoints();
    }

    void deselectAllPoints(int pointNum = -1)
    {
        // find all path containers in the scene
        GameObject[] pathContainers = GameObject.FindGameObjectsWithTag("PathContainer");

        foreach (GameObject pathContainer in pathContainers)
        {
            Transform pathContainerTrans = pathContainer.transform;
            // iterate through all points and deselect them all
            for (int i = 1; i < pathContainerTrans.childCount; i++)
            {
                // if current point matches the selected one, skip it to avoid unselecting it
                if (i == pointNum + 1)
                    continue;

                GameObject currPoint = null;
                FollowPath followPath = null;
                FollowPathCamera followPathCamera = null;

                // if it is a character point, get the sphere
                if (pathContainerTrans.GetChild(i).tag == "PathPoint")
                    currPoint = pathContainerTrans.GetChild(i).gameObject;
                else
                {
                    // if it is a camera point, get also the minicamera to change its color
                    currPoint = pathContainerTrans.GetChild(i).GetChild(0).gameObject;
                    GameObject currMiniCamera = pathContainerTrans.GetChild(i).GetChild(1).gameObject;

                    // change the minicamera state to unselected
                    CameraRotationController cameraRotationController = currMiniCamera.GetComponent<CameraRotationController>();
                    cameraRotationController.isSelected = false;
                    followPathCamera = cameraRotationController.followPathCamera;

                    // change the minicamera color according to the camera state
                    if (followPathCamera.isSelectedForPath)
                        changeColorMaterials(currMiniCamera, DefinePath.instance.selectedLineColor);
                    else
                        changeColorMaterials(currMiniCamera, DefinePath.instance.defaultLineColor);

                    // hide buttons
                    GameObject cameraControlButtons = currMiniCamera.transform.Find("CameraControlButtons").gameObject;
                    showHidePointsControl(cameraControlButtons, false);
                }

                // change the sphere's state
                PathSpheresController pathSpheresController = currPoint.GetComponent<PathSpheresController>();
                pathSpheresController.isSelected = false;
                followPath = pathSpheresController.followPath;
                Renderer renderer = currPoint.GetComponent<Renderer>();
                Material parentMaterial = renderer.material;

                // hide buttons
                GameObject pointControlButtons = currPoint.transform.Find("PointControlButtons").gameObject;
                showHidePointsControl(pointControlButtons, false);

                if (followPath != null)
                {
                    // change spheres color according to the character's state
                    Color color = DefinePath.instance.defaultLineColor;
                    if (followPath.isSelectedForPath)
                        color = DefinePath.instance.selectedLineColor;

                    parentMaterial.color = color;

                    int pathNum = pathSpheresController.pathNum;
                    int pathPointNum = pathSpheresController.pointNum;
                    // call event to change also the corresponding circle color
                    OnPathPointHovered(pathNum, pathPointNum, color);
                }
                if (followPathCamera != null)
                {
                    // change spheres color according to the camera's state
                    if (followPathCamera.isSelectedForPath)
                        parentMaterial.color = DefinePath.instance.selectedLineColor;
                    else
                        parentMaterial.color = DefinePath.instance.defaultLineColor;
                }
            }
        }
    }

    // used to show or hide points buttons
    void showHidePointsControl(GameObject pointControl, bool show)
    {
        pointControl.GetComponent<Canvas>().enabled = show;
        pointControl.transform.GetChild(0).GetComponentInChildren<BoxCollider>().enabled = show;
    }

    // when hands collide with any element in the scene, its reference must be stored to know which was the first one triggered to consider this only to perform actions
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
                // change its trigger state either if it is a character or an object
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
                // change its trigger state if it is ca camera
                FollowPathCamera followPath = other.gameObject.GetComponent<FollowPathCamera>();
                if (followPath != null)
                {
                    followPath.triggerOn = true;
                    isSelected = followPath.isSelectedForPath;
                }

                // inform to the screen project about a change in the main camera
                if (ModesManager.instance.role == ModesManager.eRoleType.ASSISTANT)
                    UDPSender.instance.changeMainCamera(other.gameObject);
            }

            // change its hover color if it is not selected
            if (!isSelected)
            {
                alreadyTriggered = true;
                changeColorMaterials(currentItemCollider, UnityEngine.Color.blue, false);
            }
            // if the object has a custom grabbable script mark it as selected
            currentItemCollider.TryGetComponent(out CustomGrabbableCharacters grabbableCharacters);
            currentItemCollider.TryGetComponent(out CustomGrabbableCameras grabbableCameras);

            if (grabbableCharacters != null)
                grabbableCharacters.objectSelected(gameObject, true);
            if (grabbableCameras != null)
                grabbableCameras.objectSelected(gameObject.transform.GetChild(0).gameObject, true);
        }

        // if sphere path point change its trigger state and its hover color
        if (!pointAlreadySelected && (other.gameObject.layer == 14))
        {
            pointAlreadySelected = true;
            currentPointCollider = other.gameObject;
            changeColorMaterials(currentPointCollider, Color.blue, false);
            PathSpheresController pathSpheresController = currentPointCollider.GetComponent<PathSpheresController>();
            pathSpheresController.changeTriggerState(true);

            if (pathSpheresController.followPath != null)
            {
                int pathNum = pathSpheresController.pathNum;
                int pointNum = pathSpheresController.pointNum;
                // change also the color for the corresponding circle
                OnPathPointHovered(pathNum, pointNum, Color.blue);
            }
        }

        // if mini camera, change its trigger state and hover color
        if (!miniCameraAlreadySelected && (other.gameObject.layer == 15))
        {
            miniCameraAlreadySelected = true;
            currentMiniCameraCollider = other.gameObject;
            currentMiniCameraCollider.GetComponent<CameraRotationController>().triggerOn = true;
            currentMiniCameraCollider.GetComponent<CameraRotationController>().followPathCamera.isMiniCameraOnTrigger = true;
        }
    }

    // selecting the object to define a path can happen at any moment while the hand is triggering the element
    private void OnTriggerStay(Collider other)
    {
        // if character or normal object
        if (currentItemCollider == other.gameObject)
        {
            FollowPath followPath = other.gameObject.GetComponent<FollowPath>();
            ObjectsSelector objectsSelector = other.gameObject.GetComponent<ObjectsSelector>();
            FollowPathCamera followPathCamera = other.gameObject.GetComponent<FollowPathCamera>();

            if (followPath != null)
            {
                // change color only if selected state has changed to avoid slowing performance since then we would do this for each frame
                if (itemAlreadySelected != followPath.isSelectedForPath)
                {
                    bool isSelected = followPath.isSelectedForPath;
                    // show or hide buttons
                    GameObject itemControlMenu = other.transform.Find("ItemControlMenu").gameObject;
                    itemControlMenu.GetComponent<Canvas>().enabled = isSelected;

                    // change its color
                    currentSelectedForPath = other.gameObject;
                    Color color = isSelected ? DefinePath.instance.selectedLineColor : Color.blue;

                    changeColorMaterials(currentItemCollider, color);
                    itemAlreadySelected = followPath.isSelectedForPath;

                    // deselect the rest of items in the scene
                    if (isSelected)
                        deselectAllItems();
                }
            }

            if (objectsSelector != null)
            {
                // change color only if selected state has changed to avoid slowing performance since then we would do this for each frame
                if (itemAlreadySelected != objectsSelector.isSelected)
                {
                    bool isSelected = objectsSelector.isSelected;

                    // change its color
                    currentSelectedForPath = other.gameObject;
                    Color color = isSelected ? DefinePath.instance.selectedLineColor : Color.blue;
                    changeColorMaterials(currentItemCollider, color);
                    itemAlreadySelected = objectsSelector.isSelected;

                    // deselect the rest of items in the scene
                    if (isSelected) 
                        deselectAllItems();                    
                }
            }

            // if camera
            if (followPathCamera != null)
            {
                // change color only if selected state has changed to avoid slowing performance since then it would do it for each frame
                if (itemAlreadySelected != followPathCamera.isSelectedForPath)
                {
                    bool isSelected = followPathCamera.isSelectedForPath;
                    currentSelectedForPath = other.gameObject;
                    Color color = isSelected ? DefinePath.instance.selectedLineColor : Color.black;

                    // change its color
                    changeColorMaterials(currentItemCollider, color);
                    itemAlreadySelected = isSelected;

                    // deselect the rest of items in the scene
                    if (isSelected)
                        deselectAllItems();

                    // reassign the textures that are shown for each path point
                    string[] splittedName = currentSelectedForPath.name.Split(" ");
                    int cameraNum = int.Parse(splittedName[1]);
                    DefinePath.instance.reassignPathCanvas(cameraNum);
                }
            }
        }

        // if point sphere
        if (currentPointCollider == other.gameObject)
        {
            currentPointCollider.TryGetComponent(out PathSpheresController pathSpheresController);

            bool isSelected = false;
            if (pathSpheresController != null)
                isSelected = pathSpheresController.isSelected;

            // change color only if selected state has changed to avoid slowing performance since then it would do it for each frame
            if (isSelected != pointAlreadySelected && currentPointCollider == other.gameObject)
            {
                // change its selected state
                pointAlreadySelected = isSelected;
                // show / hide buttons
                GameObject pointControlButtons = other.transform.Find("PointControlButtons").gameObject;
                showHidePointsControl(pointControlButtons, isSelected);

                // change its color
                Color color = isSelected ? DefinePath.instance.hoverLineColor : Color.blue;
                if (ModesManager.instance.role == ModesManager.eRoleType.ASSISTANT)
                {
                    changeColorMaterials(currentPointCollider, color, false);
                    UDPSender.instance.sendChangePointColor(pathSpheresController.item.name, currentPointCollider.name, UnityEngine.ColorUtility.ToHtmlStringRGBA(color));
                }

                // if it corresponds to a character, call the event to change also the circles color
                if (pathSpheresController.followPath != null)
                {
                    int pathNum = pathSpheresController.pathNum;
                    int pointNum = pathSpheresController.pointNum;
                    OnPathPointHovered(pathNum, pointNum, color);
                }

                // deselect the rest of points in the scene
                if (isSelected)
                    deselectAllPoints(pathSpheresController.pointNum);
            }
        }

        // if minicamera
        if (currentMiniCameraCollider == other.gameObject)
        {
            currentMiniCameraCollider.TryGetComponent(out CameraRotationController cameraRotationController);

            bool isSelected = false;
            if (cameraRotationController != null)
                isSelected = cameraRotationController.isSelected;

            // change color only if selected state has changed to avoid slowing performance since then it would do it for each frame
            if (isSelected != miniCameraAlreadySelected && currentMiniCameraCollider == other.gameObject)
            {
                // change its selected state
                miniCameraAlreadySelected = isSelected;
                // show / hide buttons
                GameObject cameraControlButtons = other.transform.Find("CameraControlButtons").gameObject;
                showHidePointsControl(cameraControlButtons, isSelected);

                // change its color
                Color color = isSelected ? DefinePath.instance.hoverLineColor : Color.blue;
                if (ModesManager.instance.role == ModesManager.eRoleType.ASSISTANT)
                    changeColorMaterials(currentMiniCameraCollider, color, false);

                // deselect the rest of points in the scene
                if (isSelected)
                    deselectAllPoints(cameraRotationController.pointNum);
            }
        }
    }

    // check trigger exit to free the general reference of the current ontrigger item
    private void OnTriggerExit(Collider other)
    {
        // check if the current collider matches the one stored
        if (other.gameObject == currentItemCollider)
        {
            // if the object has a custom grabbable script mark it as deselected
            currentItemCollider.TryGetComponent(out CustomGrabbableCharacters grabbableCharacters);
            currentItemCollider.TryGetComponent(out CustomGrabbableCameras grabbableCameras);

            if (grabbableCharacters != null)
                grabbableCharacters.objectSelected(gameObject, false);
            if (grabbableCameras != null)
                grabbableCameras.objectSelected(gameObject.transform.GetChild(0).gameObject, false);

            // if character or normal oject
            if (other.gameObject.layer == 10)
            {
                FollowPath followPath = other.gameObject.GetComponent<FollowPath>();
                ObjectsSelector objectsSelector = other.gameObject.GetComponent<ObjectsSelector>();
                // if character, changei its trigger state
                bool isSelected = false;
                if (followPath != null)
                {
                    followPath.triggerOn = false;
                    alreadyTriggered = false;
                    // if another character was already selected, ensure that the current one is set as deselected
                    if (other.gameObject != currentSelectedForPath)
                        followPath.isSelectedForPath = false;
                    isSelected = followPath.isSelectedForPath;

                }

                // if normal object, change its trigger state
                if (objectsSelector != null)
                {
                    objectsSelector.triggerOn = false;
                    alreadyTriggered = false;
                    if (other.gameObject != currentSelectedForPath)
                        objectsSelector.isSelected = false;
                    isSelected = objectsSelector.isSelected;
                }

                // if it is not selected, change its color back to white
                if (!isSelected)
                {
                    alreadyTriggered = false;

                    Color color = Color.white;

                    if (ModesManager.instance.role == ModesManager.eRoleType.ASSISTANT)
                        changeColorMaterials(currentItemCollider, color);

                    if (other.gameObject == currentSelectedForPath)
                        currentSelectedForPath = null;
                }
            }

            // if camera
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

                // if its not selected, change its color back to black
                if (!isSelected)
                {
                    alreadyTriggered = false;

                    Color color = Color.black;

                    if (ModesManager.instance.role == ModesManager.eRoleType.ASSISTANT)
                        changeColorMaterials(currentItemCollider, color);

                    if (other.gameObject == currentSelectedForPath)
                        currentSelectedForPath = null;
                }
            }
        }

        // if sphere path point
        else if (other.gameObject == currentPointCollider)
        {
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

            // change its color only in the VR side since it has the correct reference of the current selected one,
            // then, client side will change it thanks to the UDP message sent
            if (ModesManager.instance.role == ModesManager.eRoleType.ASSISTANT)
            {
                changeColorMaterials(currentPointCollider, color);
                UDPSender.instance.sendChangePointColor(spheresController.item.name, currentPointCollider.name, UnityEngine.ColorUtility.ToHtmlStringRGBA(color));
            }

            // remove the intrigger item reference
            currentPointCollider = null;

            // if it corresponds to a character, change also the corresponding circle
            if (spheresController.followPath != null)
            {
                int pathNum = spheresController.pathNum;
                int pointNum = spheresController.pointNum;
                OnPathPointHovered(pathNum, pointNum, color);
            }
        }

        // if minicamera
        else if (other.gameObject == currentMiniCameraCollider)
        {
            // change its trigger state
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

            // change its color
            Color color = isSelected ? DefinePath.instance.hoverLineColor : notHoverColor;

            if (ModesManager.instance.role == ModesManager.eRoleType.ASSISTANT)
                changeColorMaterials(currentMiniCameraCollider, color);

            // remove the intrigger item reference
            currentMiniCameraCollider = null;
        }
    }

    // needed to call the event to change the circles color from an external script
    public void callHoverPointEvent(int pathNum, int pointNum, Color color)
    {
        OnPathPointHovered(pathNum, pointNum, color);
    }

    void Start()
    {
        alreadyTriggered = false;
        itemAlreadySelected = false;
        pointAlreadySelected = false;
        currentSelectedForPath = null;
        currentItemCollider = null;
        currentPointCollider = null;
    }

    void Update()
    {
    }
}
