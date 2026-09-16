using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBehavior : MonoBehaviour
{
    enum State { idle, patrol, pursue, dead };

    NavMeshAgent agent;
    NavMeshPath currentPath;
    Queue<Vector3> remainingPoints;
    Vector3 currentCorner;
    Rigidbody rigidBody;
    Transform currentIdlePoint;
    int idlePointsIndex;
    bool isGoingForward;
    bool reachedTarget;

    [Header("Enemy Transform References")]
    [SerializeField] Transform playerPosition;
    [SerializeField] Transform[] idlePoints;

    [Header("Enemy Patrol Settings")]
    [SerializeField] float timeBeforeMovingToNextIdlePoint;
    [SerializeField] bool isLooping;

    [Header("Enemy Settings")]
    [SerializeField] State myState;
    [SerializeField] float movementSpeed;
    [SerializeField] float pursueDistance;
    [SerializeField] float fieldOfView;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        rigidBody = GetComponent<Rigidbody>();

        currentPath = new NavMeshPath();
        remainingPoints = new Queue<Vector3>();

        idlePointsIndex = 0;
        reachedTarget = false;
        isGoingForward = true;
        agent.enabled = true;
        myState = State.patrol;

        currentIdlePoint = idlePoints[idlePointsIndex];
        CalculateNextPath();
    }
    void FixedUpdate()
    {
        switch(myState)
        {
            case State.idle:
                CheckForNearbyPlayer();
                UpdateIdle();
                break;
            case State.patrol:
                CheckForNearbyPlayer();
                UpdatePatrol(); 
                break;
            case State.pursue:
                UpdatePursue(); 
                break;
            case State.dead:
                UpdateDead(); 
                break;
        }
        Debug.Log("Index: " + idlePointsIndex);
    }
    void CalculateNextPath()
    {
        if (agent.CalculatePath(currentIdlePoint.position, currentPath))
        {
            remainingPoints.Clear();
            foreach (Vector3 point in currentPath.corners)
                remainingPoints.Enqueue(point);

            currentCorner = remainingPoints.Dequeue();
            currentCorner.y = transform.position.y;

            agent.enabled = false;
        }
        else
        {
            Debug.Log("Path not found");
        }
    }
    void CheckForNearbyPlayer()
    {
        Vector3 forwardDirection = transform.forward;

        Vector3 toTarget = (playerPosition.position - transform.position).normalized;
        float _distance = (playerPosition.position - transform.position).magnitude;

        float dotProduct = Vector3.Dot(forwardDirection, toTarget);
        if (dotProduct > fieldOfView && _distance < pursueDistance)
        {
            myState = State.pursue;
        }
        Debug.Log("Distance: " + _distance);
    }
    void GetNextIdlePoint()
    {
        if (!isLooping)
        {
            if (idlePointsIndex == idlePoints.Length - 1)
                isGoingForward = false;
            else if (idlePointsIndex == 0)
                isGoingForward = true;
        }

        if (isGoingForward)
        {
            idlePointsIndex++;
            if (idlePointsIndex > idlePoints.Length - 1)
                idlePointsIndex = 0;
            currentIdlePoint = idlePoints[idlePointsIndex];
            agent.enabled = true;
            CalculateNextPath();
        }
        else
        {
            idlePointsIndex--;
            currentIdlePoint = idlePoints[idlePointsIndex];
            agent.enabled = true;
            CalculateNextPath();
        }
    }
    IEnumerator IdleBeforeNextIdlePoint()
    {
        yield return new WaitForSeconds(timeBeforeMovingToNextIdlePoint);
        GetNextIdlePoint();
        Debug.Log("Am I stupid again");
        reachedTarget = false;
        myState = State.patrol;
        yield return null;
    }
    void UpdateIdle()
    {
        if (!reachedTarget)
            StartCoroutine(IdleBeforeNextIdlePoint());
        reachedTarget = true;
    }
    void UpdatePursue()
    {
        Debug.Log("I am pursuing");
        //transform.Translate(transform.forward * 10 * Time.deltaTime);
        //float distance = Vector3.Distance(target.position, transform.position);
        //if (distance < 2)
        //{
        //    myState = State.dead;
        //}
    }
    void UpdatePatrol()
    {
        float distanceToNext = Vector3.Distance(transform.position, currentCorner);
        if (distanceToNext < 0.5)
        {
            if (remainingPoints.Count > 0)
            {
                currentCorner = remainingPoints.Dequeue();
                currentCorner.y = transform.position.y;
            }
        }
        float distanceToTarget = Vector3.Distance(transform.position, currentIdlePoint.position);
        if (distanceToTarget < 0.5)
        {
            myState = State.idle;
        }
        else
        {
            Vector3 newForward = (currentCorner - transform.position).normalized;
            newForward.y = 0;
            transform.forward = newForward;
        }
        if (!reachedTarget)
            rigidBody.linearVelocity = transform.forward * movementSpeed;
    }
    void UpdateDead()
    {
        Debug.Log("Oh no, I'm dead");
    }
    
    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawLine(transform.position, target.position);

    //    Vector3 endPoint = transform.position + transform.forward * 10;

    //    if (dot > 0.8f)
    //        Gizmos.color = Color.green;
    //    else
    //        Gizmos.color = Color.red;
    //    Gizmos.DrawLine(transform.position, endPoint);
    //}
}
