using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPath : MonoBehaviour
{
    public Vector3[] pathPositions;
    public float posSpeed = 5.0f;
    public float rotSpeed = 10.0f;
    int pointsCount;
    Vector3 startPosition;
    Quaternion startRotation;

    Animator animator;

    bool isPlaying;

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
        pointsCount = 0;
        isPlaying = false;

        startPosition = gameObject.transform.position;
        startRotation = gameObject.transform.rotation;

        animator = gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            isPlaying = !isPlaying;
        else if (Input.GetKeyDown(KeyCode.S)){
            isPlaying = false;
            gameObject.transform.position = startPosition;
            gameObject.transform.rotation = startRotation;
            pointsCount = 0;
        }

        if (isPlaying && pointsCount < pathPositions.Length)
        {
            Vector3 currTarget = pathPositions[pointsCount];
            animator.SetFloat("Speed", posSpeed);
            move(currTarget);
            if (gameObject.transform.position == currTarget)
            {
                pointsCount++;
            }
        }
    }
}
