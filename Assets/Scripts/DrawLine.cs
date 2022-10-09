using System.Collections.Generic;
using UnityEngine;

public class DrawLine : MonoBehaviour
{
    public GameObject linePrefab;
    public GameObject handController;
    private GameObject currentLine;
    private LineRenderer lineRenderer;
    private EdgeCollider2D edgeCollider;
    private List<Vector2> fingerPositions = new List<Vector2>();

    bool buttonDown;

    private void Start()
    {
        buttonDown = false;
    }
    void Update()
    {
        if (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger))
        {
            CreateLine();
        }
        if (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger) && buttonDown == false)
        {
            buttonDown = true;
            Vector2 tempFingerPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (Vector2.Distance(tempFingerPos, fingerPositions[fingerPositions.Count - 1]) > .1f)
            {
                UpdateLine(tempFingerPos);
            }
        }
    }

    void CreateLine()
    {
        // Llamamos a CreateLine una sola vez cuando empezamos a dibujar para crear currentLine. currentLine se visualizará porque tendrán un lineRenderer y tendrá colisión porque tendrá un edgeCollider
        currentLine = Instantiate(linePrefab, Vector3.zero, Quaternion.identity);
        lineRenderer = currentLine.GetComponent<LineRenderer>();
        edgeCollider = currentLine.GetComponent<EdgeCollider2D>();
        fingerPositions.Clear();
        // Como lineRenderer es una línea, necesitamos añadir dos puntos a fingerPositions para poder dibujarla sin errores
        fingerPositions.Add(handController.transform.position);
        fingerPositions.Add(handController.transform.position);
        // Dibujamos una línea compuesta de dos puntos
        lineRenderer.SetPosition(0, fingerPositions[0]);
        lineRenderer.SetPosition(1, fingerPositions[1]);
        edgeCollider.points = fingerPositions.ToArray();
    }

    void UpdateLine(Vector2 newFingerPos)
    {
        fingerPositions.Add(newFingerPos);
        lineRenderer.positionCount++;
        // Convertimos el List de posiciones por las que ha ido pasando el dedo en la línea que vamos a ver
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, newFingerPos);
        // Convertimos el List de posiciones por las que ha ido pasando el dedo en los puntos del edge collider
        edgeCollider.points = fingerPositions.ToArray();
    }
}