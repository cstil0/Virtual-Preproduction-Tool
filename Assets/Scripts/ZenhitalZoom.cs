using UnityEngine;

public class ZenhitalZoom : MonoBehaviour
{
    [SerializeField] float speed = 6.0f;
    [SerializeField] Transform cameraTransform;
    [SerializeField] Camera panelCamera;
    private bool isMouseInside = false;
    private RectTransform rectTransform;
    private Vector2 mouseLocalPoint;
    private Vector2 lastMousePos;

    private void Start()
    {
        rectTransform = gameObject.GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        // get the local mouse position with respect to the rect transform of the aerial view to know if it is inside
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, panelCamera, out mouseLocalPoint);

        // if mouse is scrolling and is inside the rect transform, check the kind of scroll it is doing
        if (Input.GetAxis("Mouse ScrollWheel") > 0f && rectTransform.rect.Contains(mouseLocalPoint))
        {
            Vector3 targetPosition = cameraTransform.position;
            targetPosition.y -= 1;
            // interpolate between current position and target one
            cameraTransform.position = Vector3.Slerp(cameraTransform.position, targetPosition, Time.deltaTime * speed);
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0f && rectTransform.rect.Contains(mouseLocalPoint))
        {
            Vector3 targetPosition = cameraTransform.position;
            targetPosition.y += 1;
            // interpolate between current position and target one
            cameraTransform.position = Vector3.Slerp(cameraTransform.position, targetPosition, Time.deltaTime * speed);
        }

        // when mouse's middle button is pressed drag the aerial view
        if (Input.GetMouseButton(2)) {
            Vector2 currMousePos = Input.mousePosition;
            Vector3 targetPosition = cameraTransform.position;
            if (lastMousePos == null)
                lastMousePos = currMousePos;
            
            // check direction of the movement and change the camera's position accordingly
            Vector2 dist = lastMousePos - currMousePos;

            if (dist.x < 0)
            {
                targetPosition.x += 1;
                cameraTransform.position = Vector3.Slerp(cameraTransform.position, targetPosition, Time.deltaTime * speed);
            }
            else if (dist.x > 0)
            {
                targetPosition.x -= 1;
                cameraTransform.position = Vector3.Slerp(cameraTransform.position, targetPosition, Time.deltaTime * speed);
            }
            else if (dist.y < 0)
            {
                targetPosition.z += 1;
                cameraTransform.position = Vector3.Slerp(cameraTransform.position, targetPosition, Time.deltaTime * speed);
            }
            else if (dist.y > 0)
            {
                targetPosition.z -= 1;
                cameraTransform.position = Vector3.Slerp(cameraTransform.position, targetPosition, Time.deltaTime * speed);
            }
            lastMousePos = currMousePos;
        }
    }
}
