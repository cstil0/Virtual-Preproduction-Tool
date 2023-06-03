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
    //[SerializeField] GameObject miniCamera;
    public List<Vector3> pathPositions;
    public List<Vector3> pathRotations;
    //public List<Quaternion> pathRotations;
    // relate each path ID with the start and end positions in the pathPositions list
    //public Dictionary<int, int[]> pathStartEnd;
    //public float posSpeed = 10.0f;
    //public float rotSpeed = 10.0f;
    float pathLength;
    float currPathPosition;
    float lastRotFactor = 0;

    public Vector3 startPosition;
    //Vector3 startDiffPosition;
    public Quaternion startRotation;

    //Animator animator;

    public GameObject pathContainer;

    bool isPlaying = false;
    bool secondaryIndexTriggerDown = false;
    bool XButtonDown = false;
    //bool AButtonDown = false;
    //bool BButtonDown = false;
    //bool newPathInstantiated = false;
    public bool triggerOn = false;
    public bool isSelectedForPath = false;
    public bool isPointOnTrigger = false;
    public bool isMiniCameraOnTrigger = false;
    // last local path ID created in this character
    [SerializeField] int lastCharacterPathID = 0;
    [SerializeField] int currentSelectedPath = 0;
    private float currPathgPosition = 0;

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.gameObject.layer == 3)
    //        triggerOn = true;
    //}

    //private void OnTriggerExit(Collider other)
    //{
    //    if (other.gameObject.layer == 3)
    //        triggerOn = false;
    //}
    //private void OnEnable()
    //{
    //    DirectorPanelManager.instance.OnPlayPath += playLinePath;
    //    DirectorPanelManager.instance.OnStopPath += stopLinePath;
    //    UDPReceiver.instance.OnChangeItemColor += changeItemColorDirector;
    //    UDPReceiver.instance.OnChangePathColor += changePathColorDirector;
    //}
    private void OnDisable()
    {
        DirectorPanelManager.instance.OnPlayPath -= playLinePath;
        DirectorPanelManager.instance.OnStopPath -= stopLinePath;
        UDPReceiver.instance.OnChangeItemColor -= changeItemColorDirector;
        UDPReceiver.instance.OnChangePathColor -= changePathColorDirector;

    }

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

    public static float InverseLerp(Vector3 pointA, Vector3 pointB, Vector3 middlePoint)
    {
        Vector3 distanceAB = pointB - pointA;
        Vector3 distanceAM = middlePoint - pointA;
        return Vector3.Dot(distanceAM, distanceAB) / Vector3.Dot(distanceAB, distanceAB);
    }

    //public static float InverseLerp(Vector3 pointA, Vector3 pointB, Vector3 middlePoint)
    //{
    //    float distanceAB = Vector3.Distance(pointB, pointA);
    //    float distanceAM = Vector3.Distance(middlePoint, pointA);
    //    return distanceAM/ distanceAB;
    //}

    // Start is called before the first frame update
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
        //pathPositions = new List<Vector3>();
        //pathStartEnd = new Dictionary<int, int[]>();

        startPosition = cinemachineVirtualCamera.transform.position;
        startRotation = cinemachineVirtualCamera.transform.rotation;

        pathPositions = new List<Vector3>();
        pathRotations = new List<Vector3>();

        //if (gameObject.GetComponent<Animator>())
        //    animator = gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger) && !isPlaying)
        {
            if (!secondaryIndexTriggerDown && triggerOn)
            {
                secondaryIndexTriggerDown = true;
                isSelectedForPath = !isSelectedForPath;

                if (pathPositions.Count == 0)
                {
                    startPosition = cinemachineVirtualCamera.transform.position;
                    StartCoroutine(defineNewPathPoint(gameObject.transform.position, gameObject.transform.rotation));
                }

                changePathColor();
            }
            else if (!secondaryIndexTriggerDown && isSelectedForPath && !isPointOnTrigger & !isMiniCameraOnTrigger && HoverObjects.instance.currentItemCollider == gameObject)
            {
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

        if (isPlaying && pathPositions.Count >= 2)
        {
            cinemachineSmoothPath.m_Appearance.width = 0.2f;

            CinemachineSmoothPath.Waypoint[] wayPoints = cinemachineSmoothPath.m_Waypoints;
            pathLength = wayPoints.Length;


            float floorPathPos = Mathf.Floor(currPathPosition);
            Vector3 currTargetPos = pathPositions[(int)floorPathPos + 1];
            //Quaternion currTargetRot = pathRotations[(int)floorPathPos + 1];
            Vector3 lastTargetPos = pathPositions[(int)floorPathPos];
            float distance = Vector3.Distance(currTargetPos, lastTargetPos);

            float step = speed * Time.deltaTime;

            // first compute the real position that we want to reach
            Vector3 currRealPos = MoveTowardsCustom(lastTargetPos, currTargetPos, step);
            // then compute the interpolation factor that we need to get to that real position
            float tempCurrPathPosition = currPathPosition + InverseLerp(lastTargetPos, currTargetPos, currRealPos);

            if (tempCurrPathPosition < pathLength - 1)
            {
                currPathPosition = tempCurrPathPosition;
                cinemachineTrackedDolly.m_PathPosition = currPathPosition;

                floorPathPos = Mathf.Floor(currPathPosition);
                float factor = currPathPosition - floorPathPos;

                Vector3 currTargetRot = pathRotations[(int)floorPathPos + 1];
                //Quaternion currTargetRot = pathRotations[(int)floorPathPos + 1];
                Vector3 lastTargetRot = pathRotations[(int)floorPathPos];

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
        Vector3 angDiffInv = vector3Abs(angDiff) - new Vector3(360.0f, 360.0f, 360.0f);

        float minAngDiffX = angDiff.x;
        float minAngDiffY = angDiff.y;
        float minAngDiffZ = angDiff.z;
        
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

        for (int i = 0; i < cinemachinePoints.Length; i++)
        {
            CinemachineSmoothPath.Waypoint newWayPoint;
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

        Vector3 newPointCinemachineWrong = startPosition - newPoint;
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

            yield return new WaitForSeconds(0.2f);

            UDPSender.instance.sendPointPath(gameObject, newPoint);
            yield return new WaitForSeconds(0.1f);
            UDPSender.instance.sendRotationPath(gameObject, Quaternion.Euler(newRotVec));
        }

        //GameObject newMiniCamera = Instantiate(miniCamera);
    }

    void addBezierPointsLineRenderer(LineRenderer lineRenderer, CinemachineSmoothPath cinemachineSmoothPath, float pathLength)
    {
        float count = pathLength - 1;
        for (int i = 1; i < cinemachineSmoothPath.m_Resolution + 1; i++)
        {
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

        int count = (pointNum + 1) * resolution;
        for (int i = resolution - 1; i >= 0 ; i--)
        {
            pathPositionsList.RemoveAt(count);
            count -= 1;
        }

        pathPositionsArray = pathPositionsList.ToArray();
        lineRenderer.SetPositions(pathPositionsArray);
        lineRenderer.positionCount = pathPositionsArray.Length;

        // relocate its following points in case it is not the last point of the movement
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
                // if first and second point are the same pass directly to second one
                try
                {
                    if (pathPositions[0] == pathPositions[1])
                        currPathPosition = 1;
                    else
                        currPathPosition = 0;
                }
                catch (Exception e) { currPathPosition = 0;  }
            }
            
            cinemachineTrackedDolly.m_PathPosition = currPathPosition;

            rotationController.transform.rotation = startRotation;

            GameObject dollyTracker = cinemachineSmoothPath.gameObject;
            dollyTracker.transform.rotation = startRotation;
            //startPosition = dollyTracker.transform.position;

            //Transform pathTransform = pathContainer.transform;

            //GameObject line = pathTransform.GetChild(0).gameObject;
            //line.GetComponent<LineRenderer>().enabled = false;

            //for (int i = 1; i < pathTransform.childCount; i++)
            //{
            //    GameObject currPoint = pathTransform.GetChild(i).gameObject;
            //    currPoint.GetComponent<MeshRenderer>().enabled = false;
            //}
            hideShowPath(false);
        }

        Camera udpSenderCamera = UDPSender.instance.screenCamera;

        isPlaying = !isPlaying;
    }

    void stopLinePath()
    {
        isPlaying = false;
        cinemachineTrackedDolly.m_PathPosition = 0;
        rotationController.transform.rotation = startRotation;
        currPathPosition = 0;

        hideShowPath(true);

        //GameObject[] paths = GameObject.FindGameObjectsWithTag("PathContainer");

        //for (int i = 0; i < paths.Length; i++)
        //{
        //    Transform currPath = paths[i].transform;

        //    for (int j = 0; j < currPath.childCount; j++)
        //    {
        //        GameObject pathObject = currPath.GetChild(j).gameObject;

        //        if (pathObject.name.Contains("Line"))
        //            pathObject.GetComponent<LineRenderer>().enabled = true;

        //        else if (pathObject.name.Contains("Point"))
        //            pathObject.SetActive(true);
        //    }
        //}

        Camera udpSenderCamera = UDPSender.instance.screenCamera;
    }

    void hideShowPath(bool isHidden)
    {
        if (pathContainer != null)
        {
            Transform pathTransform = pathContainer.transform;

            GameObject line = pathTransform.GetChild(0).gameObject;
            line.GetComponent<LineRenderer>().enabled = isHidden;

            for (int i = 1; i < pathTransform.childCount; i++)
            {
                GameObject currPoint = pathTransform.GetChild(i).gameObject;
                currPoint.SetActive(isHidden);
                //currPoint.GetComponent<MeshRenderer>().enabled = true;
            }
        }

    }

    public void deletePathPoint(int pointNum, bool deleteLine=true)
    {
        pathPositions.RemoveAt(pointNum - 1);

        if (deleteLine)
            DefinePath.instance.deletePointFromPath(pathContainer, pointNum);

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
        // inverted direction is needed for the way at which cinemachine points are saved
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
        while (pathContainer == null) yield return null;

        DefinePath.instance.changePathColor(pathContainer, color, false);
        Debug.Log("CHANGING PATH COLOR " + gameObject.name + " " + UnityEngine.ColorUtility.ToHtmlStringRGBA(color));
    }

    Vector3 getPositiveRotation(Quaternion rot)
    {
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
