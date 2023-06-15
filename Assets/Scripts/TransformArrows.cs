using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;

public class TransformArrows : MonoBehaviour
{
    [SerializeField] Vector3 arrowDirection;
    private bool isTrigger = false;
    private bool isMoving = false;

    private GameObject handCollider;
    private Vector3 lastHandPosition;
    private Color originalColor;

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
        gameObject.GetComponent<MeshRenderer>().enabled = isShow;
        Transform parent = gameObject.transform.parent.parent;
        parent.TryGetComponent(out OVRGrabbable ovrgrabbable);
        if (ovrgrabbable != null)
            ovrgrabbable.enabled = !isShow;
    }
}