using Microsoft.MixedReality.Toolkit.Input;
using System.Collections;
using System.Collections.Generic;
using Unity.RenderStreaming;
using UnityEngine;
using UnityEngine.EventSystems;

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
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, panelCamera, out mouseLocalPoint);

        Vector2 localMousePosition = rectTransform.InverseTransformPoint(Input.mousePosition);
        if (Input.GetAxis("Mouse ScrollWheel") > 0f && rectTransform.rect.Contains(mouseLocalPoint))
        {
            Vector3 targetPosition = cameraTransform.position;
            targetPosition.y -= 1;
            cameraTransform.position = Vector3.Slerp(cameraTransform.position, targetPosition, Time.deltaTime * speed);
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0f && rectTransform.rect.Contains(mouseLocalPoint))
        {
            Vector3 targetPosition = cameraTransform.position;
            targetPosition.y += 1;
            cameraTransform.position = Vector3.Slerp(cameraTransform.position, targetPosition, Time.deltaTime * speed);
        }

        if (Input.GetMouseButton(2)) {
            Vector2 currMousePos = Input.mousePosition;
            Vector3 targetPosition = cameraTransform.position;
            if (lastMousePos == null)
                lastMousePos = currMousePos;
            
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
