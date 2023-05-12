using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        DirectorPanelManager.instance.OnHideShowGrid += showHideArrows;
    }

    private void OnDisable()
    {
        DirectorPanelManager.instance.OnHideShowGrid -= showHideArrows;
    }

    private void OnTriggerEnter(Collider other)
    {
        isTrigger = true;
        if (other.gameObject.layer == 3)
            handCollider = other.gameObject;

        Renderer renderer = gameObject.GetComponent<Renderer>();
        Material material = renderer.material;
        material.color = Color.blue;
    }

    private void OnTriggerExit(Collider other)
    {
        isTrigger = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        Renderer renderer = gameObject.GetComponent<Renderer>();
        Material material = renderer.material;
        originalColor = material.color;
    }

    // Update is called once per frame
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
            gameObject.transform.parent.position += movementDirection;
        }
    }

    void showHideArrows(bool isShow)
    {
        gameObject.GetComponent<MeshRenderer>().enabled = isShow;
    }
}