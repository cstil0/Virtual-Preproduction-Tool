using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class FollowPathCamera : MonoBehaviour
{
    public GameObject handController;
    public List<Vector3> pathPositions;
    public List<Quaternion> pathRotations;
    // relate each path ID with the start and end positions in the pathPositions list
    public Dictionary<int, int[]> pathStartEnd;
    public float posSpeed = 20.0f;
    public float rotSpeed = 7.0f;
    int pointsPosCount;
    int pointsRotCount;
    Vector3 startPosition;
    Vector3 startDiffPosition;
    Quaternion startRotation;

    Animator animator;

    bool isPlaying = false;
    bool triggerButtonDown = false;
    bool AButtonDown = false;
    bool BButtonDown = false;
    bool newPathInstantiated = false;
    public bool triggerOn = false;
    public bool isSelectedForPath = false;
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

    void move(Vector3 targetPoint, Quaternion targetRot)
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
        }
        catch (Exception e)
        {
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
        pathStartEnd = new Dictionary<int, int[]>();
        pointsPosCount = 0;
        pointsRotCount = 0;

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
        //        // NO FAR� FALTA EN EL CAS CONTINU
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
        if (OVRInput.Get(OVRInput.Button.SecondaryHandTrigger))
        {
            if (!triggerButtonDown && triggerOn)
            {
                triggerButtonDown = true;
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

            else if (!triggerButtonDown && isSelectedForPath)
            {
                // this is needed because we do not want to do this just after the character is selected but when line is instantiated
                if (!newPathInstantiated)
                {
                    lastCharacterPathID += 1;
                    StartCoroutine(createNewPathButton());
                    int[] startEnd = { pathPositions.Count, -1 };
                    pathStartEnd.Add(lastCharacterPathID, startEnd);
                }

                newPathInstantiated = true;
                Vector3 controllerPos = handController.transform.position;
                Vector3 newPoint = new Vector3(controllerPos.x, controllerPos.y - startDiffPosition.y, controllerPos.z);
                Quaternion newRot = handController.transform.rotation;
                pathPositions.Add(newPoint);
                pathRotations.Add(newRot);

                // send new path point from assistant to director so that he can also play and visualize paths
                DrawLine.instance.SendPointPath(gameObject, newPoint);

                // ONLY FOR CONTINUOUS CASE
                DrawLine.instance.startLine = isSelectedForPath;
            }
        }
        else if (isSelectedForPath && newPathInstantiated)
        {
            // establish end position of last path once the button is released
            int[] startEnd = pathStartEnd[lastCharacterPathID];
            startEnd[1] = pathPositions.Count - 1;
            pathStartEnd[lastCharacterPathID] = startEnd;
            newPathInstantiated = false;
        }
        else
        {
            newPathInstantiated = false;
            triggerButtonDown = false;
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

        if (isPlaying && (pointsPosCount < pathPositions.Count || pointsRotCount < pathRotations.Count))
        {
            Vector3 currTargetPos = pathPositions[pointsPosCount];
            Quaternion currTargetRot = pathRotations[pointsRotCount];

            if (animator != null)
                animator.SetFloat("Speed", posSpeed, 0.05f, Time.deltaTime);

            if (ModesManager.instance.role == ModesManager.eRoleType.ASSISTANT)
                move(currTargetPos, currTargetRot);

            if (gameObject.transform.position == currTargetPos)
                pointsPosCount++;
            if (gameObject.transform.rotation == currTargetRot)
                pointsRotCount++;
        }
        else
        {
            if (animator != null)
                // do smooth transition from walk to idle taking the delta time
                animator.SetFloat("Speed", 0, 0.05f, Time.deltaTime);
            isPlaying = false;
        }


        // navigate through paths and delete them
        if (isSelectedForPath && OVRInput.Get(OVRInput.RawButton.A))
        {
            if (!AButtonDown)
            {
                AButtonDown = true;
                currentSelectedPath += 1;
                if (currentSelectedPath > lastCharacterPathID)
                    currentSelectedPath = 1;
                hoverCurrentPath();
            }
        }
        else
            AButtonDown = false;

        if (isSelectedForPath && OVRInput.Get(OVRInput.RawButton.B))
        {
            if (!BButtonDown)
            {
                BButtonDown = true;
                deleteCurrentPath();
                relocatePathButtons();
                currentSelectedPath = 0;
            }
        }
        else
            BButtonDown = false;

        // restablish current selected Path if character is not selected
        if (currentSelectedPath != 0 && !isSelectedForPath)
            currentSelectedPath = 0;
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
        pointsPosCount = 0;
        pointsRotCount = 0;

        GameObject[] lines;
        lines = GameObject.FindGameObjectsWithTag("Line");

        for (int i = 0; i < lines.Length; i++)
        {
            lines[i].GetComponent<LineRenderer>().enabled = true;
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

    // make the last button visible, change its name and change color for all paths for the current character
    IEnumerator createNewPathButton()
    {
        while (!DrawLine.instance.lineAlreadyInstantiated)
        {
            yield return null;
        }

        DrawLine.instance.lineAlreadyInstantiated = false;

        int lastGeneralPathID = DrawLine.instance.lastPathID;
        // by now, just handle exceptions if more than five paths are defined, but in this case they should not even be created
        try
        {
            // activate image and text to show the button
            Transform pathButtons = gameObject.transform.Find("Paths buttons");
            Transform panel = pathButtons.GetChild(0);
            GameObject pathButton = panel.GetChild(lastCharacterPathID - 1).gameObject;
            pathButton.GetComponent<Image>().enabled = true;
            GameObject text = pathButton.transform.GetChild(0).gameObject;
            text.GetComponent<TextMeshProUGUI>().enabled = true;
            text.GetComponent<TextMeshProUGUI>().text = "Path " + lastGeneralPathID;

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
                continue;

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
                continue;

            // get path ID
            GameObject text = pathButton.transform.GetChild(0).gameObject;
            string pathName = text.GetComponent<TextMeshProUGUI>().text;
            int pathID = int.Parse(pathName.Split(" ")[1]);
            DrawLine.instance.changePathColor(pathID, DrawLine.instance.defaultLineColor);
        }
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
                pathColor = DrawLine.instance.hoverLineColor;
            else
                pathColor = DrawLine.instance.getPathColor(pathID);

            ColorBlock buttonColors = pathButton.GetComponent<Button>().colors;
            buttonColors.normalColor = pathColor;
            pathButton.GetComponent<Button>().colors = buttonColors;
            DrawLine.instance.changePathColor(pathID, pathColor);
        }
    }

    void deleteCurrentPath()
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
            if (i != currentSelectedPath - 1)
                continue;

            text.GetComponent<TextMeshProUGUI>().enabled = false;
            pathButton.GetComponent<Image>().enabled = false;
            GameObject[] lines = GameObject.FindGameObjectsWithTag("Line");

            foreach (GameObject line in lines)
            {
                if (!line.name.Contains("Path " + pathID))
                    continue;

                Destroy(line.gameObject);
                break;
            }

            int globalSelectedID = getGlobalPathID(currentSelectedPath);
            int[] startEnd = pathStartEnd[globalSelectedID];
            int startPos = startEnd[0];
            int endPos = startEnd[1];
            // delete path points in the character
            List<int> removePositions = new List<int>();
            for (int j = 0; j < pathPositions.Count; j++)
            {
                if (j >= startPos && j <= endPos)
                    removePositions.Add(j);
            }

            // this is needed since it is not possible to remove items while iterating the same array
            // ESTARIA B� TORNAR-HO A PROVAR AMB LO DE BAIX
            for (int j = removePositions.Count - 1; j >= 0; j--)
                pathPositions.RemoveAt(j);

            // update start and end positions of each following path
            pathStartEnd.Remove(currentSelectedPath);
            int removedPathSize = endPos - startPos + 1;
            int removeStartEnd = -1;
            foreach (KeyValuePair<int, int[]> startEndPos in pathStartEnd)
            {
                if (startEndPos.Key <= currentSelectedPath)
                    continue;

                removeStartEnd = startEndPos.Key;
            }

            if (removedPathSize != -1)
                pathStartEnd.Remove(removeStartEnd);

            break;
        }
    }

    void relocatePathButtons()
    {
        Transform pathButtons = gameObject.transform.Find("Paths buttons");
        Transform panel = pathButtons.GetChild(0);

        for (int i = 0; i < panel.childCount; i++)
        {
            if (i <= currentSelectedPath - 1)
                continue;

            GameObject currentPathButton = panel.GetChild(i).gameObject;
            if (!currentPathButton.GetComponent<Image>().enabled)
                continue;

            try
            {
                // first, get current ID and colors
                GameObject currentText = currentPathButton.transform.GetChild(0).gameObject;
                string currentPathName = currentText.GetComponent<TextMeshProUGUI>().text;
                int currentPathID = int.Parse(currentPathName.Split(" ")[1]);
                ColorBlock currentButtonColors = currentPathButton.GetComponent<Button>().colors;
                if (i == currentSelectedPath - 1)
                    currentButtonColors.normalColor = DrawLine.instance.hoverLineColor;

                // then, assign them to the previous one
                GameObject previousPathButton = panel.GetChild(i - 1).gameObject;
                GameObject previousText = previousPathButton.transform.GetChild(0).gameObject;
                string previousPathName = "Path " + currentPathID;
                previousText.GetComponent<TextMeshProUGUI>().text = previousPathName;
                previousPathButton.GetComponent<Button>().colors = currentButtonColors;

                previousText.GetComponent<TextMeshProUGUI>().enabled = true;
                previousPathButton.GetComponent<Image>().enabled = true;

                if (i == lastCharacterPathID - 1)
                {
                    currentText.GetComponent<TextMeshProUGUI>().enabled = false;
                    currentPathButton.GetComponent<Image>().enabled = false;
                }

            }
            catch (Exception e) { }

        }
        lastCharacterPathID -= 1;
    }
}