using Cinemachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using TMPro;
using Unity.Netcode;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;

public class FollowPathCamera : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera cinemachineVirtualCamera;
    private CinemachineTrackedDolly cinemachineTrackedDolly;
    [SerializeField] CinemachineSmoothPath cinemachineSmoothPath;
    [SerializeField] GameObject rotationController;
    float speed = 0.005f;

    public GameObject handController;
    //[SerializeField] GameObject miniCamera;
    public List<Vector3> pathPositions;
    public List<Vector3> pathRotations;
    //public List<Quaternion> pathRotations;
    // relate each path ID with the start and end positions in the pathPositions list
    //public Dictionary<int, int[]> pathStartEnd;
    public float posSpeed = 10.0f;
    public float rotSpeed = 10.0f;
    float pathLength;
    float currPathPosition;
    float lastRotFactor = 0;

    Vector3 startPosition;
    //Vector3 startDiffPosition;
    Quaternion startRotation;

    //Animator animator;

    GameObject pathContainer;

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
        if (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger))
        {
            if (!secondaryIndexTriggerDown && triggerOn)
            {
                secondaryIndexTriggerDown = true;
                isSelectedForPath = !isSelectedForPath;
                startPosition = gameObject.transform.position;
                defineNewPathPoint(gameObject.transform.position, gameObject.transform.rotation);
            }
            else if (!secondaryIndexTriggerDown && isSelectedForPath && !isPointOnTrigger & !isMiniCameraOnTrigger)
            {
                secondaryIndexTriggerDown = true;
                defineNewPathPoint(handController.transform.position, handController.transform.rotation);
            }
        }
        else
        {
            secondaryIndexTriggerDown = false;
        }

        if (OVRInput.Get(OVRInput.RawButton.X))
        {
            if (!XButtonDown)
            {
                XButtonDown = true;
                playLinePath();
            }
        }
        else
            XButtonDown = false;

        if (Input.GetKeyDown(KeyCode.P))
        {
            playLinePath();
        }
        
        else if (Input.GetKeyDown(KeyCode.S) || OVRInput.Get(OVRInput.RawButton.Y))
        {
            stopLinePath();
        }
        else if (Input.GetKeyDown(KeyCode.M) && isSelectedForPath)
        {
            posSpeed += 0.1f;
            rotSpeed += 0.1f;
        }
        else if (Input.GetKeyUp(KeyCode.N) && isSelectedForPath)
        {
            posSpeed -= 0.1f;
            rotSpeed -= 0.1f;
        }

        //if (isPlaying && (posCount < pathPositions.Count))
        //{
        //    // avoid errors if rotation and positions are not fully synchronized
        //    Vector3 currTargetPos = pathPositions[pathPositions.Count - 1];
        //    Quaternion currTargetRot = pathRotations[pathRotations.Count - 1];
        //    if (pointsCount < pathPositions.Count)
        //        currTargetPos = pathPositions[pointsCount];
        //    if (pointsCount < pathRotations.Count)
        //        currTargetRot = pathRotations[pointsCount];

        //    if (animator != null)
        //        animator.SetFloat("Speed", posSpeed, 0.05f, Time.deltaTime);

        //    if (ModesManager.instance.role == ModesManager.eRoleType.ASSISTANT)
        //        move(currTargetPos, currTargetRot);

        //    if (gameObject.transform.position == currTargetPos)
        //        pointsCount++;
        //}
        //else
        //{
        //    if (animator != null)
        //        // do smooth transition from walk to idle taking the delta time
        //        animator.SetFloat("Speed", 0, 0.05f, Time.deltaTime);
        //    isPlaying = false;
        //}

        float tempCurrPathPosition = currPathPosition + speed;
        if (isPlaying && tempCurrPathPosition < pathLength)
        {
            currPathPosition = tempCurrPathPosition;
            cinemachineTrackedDolly.m_PathPosition = currPathPosition;

            /// rotate the empty object that is inside the camera so that the dolly tracker follows it
            // compute the interpolation factor according to the current position defined by the dolly tracker
            float floorPathPos = Mathf.Floor(currPathPosition);
            float factor = currPathPosition - floorPathPos;

            //if (factor == 0)
            //    lastRotFactor = 0;

            Vector3 currTargetRot = pathRotations[(int)floorPathPos + 1];
            //Quaternion currTargetRot = pathRotations[(int)floorPathPos + 1];
            Vector3 lastTargetRot = pathRotations[(int)floorPathPos];
            // find the minimum angle for each axis
            //Vector3 minAnglesDiff = new Vector3(Mathf.Min(angDiff.x, angDiffInv.x), Mathf.Min(angDiff.y, angDiffInv.y), Mathf.Min(angDiff.z, angDiffInv.z));

            // interpolate
            //Vector3 targetRot = lastTargetRot + minAnglesDiff * factor;

            //Vector3 targetRot = Vector3.Slerp(lastTargetRot, currTargetRot, factor);

            Vector3 angDiff = currTargetRot - lastTargetRot;
            Vector3 minAngDiff = findMinAngle(angDiff);
            Vector3 targetRot = lastTargetRot + (minAngDiff) * factor;

            //rotationController.transform.rotation = Quaternion.Euler(targetRot);

            //Quaternion lastTargetQuat = Quaternion.Euler(lastTargetRot);
            //Quaternion currTargetQuat = Quaternion.Euler(currTargetRot);
            ////Quaternion targetQuat = Quaternion.RotateTowards(gameObject.transform.rotation, currTargetQuat, rotSpeed * Time.deltaTime);
            //Vector3 rotDiff = (currTargetRot - lastTargetRot) * (factor - lastRotFactor);
            //float maxDelta = findMax(rotDiff.x, rotDiff.y, rotDiff.z);
            ////Quaternion targetQuat = Quaternion.RotateTowards(gameObject.transform.rotation, currTargetQuat, maxDelta * 0.1f);
            ////Vector3 targetRot = Vector3.RotateTowards(currTargetRot, last)
            rotationController.transform.rotation = Quaternion.Euler(targetRot);
            lastRotFactor = factor;
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

    public void defineNewPathPoint(Vector3 newPoint, Quaternion newRot)
    {
        CinemachineSmoothPath.Waypoint[] wayPoints = cinemachineSmoothPath.m_Waypoints;
        pathLength = wayPoints.Length;
        List<CinemachineSmoothPath.Waypoint> wayPointsList = new List<CinemachineSmoothPath.Waypoint>(wayPoints);
        Vector3 newPointCinemachine = newPoint - startPosition;
        CinemachineSmoothPath.Waypoint newWayPoint = new CinemachineSmoothPath.Waypoint();
        newWayPoint.position = newPointCinemachine;
        newWayPoint.roll = 0.0f;
        wayPointsList.Add(newWayPoint);
        wayPoints = wayPointsList.ToArray();
        cinemachineSmoothPath.m_Waypoints = wayPoints;

        pathPositions.Add(newPoint);
        pathRotations.Add(newRot.eulerAngles);

        if (pathLength == 0)
            pathContainer = DefinePath.instance.addPointToNewPath(newPoint, newRot, (int)pathLength, gameObject, DefinePath.instance.sphereCameraPrefab);
        else
            DefinePath.instance.addPointToExistentPath(pathContainer, newPoint, newRot, (int)pathLength, gameObject, DefinePath.instance.sphereCameraPrefab);
        
        UDPSender.instance.sendPointPath(gameObject, newPoint);

        //GameObject newMiniCamera = Instantiate(miniCamera);
    }

    void playLinePath()
    {
        GameObject[] paths = GameObject.FindGameObjectsWithTag("PathContainer");

        //for (int i = 0; i < paths.Length; i++)
        //{
        //    Transform currPath = paths[i].transform;

        //    for (int j = 0; j < currPath.childCount; j++)
        //    {
        //        GameObject pathObject = currPath.GetChild(j).gameObject;

        //        if (pathObject.name.Contains("Line"))
        //            pathObject.GetComponent<LineRenderer>().enabled = false;

        //        else if (pathObject.name.Contains("Point"))
        //            pathObject.SetActive(false);
        //    }
        //}

        // cameras are moved while defining their position and rotation
        // so we need them to go to the start location before playing their movement
        if (!isPlaying)
        {
            gameObject.transform.position = startPosition;
            gameObject.transform.rotation = startRotation;
        }

        Camera udpSenderCamera = UDPSender.instance.screenCamera;
        if (udpSenderCamera.transform.name == gameObject.transform.name && ModesManager.instance.role == ModesManager.eRoleType.DIRECTOR)
        {
            UDPSender.instance.sendChangeCamera();
            UDPSender.instance.SendPosRot();
        }

        isPlaying = !isPlaying;
        //isPlaying = true;
    }

    void stopLinePath()
    {
        isPlaying = false;
        gameObject.transform.position = startPosition;
        gameObject.transform.rotation = startRotation;
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
        if (udpSenderCamera.transform.name == gameObject.transform.name)
        {
            UDPSender.instance.sendChangeCamera();
            UDPSender.instance.SendPosRot();
        }
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
            DefinePath.instance.changePathColor(pathContainer, pathColor);
        }
    }

    public void deletePathPoint(int pointNum)
    {
        pathPositions.RemoveAt(pointNum);
        DefinePath.instance.deletePointFromPath(pathContainer, pointNum);
    }

    public void relocatePoint(int pointNum, Vector3 direction)
    {
        pathPositions[pointNum] += direction;

        Vector3 newPoint = pathPositions[pointNum];

        GameObject line = pathContainer.transform.Find("Line").gameObject;
        LineRenderer currLineRenderer = line.GetComponent<LineRenderer>();
        int pointsCount = currLineRenderer.positionCount;

        // relocate point
        Vector3[] pathPositionsArray = new Vector3[pathPositions.Count];
        currLineRenderer.GetPositions(pathPositionsArray);
        List<Vector3> pathPositionsList = pathPositionsArray.ToList<Vector3>();
        pathPositionsList[pointNum] = newPoint;

        // reassign
        pathPositionsArray = pathPositionsList.ToArray();
        currLineRenderer.SetPositions(pathPositionsArray);

        // relocate cinemachine point
        CinemachineSmoothPath.Waypoint[] cinemachinePoints = cinemachineSmoothPath.m_Waypoints;
        CinemachineSmoothPath.Waypoint newWayPoint = new CinemachineSmoothPath.Waypoint();
        newWayPoint.position = newPoint;

        cinemachinePoints[pointNum] = newWayPoint;
        cinemachineSmoothPath.m_Waypoints = cinemachinePoints;

        // relocate sphere
        Transform sphere = pathContainer.transform.GetChild(pointNum + 1);
        sphere.position = newPoint;
    }

    //void deleteCurrentPath()
    //{
    //    Transform pathButtons = gameObject.transform.Find("Paths buttons");
    //    Transform panel = pathButtons.GetChild(0);

    //    for (int i = 0; i < panel.childCount; i++)
    //    {
    //        GameObject pathButton = panel.GetChild(i).gameObject;
    //        if (!pathButton.GetComponent<Image>().enabled)
    //            continue;

    //        // get path ID
    //        GameObject text = pathButton.transform.GetChild(0).gameObject;
    //        string pathName = text.GetComponent<TextMeshProUGUI>().text;
    //        int pathID = int.Parse(pathName.Split(" ")[1]);
    //        if (i != currentSelectedPath - 1)
    //            continue;

    //        text.GetComponent<TextMeshProUGUI>().enabled = false;
    //        pathButton.GetComponent<Image>().enabled = false;
    //        GameObject[] lines = GameObject.FindGameObjectsWithTag("Line");

    //        foreach (GameObject line in lines)
    //        {
    //            if (!line.name.Contains("Path " + pathID))
    //                continue;

    //            Destroy(line.gameObject);
    //            break;
    //        }

    //        //int globalSelectedID = getGlobalPathID(currentSelectedPath);
    //        int[] startEnd = pathStartEnd[currentSelectedPath];
    //        int startPos = startEnd[0];
    //        int endPos = startEnd[1];
    //        // delete path points in the character
    //        List<int> removePositions = new List<int>();
    //        for (int j = 0; j < pathPositions.Count; j++)
    //        {
    //            if (j >= startPos && j <= endPos)
    //                removePositions.Add(j);
    //        }

    //        // this is needed since it is not possible to remove items while iterating the same array
    //        // ESTARIA BÉ TORNAR-HO A PROVAR AMB LO DE BAIX
    //        for (int j = removePositions.Count - 1; j >= 0; j--)
    //            pathPositions.RemoveAt(j);

    //        // update start and end positions of each following path
    //        pathStartEnd.Remove(currentSelectedPath);
    //        int removedPathSize = endPos - startPos + 1;
    //        int removeStartEnd = -1;
    //        foreach (KeyValuePair<int, int[]> startEndPos in pathStartEnd)
    //        {
    //            if (startEndPos.Key <= currentSelectedPath)
    //                continue;

    //            removeStartEnd = startEndPos.Key;
    //        }

    //        if (removedPathSize != -1)
    //            pathStartEnd.Remove(removeStartEnd);

    //        break;
    //    }
    //}

    //void relocatePathButtons()
    //{
    //    Transform pathButtons = gameObject.transform.Find("Paths buttons");
    //    Transform panel = pathButtons.GetChild(0);

    //    for (int i = 0; i < panel.childCount; i++)
    //    {
    //        if (i <= currentSelectedPath - 1)
    //            continue;

    //        GameObject currentPathButton = panel.GetChild(i).gameObject;
    //        if (!currentPathButton.GetComponent<Image>().enabled)
    //            continue;

    //        try
    //        {
    //            // first, get current ID and colors
    //            GameObject currentText = currentPathButton.transform.GetChild(0).gameObject;
    //            string currentPathName = currentText.GetComponent<TextMeshProUGUI>().text;
    //            int currentPathID = int.Parse(currentPathName.Split(" ")[1]);
    //            ColorBlock currentButtonColors = currentPathButton.GetComponent<Button>().colors;
    //            if (i == currentSelectedPath - 1)
    //                currentButtonColors.normalColor = DefinePath.instance.hoverLineColor;

    //            // then, assign them to the previous one
    //            GameObject previousPathButton = panel.GetChild(i - 1).gameObject;
    //            GameObject previousText = previousPathButton.transform.GetChild(0).gameObject;
    //            string previousPathName = "Path " + currentPathID;
    //            previousText.GetComponent<TextMeshProUGUI>().text = previousPathName;
    //            previousPathButton.GetComponent<Button>().colors = currentButtonColors;

    //            previousText.GetComponent<TextMeshProUGUI>().enabled = true;
    //            previousPathButton.GetComponent<Image>().enabled = true;

    //            if (i == lastCharacterPathID - 1)
    //            {
    //                currentText.GetComponent<TextMeshProUGUI>().enabled = false;
    //                currentPathButton.GetComponent<Image>().enabled = false;
    //            }

    //        }
    //        catch (Exception e) { }

    //    }
    //    lastCharacterPathID -= 1;
    //}
}
