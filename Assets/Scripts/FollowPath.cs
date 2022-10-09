using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Processors;

public class FollowPath : MonoBehaviour
{
    public GameObject handController;
    public List<Vector3> pathPositions;
    public float posSpeed = 5.0f;
    public float rotSpeed = 10.0f;
    int pointsCount;
    Vector3 startPosition;
    Quaternion startRotation;

    Animator animator;

    bool isPlaying;
    bool buttonDown;
    bool triggerOn;
    bool isSelected;

    private void OnTriggerEnter(Collider other)
    {
        triggerOn = true;
    }

    private void OnTriggerExit(Collider other)
    {
        triggerOn = false;
    }

    void move(Vector3 targetPoint)
    {
        Vector3 currentPos = gameObject.transform.position;
        Vector3 targetDirection = targetPoint - currentPos;

        float posStep = posSpeed * Time.deltaTime;
        float rotStep = rotSpeed * Time.deltaTime;
        gameObject.transform.position = Vector3.MoveTowards(currentPos, targetPoint, posStep);
        // compute the new formard direction where we will rotate to
        Vector3 newForward = Vector3.RotateTowards(transform.forward, targetDirection, rotStep, 0.0f);
        // compute the new rotation using this forward
        gameObject.transform.rotation = Quaternion.LookRotation(newForward);
    }

    // Start is called before the first frame update
    void Start()
    {
        pathPositions = new List<Vector3>();
        pointsCount = 0;

        isPlaying = false;
        buttonDown = false;
        triggerOn = false;
        isSelected = false;

        startPosition = gameObject.transform.position;
        startRotation = gameObject.transform.rotation;

        animator = gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger))
        {
            if (!buttonDown && triggerOn)
            {
                buttonDown = true;
                // first touch will select the character, and the second one will unselect it
                isSelected = !isSelected;
            }

            else if (!buttonDown && isSelected)
            {
                buttonDown = true;
                Vector3 controllerPos = handController.transform.position;
                Vector3 newPoint = new Vector3(controllerPos.x, gameObject.transform.position.y, controllerPos.z);
                pathPositions.Add(newPoint);
            }
        }
        else
            buttonDown = false;

        if (Input.GetKeyDown(KeyCode.P))
            isPlaying = !isPlaying;
        else if (Input.GetKeyDown(KeyCode.S)){
            isPlaying = false;
            gameObject.transform.position = startPosition;
            gameObject.transform.rotation = startRotation;
            pointsCount = 0;
        }
        else if (Input.GetKeyDown(KeyCode.M) && isSelected)
        {
            posSpeed += 0.1f;
            rotSpeed += 0.1f;
        }
        else if (Input.GetKeyUp(KeyCode.N) && isSelected)
        {
            posSpeed -= 0.1f;
            rotSpeed -= 0.1f;
        }

        if (isPlaying && pointsCount < pathPositions.Count)
        {
            Vector3 currTarget = pathPositions[pointsCount];
            animator.SetFloat("Speed", posSpeed, 0.05f, Time.deltaTime);
            move(currTarget);
            if (gameObject.transform.position == currTarget)
            {
                pointsCount++;
            }
        }
        else
        {
            // do smooth transition from walk to idle taking the delta time
            animator.SetFloat("Speed", 0, 0.05f, Time.deltaTime);
        }
    }
}
