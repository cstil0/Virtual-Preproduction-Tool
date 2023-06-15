using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FollowPath : MonoBehaviour
{
    public GameObject handController;
    public List<Vector3> pathPositions;
    // relate each path ID with the start and end positions in the pathPositions list
    public float posSpeed = 20.0f;
    public float rotSpeed = 7.0f;
    public int pointsCount;
    public int currPoint;
    public Vector3 startPosition;
    Vector3 startDiffPosition;
    public Quaternion startRotation;

    Animator animator;

    public GameObject pathContainer;
    public GameObject circlesContainer;

    public bool isPlaying = false;
    bool secondaryIndexTriggerDown = false;
    bool newPathInstantiated = false;
    public bool triggerOn = false;
    public bool isSelectedForPath = false;
    public bool isSelectedForPathOriginal = false;
    // last local path ID created in this character
    [SerializeField] int lastCharacterPathID = 0;
    [SerializeField] int currentSelectedPath = 0;

    [SerializeField] Vector3 characterRotationLeft = new Vector3(0, -5f, 0);
    [SerializeField] Vector3 characterRotationRight = new Vector3(0, 5f, 0);

    private void OnEnable()
    {
        DirectorPanelManager.instance.OnPlayPath += playLinePath;
        DirectorPanelManager.instance.OnStopPath += stopLinePath;
        UDPReceiver.instance.OnChangeItemColor += changeItemColorDirector;
        UDPReceiver.instance.OnChangePathColor += changePathColorDirector;
    }
    private void OnDisable()
    {
        DirectorPanelManager.instance.OnPlayPath -= playLinePath;
        DirectorPanelManager.instance.OnStopPath -= stopLinePath;
        UDPReceiver.instance.OnChangeItemColor -= changeItemColorDirector;
        UDPReceiver.instance.OnChangePathColor -= changePathColorDirector;
    }

    // extracted from Vector3.MoveTowards() function
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
        } catch (Exception e) {
            Debug.LogError(e.Message);
        }
    }

    void Start()
    {
        handController = GameObject.Find("RightHandAnchor");
        pathPositions = new List<Vector3>();
        pointsCount = 0;

        startPosition = gameObject.transform.position;
        startRotation = gameObject.transform.rotation;

        if (gameObject.GetComponent<Animator>())
            animator = gameObject.GetComponent<Animator>();
    }

    void Update()
    {
        if (triggerOn && !isPlaying) {
            // rotate character with thubstick
            if (OVRInput.Get(OVRInput.Button.SecondaryThumbstickLeft))
                rotateCharacter(characterRotationLeft);
            if (OVRInput.Get(OVRInput.Button.SecondaryThumbstickRight))
                rotateCharacter(characterRotationRight);

            // if character is being relocated, reset its initial position
            if (gameObject.transform.position != startPosition)
            {
                startPosition = gameObject.transform.position;
                startRotation = gameObject.transform.rotation;
                startDiffPosition = handController.transform.position - startPosition;

                // relocate first path point according to the character's new position
                if (pathPositions.Count > 0)
                {
                    pathPositions[0] = startPosition;
                    Transform firstPoint = pathContainer.transform.GetChild(0);
                    firstPoint.position = startPosition;
                }
            }
        }


        if (Input.GetKeyDown(KeyCode.P))
        {
            playLinePath();
        }
        else if (Input.GetKeyDown(KeyCode.S))
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

            // change walk / idle animations
            if (animator != null)
                animator.SetFloat("Speed", 0.1f, 0.05f, Time.deltaTime);

            // since VR application is set as host it plays the movement
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
    }

    private void LateUpdate()
    {
        if (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger) && !isPlaying)
        {
            bool isElementOnTrigger = HoverObjects.instance.checkIfElementOnTrigger();
            if (!secondaryIndexTriggerDown && triggerOn)
            {
                secondaryIndexTriggerDown = true;
                // first touch will select the character, and the second one will unselect it
                isSelectedForPath = !isSelectedForPath;
                isSelectedForPathOriginal = isSelectedForPath;

                if (pointsCount == 0)
                {
                    StartCoroutine(defineNewPathPoint(handController.transform.position));
                }

                changePathColor();
            }

            // define new path point if the item no element is being triggered
            else if (!secondaryIndexTriggerDown && isSelectedForPath && HoverObjects.instance.currentItemSelected == gameObject && !isElementOnTrigger)
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
    }

    public IEnumerator defineNewPathPoint(Vector3 controllerPos, bool instantiatePoint = true)
    {
        // if it is the first point, compute the initial difference needed to compute the height of the point
        pointsCount++;
        if (pointsCount == 1)
            startDiffPosition = controllerPos - startPosition;

        // new Y is computed to ensure that the character will be on the ground on the first point and the rest are taken from there
        float newY = controllerPos.y - startDiffPosition.y;
        Vector3 newPoint = new Vector3(controllerPos.x, newY, controllerPos.z);

        pathPositions.Add(newPoint);
        if (instantiatePoint)
        {
            if (pointsCount == 1)
            {
                // create the new path and save the corresponding containers
                List<GameObject> containers = DefinePath.instance.addPointToNewPath(controllerPos, Quaternion.identity, pointsCount - 1, gameObject, false, newY);
                pathContainer = containers[0];
                circlesContainer = containers[1];
            }
            else
            {
                // create the new point in the corresponding container
                LineRenderer lineRenderer = pathContainer.transform.GetComponentInChildren<LineRenderer>();
                addLineRendererPoint(lineRenderer, controllerPos, pointsCount - 1);
                DefinePath.instance.addPointToExistentPath(pathContainer, controllerPos, Quaternion.identity, pointsCount - 1, gameObject, false, newY);
            }

            yield return new WaitForSeconds(0.2f);
            
            // send new path point from assistant to director so that he can also play and visualize paths
            UDPSender.instance.sendPointPath(gameObject, newPoint);
        }
    }

    void playLinePath()
    {
        // hide path spheres and line renderers to show the scene in a clear way
        hideShowPath(false);

        // disable the buttons to show the scene in a clear way
        GameObject itemControlMenu = transform.Find("ItemControlMenu").gameObject;
        itemControlMenu.GetComponent<Canvas>().enabled = false;

        isPlaying = !isPlaying;
    }

    void stopLinePath()
    {
        isPlaying = false;
        // return character to its initial position and rotation
        gameObject.transform.position = startPosition;
        gameObject.transform.rotation = startRotation;
        currPoint = 0;

        // show path spheres and line renderers again
        hideShowPath(true);

        // show the buttons again in case the character was selected before playing
        GameObject itemControlMenu = transform.Find("ItemControlMenu").gameObject;
        if (isSelectedForPath)
            itemControlMenu.GetComponent<Canvas>().enabled = true;
        else
            itemControlMenu.GetComponent<Canvas>().enabled = false;
    }

    void hideShowPath(bool isHidden)
    {
        if (pathContainer != null)
        {
            Transform pathTransform = pathContainer.transform;
            Transform circlesTransform = circlesContainer.transform;

            // disable line renderer to hide or show it
            GameObject line = pathTransform.GetChild(0).gameObject;
            line.GetComponent<LineRenderer>().enabled = isHidden;

            // iterate through all spheres and circles and hide or show them
            for (int i = 1; i < pathTransform.childCount; i++)
            {
                GameObject currPoint = pathTransform.GetChild(i).gameObject;
                currPoint.GetComponent<MeshRenderer>().enabled = isHidden;
            }

            for (int i = 0; i < circlesTransform.childCount; i++)
            {
                GameObject currCircle = circlesTransform.GetChild(i).gameObject;
                currCircle.GetComponent<SpriteRenderer>().enabled = isHidden;
            }
        }
    }

    void addLineRendererPoint(LineRenderer lineRenderer, Vector3 newPosition, int pointsCount)
    {
        // add it both to the positions array and line renderer
        lineRenderer.positionCount += 1;
        lineRenderer.SetPosition(pointsCount, newPosition);
    }

    void removeLineRendererPoint(LineRenderer lineRenderer, int pointNum) 
    {
        // we cannot modify a linerenderer point, but we can copy them to a list, modify it and assign the list again
        int pointsCount = lineRenderer.positionCount;
        Vector3[] pathPositionsArray = new Vector3[pointsCount];
        lineRenderer.GetPositions(pathPositionsArray);
        List<Vector3> pathPositionsList = pathPositionsArray.ToList<Vector3>();
        pathPositionsList.RemoveAt(pointNum);

        pathPositionsArray = pathPositionsList.ToArray();
        lineRenderer.SetPositions(pathPositionsArray);
        lineRenderer.positionCount = pointsCount - 1;
    }


    public void deletePathPoint(int pointNum, bool deleteLine=true)
    {
        pathPositions.RemoveAt(pointNum);
        LineRenderer lineRenderer = pathContainer.transform.GetComponentInChildren<LineRenderer>();
        removeLineRendererPoint(lineRenderer, pointNum);

        if (deleteLine)
            DefinePath.instance.deletePointFromPath(pathContainer, pointNum, circlesContainer);
        pointsCount--;
    }

    public void relocatePoint(int pointNum, Vector3 direction, bool relocateSphere)
    {
        // relocate sphere
        if (relocateSphere)
        {
            Transform sphere = pathContainer.transform.GetChild(pointNum + 1);
            sphere.position += direction;
            UDPSender.instance.sendPointRelocation(gameObject.name, pointNum, direction, new Vector3());
        }

        // get the position of the point by correcting its height, as it is saved considering the first point to be over the ground
        pathPositions[pointNum] = pathPositions[pointNum] + direction;

        GameObject line = null;
        foreach (Transform child in pathContainer.transform)
        {
            if (child.name.Contains("Line"))
            {
                line = child.gameObject;
                break;
            }
        }
        LineRenderer currLineRenderer = line.GetComponent<LineRenderer>();

        // relocate point in line renderer
        Vector3[] pathPositionsArray = new Vector3[pathPositions.Count];
        currLineRenderer.GetPositions(pathPositionsArray);
        List<Vector3> pathPositionsList = pathPositionsArray.ToList<Vector3>();
        pathPositionsList[pointNum] += direction;

        // reassign positions to the line renderer
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

    public void changePathColor()
    {
        // get the corresponding color
        Color color = DefinePath.instance.defaultLineColor;
        if (isSelectedForPath)
            color = DefinePath.instance.selectedLineColor;

        DefinePath.instance.changePathColor(pathContainer, color, isSelectedForPath);

        // inform about the change of color for this path
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

    // this coroutine is necessary in the client part since we need to wait until the path container is received and correctly stored
    IEnumerator changeColorWaitPathContainer(Color color)
    {
        while (pathContainer == null) yield return null;

        DefinePath.instance.changePathColor(pathContainer, color, false);
    }

    public int extractItemNum()
    {
        string[] splittedName = gameObject.name.Split(' ');
        return int.Parse(splittedName[1]);
    }
}
