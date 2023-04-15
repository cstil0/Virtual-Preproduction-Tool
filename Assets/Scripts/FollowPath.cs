using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using TMPro;
using Unity.IO.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class FollowPath : MonoBehaviour
{
    public GameObject handController;
    public List<Vector3> pathPositions;
    // relate each path ID with the start and end positions in the pathPositions list
    //public Dictionary<int, int[]> pathStartEnd;
    public float posSpeed = 20.0f;
    public float rotSpeed = 7.0f;
    public int pointsCount;
    public int currPoint;
    public Vector3 startPosition;
    Vector3 startDiffPosition;
    Quaternion startRotation;

    Animator animator;

    public GameObject pathContainer;
    //int pathNum = -1;

    public bool isPlaying = false;
    bool secondaryIndexTriggerDown = false;
    //bool AButtonDown = false;
    //bool BButtonDown = false;
    bool newPathInstantiated = false;
    public bool triggerOn = false;
    public bool isSelectedForPath = false;
    public bool isPointOnTrigger = false;
    // last local path ID created in this character
    [SerializeField] int lastCharacterPathID = 0;
    [SerializeField] int currentSelectedPath = 0;

    [SerializeField] Vector3 characterRotationLeft = new Vector3(0, -5f, 0);
    [SerializeField] Vector3 characterRotationRight = new Vector3(0, 5f, 0);


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

    void move(Vector3 targetPoint)
    {
        Vector3 currentPos = gameObject.transform.position;
        Vector3 targetDirection = targetPoint - currentPos;

        float posStep = posSpeed * Time.deltaTime;
        float rotStep = rotSpeed * Time.deltaTime;

        Vector3 newPos = MoveTowardsCustom(currentPos, targetPoint, posStep);
        gameObject.transform.position = newPos;
        // if it is a camera there is no RotationScale script, and we do not want it to rotate with direction
        try
        {
            Vector3 originalRotation = gameObject.GetComponent<RotationScale>().rotation;

            // compute the new formard direction where we will rotate to
            // set y coordinate to 0 so that the rotation only takes into account the floor plane, and then it does not try to rotate to higher altitudes, which are where it starts doing weird things
            Vector3 targetDirectionXZ = new Vector3(targetDirection.x, 0.0f, targetDirection.z);
            Vector3 newforward = Vector3.RotateTowards(transform.forward, targetDirectionXZ, rotStep, 0.0f);
            // compute the new rotation using this forward
            gameObject.transform.rotation = Quaternion.LookRotation(newforward, new Vector3(0.0f, 1.0f, 0.0f));
            //gameObject.transform.rotation = Quaternion.LookRotation(new Vector3(originalRotation.x, newForward.y, originalRotation.z));
        } catch (Exception e) {
            Debug.LogError(e.Message);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        handController = GameObject.Find("RightHandAnchor");
        pathPositions = new List<Vector3>();
        //pathStartEnd = new Dictionary<int, int[]>();
        pointsCount = 0;

        startPosition = gameObject.transform.position;
        startRotation = gameObject.transform.rotation;

        if (gameObject.GetComponent<Animator>())
            animator = gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger) && !isPlaying)
        {
            if (!secondaryIndexTriggerDown && triggerOn)
            {
                //DefinePath.instance.startLine = false;
                secondaryIndexTriggerDown = true;
                // first touch will select the character, and the second one will unselect it
                isSelectedForPath = !isSelectedForPath;

                if(pointsCount == 0)
                {
                    StartCoroutine(defineNewPathPoint(handController.transform.position));
                    //pathNum = DefinePath.instance.getItemsCount();
                }

                if (!isSelectedForPath)
                    DefinePath.instance.changePathColor(pathContainer, DefinePath.instance.defaultLineColor);
                else
                    DefinePath.instance.changePathColor(pathContainer, DefinePath.instance.selectedLineColor);
            }

            else if (!secondaryIndexTriggerDown && isSelectedForPath && !isPointOnTrigger && HoverObjects.instance.currentItemCollider == gameObject)
            {
                secondaryIndexTriggerDown = true;
                StartCoroutine(defineNewPathPoint(handController.transform.position));
            }
        }
        else
        {
            newPathInstantiated = false;
            secondaryIndexTriggerDown = false;
        }


        if (triggerOn && !isPlaying) {
            if (OVRInput.Get(OVRInput.Button.SecondaryThumbstickLeft))
                rotateCharacter(characterRotationLeft);
            if (OVRInput.Get(OVRInput.Button.SecondaryThumbstickRight))
                rotateCharacter(characterRotationRight);
            if (gameObject.transform.position != startPosition)
            {
                startPosition = gameObject.transform.position;
                startRotation = gameObject.transform.rotation;
                startDiffPosition = handController.transform.position - startPosition;

                pathPositions[0] = startPosition;
                Transform firstPoint = pathContainer.transform.GetChild(0);
                firstPoint.position = startPosition;
            }
        }


        if (Input.GetKeyDown(KeyCode.P))// || OVRInput.Get(OVRInput.RawButton.X))
        {
            playLinePath();
        }
        else if (Input.GetKeyDown(KeyCode.S))// || OVRInput.Get(OVRInput.RawButton.Y))
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

        if (isPlaying && currPoint < pathPositions.Count)
        {
            Vector3 currTarget = pathPositions[currPoint];

            // QUE HI FA AQUÍ EL POSSPEED? NO HAURIA DE SER 0.1? AH POTSER ERA PER QUE VAGI UNA MICA MÉS RÀPID SI EL PERSONATGE TB?
            if (animator != null)
                animator.SetFloat("Speed", 0.1f, 0.05f, Time.deltaTime);
                //animator.SetFloat("Speed", posSpeed, 0.05f, Time.deltaTime);

            if (ModesManager.instance.role == ModesManager.eRoleType.ASSISTANT)
                move(currTarget);

            if (gameObject.transform.position == currTarget)
                currPoint++;
        }
        else
        {
            if (animator != null)
                // do smooth transition from walk to idle taking the delta time
                animator.SetFloat("Speed", 0.0f, 0.05f, Time.deltaTime);
        }
        

        // navigate through paths and delete them
        //if (isSelectedForPath && OVRInput.Get(OVRInput.RawButton.A))
        //{
        //    if (!AButtonDown)
        //    {
        //        AButtonDown = true;
        //        currentSelectedPath += 1;
        //        if (currentSelectedPath > lastCharacterPathID)
        //            currentSelectedPath = 1;
        //        hoverCurrentPath();
        //    }
        //}
        //else
        //    AButtonDown = false;
        
        //if (isSelectedForPath && OVRInput.Get(OVRInput.RawButton.B))
        //{
        //    if (!BButtonDown)
        //    {
        //        BButtonDown = true;
        //        //deleteCurrentPath();
        //        //relocatePathButtons();
        //        currentSelectedPath = 0;
        //    }
        //}
        //else
        //    BButtonDown = false;

        //// restablish current selected Path if character is not selected
        //if (currentSelectedPath != 0 && !isSelectedForPath)
        //    currentSelectedPath = 0;
    }

    public IEnumerator defineNewPathPoint(Vector3 controllerPos, bool instantiatePoint = true)
    {
        pointsCount++;
        if (pointsCount == 1)
            startDiffPosition = controllerPos - startPosition;

        Vector3 newPoint = new Vector3(controllerPos.x, controllerPos.y - startDiffPosition.y, controllerPos.z);

        pathPositions.Add(newPoint);
        if (instantiatePoint)
        {
            if (pointsCount == 1)
                pathContainer = DefinePath.instance.addPointToNewPath(controllerPos, Quaternion.identity, pointsCount - 1, gameObject, false, startDiffPosition.y);
            else 
                DefinePath.instance.addPointToExistentPath(pathContainer, controllerPos, Quaternion.identity, pointsCount - 1, gameObject, false, startDiffPosition.y);

            yield return new WaitForSeconds(1.0f);
            
            // send new path point from assistant to director so that he can also play and visualize paths
            UDPSender.instance.sendPointPath(gameObject, newPoint);
        }

    }

    void playLinePath()
    {
        Transform pathTransform = pathContainer.transform;
        GameObject line = pathTransform.GetChild(0).gameObject;
        line.GetComponent<LineRenderer>().enabled = false;

        for (int i = 1; i < pathTransform.childCount; i++)
        {
            GameObject currPoint = pathTransform.GetChild(i).gameObject;
            currPoint.GetComponent<MeshRenderer>().enabled = false;
        }

        isPlaying = !isPlaying;
        //isPlaying = true;
    }

    void stopLinePath()
    {
        isPlaying = false;
        gameObject.transform.position = startPosition;
        gameObject.transform.rotation = startRotation;
        currPoint = 0;

        Transform pathTransform = pathContainer.transform;
        GameObject line = pathTransform.GetChild(0).gameObject;
        line.GetComponent<LineRenderer>().enabled = true;

        for (int i = 1; i < pathTransform.childCount; i++)
        {
            GameObject currPoint = pathTransform.GetChild(i).gameObject;
            currPoint.GetComponent<MeshRenderer>().enabled = true;
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

    public void deletePathPoint(int pointNum, bool deleteLine=true)
    {
        pathPositions.RemoveAt(pointNum);

        if (deleteLine)
            DefinePath.instance.deletePointFromPath(pathContainer, pointNum);
        pointsCount--;
    }

    public void relocatePoint(int pointNum, Vector3 direction)
    {
        // relocate sphere
        Transform sphere = pathContainer.transform.GetChild(pointNum + 1);
        sphere.position += direction;
        Vector3 newPoint = sphere.position;

        // position saved in the list has different height
        pathPositions[pointNum] = pathPositions[pointNum] + direction;

        GameObject line = pathContainer.transform.Find("Line").gameObject;
        LineRenderer currLineRenderer = line.GetComponent<LineRenderer>();
        int pointsCount = currLineRenderer.positionCount;

        // relocate point in line renderer
        Vector3[] pathPositionsArray = new Vector3[pathPositions.Count];
        currLineRenderer.GetPositions(pathPositionsArray);
        List<Vector3> pathPositionsList = pathPositionsArray.ToList<Vector3>();
        pathPositionsList[pointNum] = newPoint;

        // reassign
        pathPositionsArray = pathPositionsList.ToArray();
        currLineRenderer.SetPositions(pathPositionsArray);
    }

    public void rotateCharacter(Vector3 rotation)
    {
        gameObject.transform.Rotate(rotation);
    }

    public void changeSpeed(float speed)
    {
        posSpeed = speed;
        rotSpeed = speed * 3;
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
    //        for (int j = removePositions.Count - 1; j >= 0 ; j--)
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
    //        catch (Exception e) {}

    //    }
    //    lastCharacterPathID -= 1;
    //}
}
