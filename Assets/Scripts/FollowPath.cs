using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Processors;

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

    bool isPlaying;
    bool buttonDown;
    public bool triggerOn;
    public bool isSelectedForPath;
   
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

    void move(Vector3 targetPoint)
    {
        Vector3 currentPos = gameObject.transform.position;
        Vector3 targetDirection = targetPoint - currentPos;

        float posStep = posSpeed * Time.deltaTime;
        float rotStep = rotSpeed * Time.deltaTime;

        Vector3 newPos = Vector3.MoveTowards(currentPos, targetPoint, posStep);
        Debug.Log("NEXT POSITION: " + newPos);
        gameObject.transform.position = newPos;
        // if it is a camera there is no RotationScale script, and we do not want it to rotate with direction
        try
        {
            Vector3 originalRotation = gameObject.GetComponent<RotationScale>().rotation;

            // compute the new formard direction where we will rotate to
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
            }

            else if (!buttonDown && isSelectedForPath)
            {
                Vector3 controllerPos = handController.transform.position;
                Vector3 newPoint = new Vector3(controllerPos.x, startDiffPosition.y - controllerPos.y, controllerPos.z);
                pathPositions.Add(newPoint);

                // send new path point from assistant to director so that he can also play and visualize paths
                DrawLine.instance.SendPointPath(gameObject, newPoint);

                // ONLY FOR CONTINUOUS CASE
                DrawLine.instance.startLine = isSelectedForPath;
            }
        }
        else
            buttonDown = false;


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

            move(currTarget);
            Debug.Log("CURR TARGET: " + currTarget);
            Debug.Log("CURR POSITION: " + gameObject.transform.position);
            if (gameObject.transform.position == currTarget)
            {
                pointsCount++;
            }
        }
        else
        {
            if (animator != null)
                // do smooth transition from walk to idle taking the delta time
                animator.SetFloat("Speed", 0, 0.05f, Time.deltaTime);
        }
    }

    void playLinePath()
    {
        Debug.Log("PLAYING");
        GameObject[] lines;
        lines = GameObject.FindGameObjectsWithTag("Line");

        for (int i = 0; i < lines.Length; i++)
        {
            lines[i].GetComponent<LineRenderer>().enabled = false;
        }

        //isPlaying = !isPlaying;
        isPlaying = true;
    }

    void stopLinePath()
    {
        Debug.Log("STOPPING");
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
}
