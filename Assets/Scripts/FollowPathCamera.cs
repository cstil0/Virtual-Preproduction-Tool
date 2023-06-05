using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FollowPathCamera : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera cinemachineVirtualCamera;
    private CinemachineTrackedDolly cinemachineTrackedDolly;
    public CinemachineSmoothPath cinemachineSmoothPath;
    [SerializeField] GameObject rotationController;
    public float speed = 0.005f;

    public GameObject handController;
    public List<Vector3> pathPositions;
    public List<Vector3> pathRotations;
    float pathLength;
    float currPathPosition;
    float lastRotFactor = 0;

    public Vector3 startPosition;
    public Quaternion startRotation;


    public GameObject pathContainer;

    bool isPlaying = false;
    bool secondaryIndexTriggerDown = false;
    bool XButtonDown = false;
    public bool triggerOn = false;
    public bool isSelectedForPath = false;
    public bool isPointOnTrigger = false;
    public bool isMiniCameraOnTrigger = false;
    // last local path ID created in this character
    [SerializeField] int lastCharacterPathID = 0;
    [SerializeField] int currentSelectedPath = 0;
    private float currPathgPosition = 0;

    private void OnDisable()
    {
        DirectorPanelManager.instance.OnPlayPath -= playLinePath;
        DirectorPanelManager.instance.OnStopPath -= stopLinePath;
        UDPReceiver.instance.OnChangeItemColor -= changeItemColorDirector;
        UDPReceiver.instance.OnChangePathColor -= changePathColorDirector;

    }

    // extracted from Vector3.MoveTowards() method
    public Vector3 MoveTowardsCustom(Vector3 current, Vector3 target, float maxDistanceDelta)
    {
        float num = target.x - current.x;
        float num2 = target.y - current.y;
        float num3 = target.z - current.z;
        float num4 = num * num + num2 * num2 + num3 * num3;
        if (num4 == 0f || (maxDistanceDelta >= 0f && num4 <= maxDistanceDelta * maxDistanceDelta))
        {
            return target;
        }

        float num5 = (float)Math.Sqrt(num4);
        return new Vector3(current.x + num / num5 * maxDistanceDelta, current.y + num2 / num5 * maxDistanceDelta, current.z + num3 / num5 * maxDistanceDelta);
    }

    // inverse interpolation
    public static float InverseLerp(Vector3 pointA, Vector3 pointB, Vector3 middlePoint)
    {
        Vector3 distanceAB = pointB - pointA;
        Vector3 distanceAM = middlePoint - pointA;
        return Vector3.Dot(distanceAM, distanceAB) / Vector3.Dot(distanceAB, distanceAB);
    }

    void Start()
    {
        DirectorPanelManager.instance.OnPlayPath += playLinePath;
        DirectorPanelManager.instance.OnStopPath += stopLinePath;
        UDPReceiver.instance.OnChangeItemColor += changeItemColorDirector;
        UDPReceiver.instance.OnChangePathColor += changePathColorDirector;

        cinemachineTrackedDolly = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineTrackedDolly>();
        currPathPosition = 0.0f;
        pathLength = cinemachineTrackedDolly.m_Path.MaxPos;

        handController = GameObject.Find("RightHandAnchor");

        startPosition = cinemachineVirtualCamera.transform.position;
        startRotation = cinemachineVirtualCamera.transform.rotation;

        pathPositions = new List<Vector3>();
        pathRotations = new List<Vector3>();
    }

    void Update()
    {
        if (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger) && !isPlaying)
        {
            if (!secondaryIndexTriggerDown && triggerOn)
            {
                secondaryIndexTriggerDown = true;
                isSelectedForPath = !isSelectedForPath;

                // if camera was just selected and there are no points defined yet, define a new one
                if (pathPositions.Count == 0)
                {
                    startPosition = cinemachineVirtualCamera.transform.position;
                    StartCoroutine(defineNewPathPoint(gameObject.transform.position, gameObject.transform.rotation));
                }

                changePathColor();
            }
            else if (!secondaryIndexTriggerDown && isSelectedForPath && !isPointOnTrigger & !isMiniCameraOnTrigger && HoverObjects.instance.currentItemCollider == gameObject)
            {
                // define a new path point if camera is selected but controller is not touching it
                secondaryIndexTriggerDown = true;
                StartCoroutine(defineNewPathPoint(handController.transform.position, handController.transform.rotation));
            }
        }
        else
        {
            secondaryIndexTriggerDown = false;
        }

        if (OVRInput.Get(OVRInput.Button.SecondaryHandTrigger) && !isPlaying)
        {
            // when moving the camera reset its start position and rotation for the movement
            if (triggerOn && cinemachineVirtualCamera.transform.position != startPosition)
            {
                startPosition = cinemachineVirtualCamera.transform.position;
                startRotation = rotationController.transform.rotation;
                relocateCinemachinePoints(cinemachineSmoothPath, startPosition);
            }
        }

        if (Input.GetKeyDown(KeyCode.P))
            playLinePath();
        else if (Input.GetKeyDown(KeyCode.S))
            stopLinePath();

        // check that there are at least two points already defined since the first one determines the initial position
        if (isPlaying && pathPositions.Count >= 2)
        {
            cinemachineSmoothPath.m_Appearance.width = 0.2f;

            CinemachineSmoothPath.Waypoint[] wayPoints = cinemachineSmoothPath.m_Waypoints;
            pathLength = wayPoints.Length;


            float floorPathPos = Mathf.Floor(currPathPosition);
            Vector3 currTargetPos = pathPositions[(int)floorPathPos + 1];
            Vector3 lastTargetPos = pathPositions[(int)floorPathPos];
            float distance = Vector3.Distance(currTargetPos, lastTargetPos);

            float step = speed * Time.deltaTime;

            // first compute the real position that we want to reach using a constant step
            Vector3 currRealPos = MoveTowardsCustom(lastTargetPos, currTargetPos, step);
            // then compute the interpolation factor that we need to get to that real position
            float tempCurrPathPosition = currPathPosition + InverseLerp(lastTargetPos, currTargetPos, currRealPos);

            // check if we are already at the final point of the path
            if (tempCurrPathPosition < pathLength - 1)
            {
                currPathPosition = tempCurrPathPosition;
                cinemachineTrackedDolly.m_PathPosition = currPathPosition;

                // get the current point num without decimals
                floorPathPos = Mathf.Floor(currPathPosition);
                // compute the factor from 0 to 1 between the two current positions
                float factor = currPathPosition - floorPathPos;

                // get the two current rotations
                Vector3 currTargetRot = pathRotations[(int)floorPathPos + 1];
                Vector3 lastTargetRot = pathRotations[(int)floorPathPos];

                // compute the current angle by an interpolation using the cinemachine factor and apply it
                Vector3 angDiff = currTargetRot - lastTargetRot;
                Vector3 minAngDiff = findMinAngle(angDiff);
                Vector3 targetRot = lastTargetRot + (minAngDiff) * factor;

                rotationController.transform.rotation = Quaternion.Euler(targetRot);
                lastRotFactor = factor;
            }
        }
    }

    Vector3 vector3Abs(Vector3 vector)
    {
        return new Vector3(Math.Abs(vector.x), Math.Abs(vector.y), Math.Abs(vector.z));
    }

    Vector3 findMinAngle(Vector3 angDiff)
    {
        // compute the conjugate angle
        Vector3 angDiffInv = vector3Abs(angDiff) - new Vector3(360.0f, 360.0f, 360.0f);

        float minAngDiffX = angDiff.x;
        float minAngDiffY = angDiff.y;
        float minAngDiffZ = angDiff.z;
        
        // get the minimum one ignoring the negative sign
        if (Math.Abs(angDiffInv.x) < minAngDiffX)
            minAngDiffX = angDiffInv.x;
        if (Math.Abs(angDiffInv.y) < minAngDiffY)
            minAngDiffY = angDiffInv.y;
        if (Math.Abs(angDiffInv.z) < minAngDiffZ)
            minAngDiffZ = angDiffInv.z;

        return new Vector3(minAngDiffX, minAngDiffY, minAngDiffZ);
    }

    public void relocateCinemachinePoints(CinemachineSmoothPath cinemachineSmoothPath, Vector3 startPosition)
    {
        CinemachineSmoothPath.Waypoint[] cinemachinePoints = cinemachineSmoothPath.m_Waypoints;

        // iterate through all points in the cinemachine path and compute the new point considering the distance between the new initial position and the already defined point
        for (int i = 0; i < cinemachinePoints.Length; i++)
        {
            CinemachineSmoothPath.Waypoint newWayPoint;
            // first point is always (0,0,0) representing distance 0
            if (i == 0)
            {
                newWayPoint = new CinemachineSmoothPath.Waypoint();
                newWayPoint.position = new Vector3(0.0f, 0.0f, 0.0f);
                cinemachinePoints[i] = newWayPoint;
                cinemachineSmoothPath.m_Waypoints = cinemachinePoints;
            }
            else
            {
                newWayPoint = new CinemachineSmoothPath.Waypoint();
                Vector3 newPointWrong = startPosition - pathPositions[i - 1];
                Vector3 newPoint = new Vector3(newPointWrong.x, -newPointWrong.y, newPointWrong.z);
                newWayPoint.position = newPoint;
                cinemachinePoints[i] = newWayPoint;
                cinemachineSmoothPath.m_Waypoints = cinemachinePoints;
            }
        }
    }

    public IEnumerator defineNewPathPoint(Vector3 newPoint, Quaternion newRot, bool instantiatePos = true)
    {
        // avoid having negative angles at it is easier to work with positive ones
        Vector3 newRotVec = getPositiveRotation(newRot);


        CinemachineSmoothPath.Waypoint[] wayPoints = cinemachineSmoothPath.m_Waypoints;
        pathLength = wayPoints.Length;
        if (pathLength >= DefinePath.instance.maxCameraPoints)
        {
            // start vibration to indicate that no more points can be instantiated
            OVRInput.SetControllerVibration(1, 1, OVRInput.Controller.RTouch);
            yield return new WaitForSeconds(1f);
            // stop vibration
            OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
            yield break;
        }
        
        List<CinemachineSmoothPath.Waypoint> wayPointsList = new List<CinemachineSmoothPath.Waypoint>(wayPoints);

        // it is necessary to separate the first point which corresponding to 0,
        // so that the difference with the next one is correct in case the camera is moved from the first point
        if (pathLength == 0)
        {
            CinemachineSmoothPath.Waypoint firstWayPoint = new CinemachineSmoothPath.Waypoint();
            firstWayPoint.position = new Vector3(0.0f, 0.0f, 0.0f);
            firstWayPoint.roll = 0.0f;
            wayPointsList.Add(firstWayPoint);

            pathPositions.Add(newPoint);
            pathRotations.Add(newRotVec);
        }

        // the rest of points are computed as the difference between the actual point and the initial one
        Vector3 newPointCinemachineWrong = startPosition - newPoint;
        // y-axis needs to be flipped to obtain the desired position
        Vector3 newPointCinemachine = new Vector3(newPointCinemachineWrong.x, - newPointCinemachineWrong.y, newPointCinemachineWrong.z);
        CinemachineSmoothPath.Waypoint newWayPoint = new CinemachineSmoothPath.Waypoint();
        newWayPoint.position = newPointCinemachine;
        newWayPoint.roll = 0.0f;

        wayPointsList.Add(newWayPoint);
        wayPoints = wayPointsList.ToArray();
        cinemachineSmoothPath.m_Waypoints = wayPoints;

        pathPositions.Add(newPoint);
        pathRotations.Add(newRotVec);

        if (instantiatePos)
        {
            if (pathLength == 0)
            {
                List<GameObject> containers= DefinePath.instance.addPointToNewPath(newPoint, Quaternion.Euler(newRotVec), (int)pathLength, gameObject, true);
                pathContainer = containers[0];
            }
            else
            {
                // add bezier points to show the interpolated path to the user
                LineRenderer lineRenderer = pathContainer.transform.GetComponentInChildren<LineRenderer>();
                addBezierPointsLineRenderer(lineRenderer, cinemachineSmoothPath, pathLength);

                DefinePath.instance.addPointToExistentPath(pathContainer, newPoint, Quaternion.Euler(newRotVec), (int)pathLength - 1, gameObject, true);
            }

            // inform of the new position and rotation
            // wait to ensure that the point was already created and renamed in the client side
            yield return new WaitForSeconds(0.2f);
            UDPSender.instance.sendPointPath(gameObject, newPoint);
            yield return new WaitForSeconds(0.1f);
            UDPSender.instance.sendRotationPath(gameObject, Quaternion.Euler(newRotVec));
        }
    }

    void addBezierPointsLineRenderer(LineRenderer lineRenderer, CinemachineSmoothPath cinemachineSmoothPath, float pathLength)
    {
        float count = pathLength - 1;
        // iterate through all in-between positions generated in the cinemachine path from the last one to the new defined point
        for (int i = 1; i < cinemachineSmoothPath.m_Resolution + 1; i++)
        {
            // evaluate the real position of each in-between point and assign it to the line renderer
            float step = 1.0f / (float)cinemachineSmoothPath.m_Resolution;
            Vector3 bezierPosition = cinemachineSmoothPath.EvaluatePosition(count);
            lineRenderer.positionCount += 1;
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, bezierPosition);

            count += step;
        }
    }

    void removeBezierPointsLineRenderer(LineRenderer lineRenderer, CinemachineSmoothPath cinemachineSmoothPath, int pointNum)
    {
        int pointsCount = lineRenderer.positionCount;
        Vector3[] pathPositionsArray = new Vector3[pointsCount];
        lineRenderer.GetPositions(pathPositionsArray);
        // we cannot modify a linerenderer point, but we can copy them to a list, modify it and assign the list again
        List<Vector3> pathPositionsList = pathPositionsArray.ToList<Vector3>();

        int resolution = cinemachineSmoothPath.m_Resolution;

        // remove all line renderer positions previous to the deleted point
        int count = (pointNum + 1) * resolution;
        for (int i = resolution - 1; i >= 0 ; i--)
        {
            pathPositionsList.RemoveAt(count);
            count -= 1;
        }

        pathPositionsArray = pathPositionsList.ToArray();
        lineRenderer.SetPositions(pathPositionsArray);
        lineRenderer.positionCount = pathPositionsArray.Length;

        // relocate its following points in case it is not the last point of the movement to follow the new bezier curve
        CinemachineSmoothPath.Waypoint[] wayPoints = cinemachineSmoothPath.m_Waypoints;
        int pathLength = wayPoints.Length;
        if (pointNum < pathLength - 2)
        {
            float countFloat = pointNum + 1;
            for (int i = 1; i < resolution + 1; i++)
            {
                float step = 1.0f / (float)resolution;
                Vector3 bezierPosition = cinemachineSmoothPath.EvaluatePosition(countFloat);
                lineRenderer.SetPosition(pointNum * resolution + i, bezierPosition);

                countFloat += step;
            }
        }
    }

    void relocateBezierPointsLineRenderer(LineRenderer lineRenderer, CinemachineSmoothPath cinemachineSmoothPath, int pointNum, Vector3 newPoint)
    {
        int pointsCount = lineRenderer.positionCount;

        Vector3[] pathPositionsArray = new Vector3[pointsCount];
        lineRenderer.GetPositions(pathPositionsArray);

        int resolution = cinemachineSmoothPath.m_Resolution;

        CinemachineSmoothPath.Waypoint[] wayPoints = cinemachineSmoothPath.m_Waypoints;
        int pathLength = wayPoints.Length;

        // the start and ending positions to be re-evaluated depend if the relocated point is at the beginning or end of the movement
        int doubleResolution = resolution;
        if (pointNum < (pathLength - 2))
            doubleResolution *= 2;

        float countFloat = pointNum;
        if (pointNum <= 0)
        {
            countFloat = pointNum;
            doubleResolution = resolution;
        }

        // iterate through the previous and following lines, adjacent to the current point to evaluate their bezier points
        for (int i = 1; i < doubleResolution + 1; i++)
        {
            float step = 1.0f / (float)resolution;
            Vector3 bezierPosition = cinemachineSmoothPath.EvaluatePosition(countFloat);
            lineRenderer.SetPosition((pointNum - 1) * resolution + i, bezierPosition);

            countFloat += step;
        }
    }

    void playLinePath()
    {
        // need them to go to the start location before playing their movement
        if (!isPlaying)
        {
            // check if we are comming from stop
            if (currPathPosition == 0)
            {
                // if first and second point are the same, pass directly to second one. Instead it takes longer to start the movement
                try
                {
                    if (pathPositions[0] == pathPositions[1])
                        currPathPosition = 1;
                    else
                        currPathPosition = 0;
                }
                catch (Exception e) { currPathPosition = 0;  }
            }
            
            // continue movement from the current position
            cinemachineTrackedDolly.m_PathPosition = currPathPosition;
            rotationController.transform.rotation = Quaternion.Euler(pathRotations[(int)currPathPosition]);

            hideShowPath(false);
        }

        isPlaying = !isPlaying;
    }

    void stopLinePath()
    {
        // go back to the start position and rotation
        isPlaying = false;
        cinemachineTrackedDolly.m_PathPosition = 0;
        rotationController.transform.rotation = startRotation;
        currPathPosition = 0;

        hideShowPath(true);
    }

    void hideShowPath(bool isHidden)
    {
        // iterate through all points and line renderer and disable / enable them
        if (pathContainer != null)
        {
            Transform pathTransform = pathContainer.transform;

            GameObject line = pathTransform.GetChild(0).gameObject;
            line.GetComponent<LineRenderer>().enabled = isHidden;

            for (int i = 1; i < pathTransform.childCount; i++)
            {
                GameObject currPoint = pathTransform.GetChild(i).gameObject;
                currPoint.SetActive(isHidden);
            }
        }
    }

    public void deletePathPoint(int pointNum, bool deleteLine=true)
    {
        // remove point from local arrays
        pathPositions.RemoveAt(pointNum - 1);
        pathRotations.RemoveAt(pointNum - 1);

        if (deleteLine)
            DefinePath.instance.deletePointFromPath(pathContainer, pointNum);

        // remove point from cinemachine array
        CinemachineSmoothPath.Waypoint[] cinemachinePoints = cinemachineSmoothPath.m_Waypoints;

        List<CinemachineSmoothPath.Waypoint> cinemachinePointsList = cinemachinePoints.ToList();
        cinemachinePointsList.RemoveAt(pointNum + 1);
        cinemachinePoints = cinemachinePointsList.ToArray();
        cinemachineSmoothPath.m_Waypoints = cinemachinePoints;

        LineRenderer lineRenderer = pathContainer.transform.GetComponentInChildren<LineRenderer>();
        removeBezierPointsLineRenderer(lineRenderer, cinemachineSmoothPath, pointNum - 1);
    }

    public void relocatePoint(int pointNum, Vector3 direction, bool moveSphere, Vector3 directionInv)
    {
        // relocate point from local position array
        // inverted direction is needed because of the way at which cinemachine points are saved
        if (directionInv == new Vector3(0.0f,0.0f,0.0f))
            pathPositions[pointNum + 1] += direction;
        else
            pathPositions[pointNum + 1] += directionInv;

        Vector3 newPoint = pathPositions[pointNum + 1];

        // relocate cinemachine point
        CinemachineSmoothPath.Waypoint[] cinemachinePoints = cinemachineSmoothPath.m_Waypoints;
        CinemachineSmoothPath.Waypoint newWayPoint = new CinemachineSmoothPath.Waypoint();
        newWayPoint.position = cinemachinePoints[pointNum + 1].position + direction;

        cinemachinePoints[pointNum + 1] = newWayPoint;
        cinemachineSmoothPath.m_Waypoints = cinemachinePoints;

        // relocate point in line renderer
        LineRenderer lineRenderer = pathContainer.transform.GetComponentInChildren<LineRenderer>();
        relocateBezierPointsLineRenderer(lineRenderer,cinemachineSmoothPath, pointNum, newPoint);

        if (moveSphere)
        {
            // relocate sphere
            Transform sphere = pathContainer.transform.GetChild(pointNum + 1);
            sphere.position = newPoint;
        }
    }

    public void changeSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    public void changePathColor()
    {
        Color color = DefinePath.instance.defaultLineColor;
        if (isSelectedForPath)
            color = DefinePath.instance.selectedLineColor;

        DefinePath.instance.changePathColor(pathContainer, color, isSelectedForPath);

        if (ModesManager.instance.role == ModesManager.eRoleType.ASSISTANT)
            UDPSender.instance.sendChangePathColor(gameObject.name, UnityEngine.ColorUtility.ToHtmlStringRGBA(color));
    }

    private void changeItemColorDirector(string itemName, Color color)
    {
        if (itemName == gameObject.name)
            HoverObjects.instance.changeColorMaterials(gameObject, color, false);
    }

    private void changePathColorDirector(string itemName, Color color)
    {
        if (itemName == gameObject.name)
            StartCoroutine(changeColorWaitPathContainer(color));
    }

    IEnumerator changeColorWaitPathContainer(Color color)
    {
        // wait until path container is received and correctly stored
        while (pathContainer == null) yield return null;

        DefinePath.instance.changePathColor(pathContainer, color, false);
    }

    Vector3 getPositiveRotation(Quaternion rot)
    {
        // if angle is negative, compute its conjugate
        Vector3 newRot = rot.eulerAngles;
        if (newRot.x < 0)
            newRot.x = newRot.x + 360.0f;

        if (newRot.y < 0)
            newRot.y = newRot.x + 360.0f;

        if (newRot.z < 0)
            newRot.z = newRot.z + 360.0f;

        return newRot;
    }
}
