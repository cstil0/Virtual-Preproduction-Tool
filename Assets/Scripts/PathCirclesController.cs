using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathCirclesController : MonoBehaviour
{
    public int pathNum;
    public int pointNum;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
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

    void changeCirclePosition(int currPathNum, int currPointNum, Vector3 distance) {
        if(pointNum == currPointNum && pathNum == currPathNum)
            transform.position = transform.position - distance;
    }

    void changeColor(int currPathNum, int currPointNum, Color color)
    {
        if (pointNum == currPointNum && pathNum == currPathNum)
            HoverObjects.instance.changeColorMaterials(gameObject, color);
    }
}
