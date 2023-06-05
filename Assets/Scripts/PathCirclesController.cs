using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathCirclesController : MonoBehaviour
{
    public int pathNum;
    public int pointNum;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void OnEnable()
    {
        DefinePath.instance.OnPathPositionChanged += changeCirclePosition;
        HoverObjects.instance.OnPathPointHovered += changeColor;
    }

    private void OnDisable()
    {
        DefinePath.instance.OnPathPositionChanged -= changeCirclePosition;
        HoverObjects.instance.OnPathPointHovered -= changeColor;
    }

    // change circle position when its corresponding point is relocated
    void changeCirclePosition(int currPathNum, int currPointNum, Vector3 distance) {
        // event is received for all circles in the scene, so first check that this is the corresponding one
        if(pointNum == currPointNum && pathNum == currPathNum)
            transform.position = transform.position - distance;
    }

    // change circle color when its corresponding point changes its color
    void changeColor(int currPathNum, int currPointNum, Color color)
    {
        // event is received for all circles in the scene, so first check that this is the corresponding one
        if (pointNum == currPointNum && pathNum == currPathNum)
            HoverObjects.instance.changeColorMaterials(gameObject, color, false);
    }
}
