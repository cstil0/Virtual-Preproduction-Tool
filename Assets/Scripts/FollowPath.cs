using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class FollowPath : MonoBehaviour
{
    public GameObject handController;
    public List<Vector3> pathPositions;
    public float posSpeed = 20.0f;
    public float rotSpeed = 7.0f;
    int pointsCount;
    Vector3 startPosition;
    Vector3 startDiffPosition;
    Quaternion startRotation;

    Animator animator;

    bool isPlaying = false;
    bool buttonDown;
    bool newPathInstantiated;
    public bool triggerOn;
    public bool isSelectedForPath;
    int lastCharacterPathID = 0;

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
        DirectorPanelManager.instance.OnPlayPath += playLinePath;
        DirectorPanelManager.instance.OnStopPath += stopLinePath;

        handController = GameObject.Find("RightHandAnchor");
        pathPositions = new List<Vector3>();
        pointsCount = 0;

        isPlaying = false;
        buttonDown = false;
        triggerOn = false;
        newPathInstantiated = false;
        isSelectedForPath = false;

        startPosition = gameObject.transform.position;
        startRotation = gameObject.transform.rotation;

        if (gameObject.GetComponent<Animator>())
            animator = gameObject.GetComponent<Animator>();
    }

    private void OnDisable()
    {
        DirectorPanelManager.instance.OnPlayPath -= playLinePath;
        DirectorPanelManager.instance.OnStopPath -= stopLinePath;
    }

    // Update is called once per frame
    void Update()
    {
        //if (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger))
        //{
        //    if (!buttonDown && triggerOn)
        //    {
        //        buttonDown = true;
        //        // first touch will select the character, and the second one will unselect it
        //        isSelected = !isSelected;
        //        // NO FARÀ FALTA EN EL CAS CONTINU
        //        DrawLine.instance.continueLine = isSelected;
        //    }

        //    else if (!buttonDown && isSelected)
        //    {
        //        buttonDown = true;
        //        Vector3 controllerPos = handController.transform.position;
        //        Vector3 newPoint = new Vector3(controllerPos.x, gameObject.transform.position.y, controllerPos.z);
        //        pathPositions.Add(newPoint);
        //    }
        //}
        //else
        //    buttonDown = false;

        // CONTINUOUS CASE
        if (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger))
        {
            if (!buttonDown && triggerOn)
            {
                buttonDown = true;
                // first touch will select the character, and the second one will unselect it
                isSelectedForPath = !isSelectedForPath;
                DrawLine.instance.startLine = false;
                startPosition = gameObject.transform.position;
                startDiffPosition = handController.transform.position - startPosition;

                if (isSelectedForPath)
                    showPathButtons();
                else  
                    hidePathButtons();
            }

            else if (!buttonDown && isSelectedForPath)
            {
                // this is needed because we do not want to do this just after the character is selected but when line is instantiated
                if (!newPathInstantiated)
                    StartCoroutine(createNewPathButton());

                newPathInstantiated = true;
                Vector3 controllerPos = handController.transform.position;
                Vector3 newPoint = new Vector3(controllerPos.x, controllerPos.y - startDiffPosition.y, controllerPos.z);
                pathPositions.Add(newPoint);

                // send new path point from assistant to director so that he can also play and visualize paths
                DrawLine.instance.SendPointPath(gameObject, newPoint);

                // ONLY FOR CONTINUOUS CASE
                DrawLine.instance.startLine = isSelectedForPath;
            }
        }
        else
        {
            newPathInstantiated = false;
            buttonDown = false;
        }


        if (Input.GetKeyDown(KeyCode.P) || OVRInput.Get(OVRInput.RawButton.X))
        {
            playLinePath();
        }
        else if (Input.GetKeyDown(KeyCode.S)) //|| OVRInput.Get(OVRInput.RawButton.Y))
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

        if (isPlaying && pointsCount < pathPositions.Count)
        {
            Vector3 currTarget = pathPositions[pointsCount];

            if (animator != null)
                animator.SetFloat("Speed", posSpeed, 0.05f, Time.deltaTime);

            if (ModesManager.instance.role == ModesManager.eRoleType.ASSISTANT)
                move(currTarget);

            if (gameObject.transform.position == currTarget)
                pointsCount++;
        }
        else
        {
            if (animator != null)
                // do smooth transition from walk to idle taking the delta time
                animator.SetFloat("Speed", 0, 0.05f, Time.deltaTime);
            isPlaying = false;
        }
    }

    void playLinePath()
    {
        GameObject[] lines;
        lines = GameObject.FindGameObjectsWithTag("Line");

        for (int i = 0; i < lines.Length; i++)
        {
            lines[i].GetComponent<LineRenderer>().enabled = false;
        }

        isPlaying = !isPlaying;
        //isPlaying = true;
    }

    void stopLinePath()
    {
        isPlaying = false;
        gameObject.transform.position = startPosition;
        gameObject.transform.rotation = startRotation;
        pointsCount = 0;

        GameObject[] lines;
        lines = GameObject.FindGameObjectsWithTag("Line");

        for (int i = 0; i < lines.Length; i++)
        {
            lines[i].GetComponent<LineRenderer>().enabled = true;
        }
    }

    // make the last button visible, change its name and change color for all paths for the current character
    IEnumerator createNewPathButton()
    {
        while (!DrawLine.instance.lineAlreadyInstantiated)
        {
            yield return null;
        }

        DrawLine.instance.lineAlreadyInstantiated = false;

        int lastGeneralPathID = DrawLine.instance.lastPathID;
        lastCharacterPathID += 1;
        // by now, just handle exceptions if more than five paths are defined, but in this case they should not even be created
        try
        {
            // activate image and text to show the button
            Transform pathButtons = gameObject.transform.Find("Paths buttons");
            Transform panel = pathButtons.GetChild(0);
            GameObject pathButton = panel.Find("Path " + lastCharacterPathID).gameObject;
            pathButton.GetComponent<Image>().enabled = true;
            GameObject text = pathButton.transform.GetChild(0).gameObject;
            text.GetComponent<TextMeshProUGUI>().enabled = true;
            text.GetComponent<TextMeshProUGUI>().text += " " + lastGeneralPathID;

            // change button color to match path color
            Color pathColor = DrawLine.instance.getPathColor(DrawLine.instance.lastPathID);
            ColorBlock buttonColors = pathButton.GetComponent<Button>().colors;
            buttonColors.normalColor = pathColor;
            pathButton.GetComponent<Button>().colors = buttonColors;
            DrawLine.instance.changePathColor(DrawLine.instance.lastPathID, pathColor);
        }
        catch (Exception e)
        {
            Debug.LogError("More than 5 paths were created for this character");
        }
    }
    
    void showPathButtons()
    {
        Transform pathButtons = gameObject.transform.Find("Paths buttons");
        pathButtons.GetComponent<Canvas>().enabled = true;
        Transform panel = pathButtons.GetChild(0);

        // change color for all paths of this character
        for (int i = 0; i < panel.childCount; i++)
        {
            GameObject currentPathButton = panel.GetChild(i).gameObject;
            if (!currentPathButton.GetComponent<Image>().enabled)
                break;

            // get path ID
            GameObject currentText = currentPathButton.transform.GetChild(0).gameObject;
            string currentPathName = currentText.GetComponent<TextMeshProUGUI>().text;
            int currentPathID = int.Parse(currentPathName.Split(" ")[1]);
            Color currentPathColor = DrawLine.instance.getPathColor(currentPathID);
            DrawLine.instance.changePathColor(currentPathID, currentPathColor);
        }
    } 

    void hidePathButtons()
    {
        // iterate through all path buttons
        Transform pathButtons = gameObject.transform.Find("Paths buttons");
        pathButtons.GetComponent<Canvas>().enabled = false;
        Transform panel = pathButtons.GetChild(0);
        for (int i = 0; i < panel.childCount; i++)
        {
            GameObject pathButton = panel.GetChild(i).gameObject;
            if (!pathButton.GetComponent<Image>().enabled)
                break;

            // get path ID
            GameObject text = pathButton.transform.GetChild(0).gameObject;
            string pathName = text.GetComponent<TextMeshProUGUI>().text;
            int pathID = int.Parse(pathName.Split(" ")[1]);
            DrawLine.instance.changePathColor(pathID, DrawLine.instance.defaultLineColor);
        }
    }

}
