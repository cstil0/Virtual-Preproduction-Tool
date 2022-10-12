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
    // pel cas continu
    public bool startLine;

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

        // VERSIÓN POR PUNTOS
        //if (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger))
        //{
        //    if (!buttonDown)
        //    {
        //        buttonDown = true;

        //        if (continueLine)
        //        {
        //            if (countPoints == 0)
        //            {
        //                GameObject line = Instantiate(linePrefab, Vector3.zero, Quaternion.identity);
        //                lineRenderer = line.GetComponent<LineRenderer>();
        //                lineRenderer.SetPosition(countPoints, handController.transform.position);
        //                lineRenderer.positionCount = 2;
        //                countPoints += 1;
        //            }
        //            else
        //            {
        //                lineRenderer.SetPosition(countPoints, handController.transform.position);
        //                lineRenderer.positionCount += 1;
        //                countPoints += 1;
        //            }
        //        }
        //        else
        //        {
        //            countPoints = 0;
        //        }
        //    }
        //}
        //else
        //{
        //    buttonDown = false;
        //}

        // VERSIÓN CONTINUA
        if (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger) && startLine)
        {
            if (countPoints == 0)
            {
                GameObject line = Instantiate(linePrefab, Vector3.zero, Quaternion.identity);
                lineRenderer = line.GetComponent<LineRenderer>();
                lineRenderer.SetPosition(countPoints, handController.transform.position);
                lineRenderer.positionCount = 1;
                countPoints += 1;
            }
            else
            {
                lineRenderer.positionCount += 1;
                lineRenderer.SetPosition(countPoints, handController.transform.position);
                countPoints += 1;
            }
        }
        else
        {
            countPoints = 0;
        }
    }
}