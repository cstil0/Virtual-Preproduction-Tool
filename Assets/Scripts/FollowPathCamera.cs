using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Transactions;
using TMPro;
using Unity.Netcode;
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
    Quaternion startRotation;

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
    private float currPathgPosition;

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
    private void OnEnable()
    {
        DirectorPanelManager.instance.OnPlayPath += playLinePath;
        DirectorPanelManager.instance.OnStopPath += stopLinePath;
    }
    private void OnDisable()
    {
        DirectorPanelManager.instance.OnPlayPath -= playLinePath;
        DirectorPanelManager.instance.OnStopPath -= stopLinePath;
    }

    //public Vector3 MoveTowardsCustom(Vector3 current, Vector3 target, float maxDistanceDelta)
    //{
    //    float num = target.x - current.x;
    //    float num2 = target.y - current.y;
    //    float num3 = target.z - current.z;
    //    float num4 = num * num + num2 * num2 + num3 * num3;
    //    if (num4 == 0f || (maxDistanceDelta >= 0f && num4 <= maxDistanceDelta * maxDistanceDelta))
    //    {
    //        return target;
    //    }

    //    float num5 = (float)Math.Sqrt(num4);
    //    return new Vector3(current.x + num / num5 * maxDistanceDelta, current.y + num2 / num5 * maxDistanceDelta, current.z + num3 / num5 * maxDistanceDelta);
    //}

    //void move(Vector3 targetPoint, Quaternion targetRot)
    //{
    //    Vector3 currentPos = gameObject.transform.position;
    //    Quaternion currentRot = gameObject.transform.rotation;
    //    Vector3 targetDirection = targetPoint - currentPos;

    //    float posStep = posSpeed * Time.deltaTime;

    //    Vector3 newPos = MoveTowardsCustom(currentPos, targetPoint, posStep);
    //    gameObject.transform.position = newPos;
    //    Quaternion newRot = Quaternion.RotateTowards(currentRot, targetRot, rotSpeed);
    //    gameObject.transform.rotation = newRot;
    //}

    // Start is called before the first frame update
    void Start()
    {
        cinemachineTrackedDolly = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineTrackedDolly>();
        currPathPosition = 0.0f;
        pathLength = cinemachineTrackedDolly.m_Path.MaxPos;

        handController = GameObject.Find("RightHandAnchor");
        //pathPositions = new List<Vector3>();
        //pathStartEnd = new Dictionary<int, int[]>();

        startPosition = gameObject.transform.position;
        startRotation = gameObject.transform.rotation;

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
                    startPosition = gameObject.transform.position;
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
            if (triggerOn)
            {
                startPosition = cinemachineVirtualCamera.transform.position;
                startRotation = rotationController.transform.rotation;
            }
        }

        //if (OVRInput.Get(OVRInput.RawButton.X))
        //{
        //    if (!XButtonDown)
        //    {
        //        XButtonDown = true;
        //        playLinePath();
        //    }
        //}
        //else
        //    XButtonDown = false;

        //if (OVRInput.Get(OVRInput.RawButton.Y))
        //{
        //    stopLinePath();
        //}

        if (Input.GetKeyDown(KeyCode.P))
            playLinePath();
        else if (Input.GetKeyDown(KeyCode.S))
            stopLinePath();

        if (isPlaying)
        {
            cinemachineSmoothPath.m_Appearance.width = 0.2f;

            CinemachineSmoothPath.Waypoint[] wayPoints = cinemachineSmoothPath.m_Waypoints;
            pathLength = wayPoints.Length;

            float step = speed * Time.deltaTime;

            float tempCurrPathPosition = currPathPosition + step;

            if (tempCurrPathPosition < pathLength - 1)
            {
                currPathPosition = tempCurrPathPosition;
                cinemachineTrackedDolly.m_PathPosition = currPathPosition;

                float floorPathPos = Mathf.Floor(currPathPosition);
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

    public IEnumerator defineNewPathPoint(Vector3 newPoint, Quaternion newRot, bool instantiatePos = true)
    {
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
            pathRotations.Add(newRot.eulerAngles);
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
        pathRotations.Add(newRot.eulerAngles);

        if (instantiatePos)
        {
            if (pathLength == 0)
            {
                List<GameObject> containers= DefinePath.instance.addPointToNewPath(newPoint, newRot, (int)pathLength, gameObject, true);
                pathContainer = containers[0];
            }
            else
                DefinePath.instance.addPointToExistentPath(pathContainer, newPoint, newRot, (int)pathLength - 1, gameObject, true);

            yield return new WaitForSeconds(1.0f);

            UDPSender.instance.sendPointPath(gameObject, newPoint);
            yield return new WaitForSeconds(0.1f);
            UDPSender.instance.sendRotationPath(gameObject, newRot);
        }

        //GameObject newMiniCamera = Instantiate(miniCamera);
    }

    void removeCinemachinePoints()
    {
        CinemachineSmoothPath.Waypoint[] wayPoints = cinemachineSmoothPath.m_Waypoints;
        List<CinemachineSmoothPath.Waypoint> wayPointsList = new List<CinemachineSmoothPath.Waypoint>(wayPoints);

        while (wayPointsList.Count > 0) {
            wayPointsList.RemoveAt(0);
            wayPoints = wayPointsList.ToArray();
            cinemachineSmoothPath.m_Waypoints = wayPoints;
        }
    }

    void playLinePath()
    {
        // need them to go to the start location before playing their movement
        if (!isPlaying)
        {
            cinemachineTrackedDolly.m_PathPosition = 0;
            rotationController.transform.rotation = startRotation;
            //gameObject.transform.position = startPosition;
            //gameObject.transform.rotation = startRotation;

            GameObject dollyTracker = cinemachineSmoothPath.gameObject;
            dollyTracker.transform.rotation = startRotation;
            //startPosition = dollyTracker.transform.position;

            GameObject[] paths = GameObject.FindGameObjectsWithTag("PathContainer");

            //foreach (GameObject path in paths)
            //{
            //    if (path.name != "Path " + gameObject.name)
            //        continue;


            //    //removeCinemachinePoints();
            //    CinemachineSmoothPath.Waypoint[] wayPoints = new CinemachineSmoothPath.Waypoint[0];
            //    List<CinemachineSmoothPath.Waypoint> wayPointsList = new List<CinemachineSmoothPath.Waypoint>();
            //    CinemachineSmoothPath.Waypoint newWayPoint = new CinemachineSmoothPath.Waypoint();

            //    newWayPoint.position = new Vector3(0.0f, 0.0f, 0.0f);
            //    newWayPoint.roll = 0.0f;
            //    wayPointsList.Add(newWayPoint);
            //    for (int i = 0; i < path.transform.childCount - 1; i++)
            //    {
            //        Transform currPoint = path.transform.GetChild(i + 1);

            //        Vector3 currPosition = currPoint.position;
            //        Quaternion currRotation = currPoint.GetChild(0).rotation;
            //        Debug.Log("CURR POS: " + currPosition);

            //        ////Vector3 currPosition = currPoint.GetChild(1).position;
            //        pathPositions[i + 1] = currPosition;
            //        pathRotations[i + 1] = currRotation.eulerAngles;

            //        // update cinemachine points
            //        Vector3 newPointCinemachineWrong = startPosition - currPosition;
            //        Vector3 newPointCinemachine = new Vector3(newPointCinemachineWrong.x, - newPointCinemachineWrong.y, newPointCinemachineWrong.z);
            //        newWayPoint = new CinemachineSmoothPath.Waypoint();
            //        newWayPoint.position = newPointCinemachine;
            //        newWayPoint.roll = 0.0f;
            //        wayPointsList.Add(newWayPoint);
            //        wayPoints = wayPointsList.ToArray();
            //        cinemachineSmoothPath.m_Waypoints = wayPoints;
                //}
            //}
        }

        Camera udpSenderCamera = UDPSender.instance.screenCamera;
        //if (udpSenderCamera.transform.name == gameObject.transform.name && ModesManager.instance.role == ModesManager.eRoleType.ASSISTANT)
        //{
        //    //UDPSender.instance.SendPosRot();
        //}

        isPlaying = !isPlaying;
    }

    void stopLinePath()
    {
        isPlaying = false;
        cinemachineTrackedDolly.m_PathPosition = 0;
        rotationController.transform.rotation = startRotation;
        //gameObject.transform.position = startPosition;
        //gameObject.transform.rotation = startRotation;
        currPathPosition = 0;

        GameObject[] paths = GameObject.FindGameObjectsWithTag("PathContainer");

        for (int i = 0; i < paths.Length; i++)
        {
            Transform currPath = paths[i].transform;

            for (int j = 0; j < currPath.childCount; j++)
            {
                GameObject pathObject = currPath.GetChild(j).gameObject;

                if (pathObject.name.Contains("Line"))
                    pathObject.GetComponent<LineRenderer>().enabled = true;

                else if (pathObject.name.Contains("Point"))
                    pathObject.SetActive(true);
            }
        }

        Camera udpSenderCamera = UDPSender.instance.screenCamera;
        //if (udpSenderCamera.transform == gameObject.transform && ModesManager.instance.role == ModesManager.eRoleType.ASSISTANT)
        //{
        //    UDPSender.instance.SendPosRot();
        //}
    }

    int getGlobalPathID(int localPathID)
    {
        Transform pathButtons = gameObject.transform.Find("Paths buttons");
        Transform panel = pathButtons.GetChild(0);
        for (int i = 0; i < panel.transform.childCount; i++)
        {
            GameObject currentChild = panel.transform.GetChild(i).gameObject;
            GameObject text = currentChild.transform.GetChild(0).gameObject;
            string pathName = text.GetComponent<TextMeshProUGUI>().text;
            return int.Parse(pathName.Split(" ")[1]);
        }

        return 0;
    }

    void hoverCurrentPath()
    {
        Transform pathButtons = gameObject.transform.Find("Paths buttons");
        Transform panel = pathButtons.GetChild(0);

        for (int i = 0; i < panel.childCount; i++)
        {
            GameObject pathButton = panel.GetChild(i).gameObject;
            if (!pathButton.GetComponent<Image>().enabled)
                continue;

            // get path ID
            GameObject text = pathButton.transform.GetChild(0).gameObject;
            string pathName = text.GetComponent<TextMeshProUGUI>().text;
            int pathID = int.Parse(pathName.Split(" ")[1]);
            Color pathColor = new Color();
            if (i == currentSelectedPath - 1)
                pathColor = DefinePath.instance.hoverLineColor;
            else
                pathColor = DefinePath.instance.selectedLineColor;

            ColorBlock buttonColors = pathButton.GetComponent<Button>().colors;
            buttonColors.normalColor = pathColor;
            pathButton.GetComponent<Button>().colors = buttonColors;
            DefinePath.instance.changePathColor(pathContainer, pathColor, true);
        }
    }

    public void deletePathPoint(int pointNum, bool deleteLine=true)
    {
        pathPositions.RemoveAt(pointNum);
        if (deleteLine)
            DefinePath.instance.deletePointFromPath(pathContainer, pointNum);
    }

    public void relocatePoint(int pointNum, Vector3 direction, bool moveSphere)
    {
        pathPositions[pointNum + 1] += direction;

        Vector3 newPoint = pathPositions[pointNum + 1];

        // relocate cinemachine point
        CinemachineSmoothPath.Waypoint[] cinemachinePoints = cinemachineSmoothPath.m_Waypoints;
        CinemachineSmoothPath.Waypoint newWayPoint = new CinemachineSmoothPath.Waypoint();
        newWayPoint.position = cinemachinePoints[pointNum + 1].position + direction;

        cinemachinePoints[pointNum + 1] = newWayPoint;
        cinemachineSmoothPath.m_Waypoints = cinemachinePoints;

        // relocate point in line renderer
        GameObject line = pathContainer.transform.Find("Line").gameObject;
        LineRenderer currLineRenderer = line.GetComponent<LineRenderer>();
        int pointsCount = currLineRenderer.positionCount;

        Vector3[] pathPositionsArray = new Vector3[pathPositions.Count];
        currLineRenderer.GetPositions(pathPositionsArray);
        List<Vector3> pathPositionsList = pathPositionsArray.ToList<Vector3>();
        pathPositionsList[pointNum] = newPoint;

        // reassign
        pathPositionsArray = pathPositionsList.ToArray();
        currLineRenderer.SetPositions(pathPositionsArray);

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
        if (!isSelectedForPath)
            DefinePath.instance.changePathColor(pathContainer, DefinePath.instance.defaultLineColor, false);
        else
            DefinePath.instance.changePathColor(pathContainer, DefinePath.instance.selectedLineColor, true);
    }
}
