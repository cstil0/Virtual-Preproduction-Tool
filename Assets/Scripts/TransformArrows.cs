using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;

// this script is used to manage the relocation of items using the transform arrows is grid mode
public class TransformArrows : MonoBehaviour
{
    [SerializeField] Vector3 arrowDirection;
    private bool isTrigger = false;
    private bool isMoving = false;

    private GameObject handCollider;
    private Vector3 lastHandPosition;
    private Color originalColor;

    private Collider grabPoint;

    private void Awake()
    {
        // check if the object has originally an OVRGrabbable script, since we will destroy it when grid is shown and add it again when hidden
        Transform parent = gameObject.transform.parent.parent;
        parent.TryGetComponent(out OVRGrabbable ovrgrabbable);
        if (ovrgrabbable != null)
            grabPoint = ovrgrabbable.grabPoints[0];
    }

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
        DirectorPanelManager.instance.OnHideShowGrid -= showHideArrows;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 3)
        {
            isTrigger = true;
            handCollider = other.gameObject;
            Renderer renderer = gameObject.GetComponent<Renderer>();
            Material material = renderer.material;
            material.color = Color.blue;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 3) 
        {
            isTrigger = false;
        }
    }

    void Start()
    {
        // suscript to the event on start function, since in onEnable, the event is usually not created yet, causing an error
        DirectorPanelManager.instance.OnHideShowGrid += showHideArrows;

        Renderer renderer = gameObject.GetComponent<Renderer>();
        Material material = renderer.material;
        originalColor = material.color;

        showHideArrows(DirectorPanelManager.instance.isGridShown);
    }

    void Update()
    {
        // start movement when controller is triggering the arrow and button is pressed
        if (isTrigger && OVRInput.Get(OVRInput.Button.SecondaryHandTrigger))
        {
            if (!isMoving)
                lastHandPosition = handCollider.transform.position;

            isMoving = true;
        }

        // mantain movement until trigger button is released
        else if (!(isMoving && OVRInput.Get(OVRInput.Button.SecondaryHandTrigger)))
        {
            isMoving = false;

            Renderer renderer = gameObject.GetComponent<Renderer>();
            Material material = renderer.material;
            material.color = originalColor;
        }

        if (isMoving)
        {
            Vector3 difference = handCollider.transform.position - lastHandPosition;  
            // only the axis with value 1 in arrowDirection will remain
            Vector3 movementDirection = new Vector3(difference.x * arrowDirection.x, difference.y * arrowDirection.y, difference.z * arrowDirection.z);

            Transform parent = gameObject.transform.parent.parent;
            // if item is a camera, apply the movement to its corresponding dolly track
            if (parent.name.Contains("Camera")){
                FollowPathCamera followPathCamera = parent.gameObject.GetComponent<FollowPathCamera>();
                movementDirection = applyRotation(parent, movementDirection);
                followPathCamera.cinemachineSmoothPath.transform.position += movementDirection;
            }
            else
            {
                movementDirection = applyRotation(parent, movementDirection);
                parent.position += movementDirection;
            }
            
            lastHandPosition = handCollider.transform.position;
        }
    }

    // rotate transform arrows using the item's coordinates, and not world ones to translate it towards the desired direction
    Vector3 applyRotation(Transform item, Vector3 movementDirection)
    {
        if (movementDirection.x != 0.0f)
            return movementDirection.x * item.right;
        else if (movementDirection.y != 0.0f)
            return movementDirection.y * item.up;
        else if (movementDirection.z != 0.0f)
            return movementDirection.z * item.forward;
        else
            return movementDirection;
    }

    void showHideArrows(bool isShow)
    {
        // show / hide arrows mesh renderers
        gameObject.GetComponent<MeshRenderer>().enabled = isShow;
        Transform parent = gameObject.transform.parent.parent;
        parent.TryGetComponent(out OVRGrabbable ovrgrabbable);

        // if it has an ovrgrabbable and arrows are shown, remove its grab point to disable OVR Grabbable functionalities
        if (ovrgrabbable != null && isShow)
        {
            ovrgrabbable.removeGrabPoint();
        }

        // if it originally had a grabpoint from an OVR Grabbable component, add it again once the arrows are hidden
        else if (grabPoint != null && !isShow)
            ovrgrabbable.setGrabPoint(grabPoint);
    }
}