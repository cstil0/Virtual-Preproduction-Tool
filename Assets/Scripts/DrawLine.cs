using System.Collections.Generic;
using UnityEngine;

public class DrawLine : MonoBehaviour
{
    // singleton
    public static DrawLine instance = null;

    public GameObject linePrefab;
    public GameObject handController;
    private GameObject currentLine;
    private EdgeCollider2D edgeCollider;
    private List<Vector2> fingerPositions = new List<Vector2>();

    private LineRenderer lineRenderer;
    public int countPoints;
    bool buttonDown;
    public bool continueLine;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }
    private void Start()
    {
        buttonDown = false;
        continueLine = false;
        countPoints = 0;
    }

    void Update()
    {
        //if (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger))
        //{
        //    CreateLine();
        //}
        if (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger))
        {
            if (!buttonDown)
            {
                buttonDown = true;

                if (continueLine)
                {
                    if (countPoints == 0)
                    {
                        GameObject line = Instantiate(linePrefab, Vector3.zero, Quaternion.identity);
                        lineRenderer = line.GetComponent<LineRenderer>();
                        lineRenderer.SetPosition(countPoints, handController.transform.position);
                        lineRenderer.positionCount = 2;
                        countPoints += 1;
                    }
                    else
                    {
                        lineRenderer.SetPosition(countPoints, handController.transform.position);
                        lineRenderer.positionCount += 1;
                        countPoints += 1;
                    }
                }
                else
                {
                    countPoints = 0;
                }
            }
        }
        else
        {
            buttonDown = false;
        }
    }

    void CreateLine()
    {
        // Llamamos a CreateLine una sola vez cuando empezamos a dibujar para crear currentLine. currentLine se visualizar� porque tendr�n un lineRenderer y tendr� colisi�n porque tendr� un edgeCollider
        currentLine = Instantiate(linePrefab, Vector3.zero, Quaternion.identity);
        lineRenderer = currentLine.GetComponent<LineRenderer>();
        edgeCollider = currentLine.GetComponent<EdgeCollider2D>();
        fingerPositions.Clear();
        // Como lineRenderer es una l�nea, necesitamos a�adir dos puntos a fingerPositions para poder dibujarla sin errores
        fingerPositions.Add(handController.transform.position);
        fingerPositions.Add(handController.transform.position);
        // Dibujamos una l�nea compuesta de dos puntos
        lineRenderer.SetPosition(0, fingerPositions[0]);
        lineRenderer.SetPosition(1, fingerPositions[1]);
        edgeCollider.points = fingerPositions.ToArray();
    }

    void UpdateLine(Vector2 newFingerPos)
    {
        fingerPositions.Add(newFingerPos);
        lineRenderer.positionCount++;
        // Convertimos el List de posiciones por las que ha ido pasando el dedo en la l�nea que vamos a ver
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, newFingerPos);
        // Convertimos el List de posiciones por las que ha ido pasando el dedo en los puntos del edge collider
        edgeCollider.points = fingerPositions.ToArray();
    }
}