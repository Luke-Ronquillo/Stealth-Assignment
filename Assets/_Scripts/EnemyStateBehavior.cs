using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyStateBehavior : MonoBehaviour
{
    enum State { idle, patrol, pursue, dead };

    NavMeshAgent agent;
    NavMeshPath currentPath;
    Queue<Vector3> remainingPoints;
    Vector3 currentCorner;
    Rigidbody rigidBody;
    Transform currentIdlePoint;
    float timeSinceLastDestination;
    int idlePointsIndex;
    bool isGoingForward;
    bool reachedTarget;
    float dot;

    [Header("Enemy Transform References")]
    [SerializeField] Transform playerPosition;
    [SerializeField] Transform[] idlePoints;

    [Header("Enemy Patrol Settings")]
    [SerializeField] float timeBeforeMovingToNextIdlePoint;
    [SerializeField] bool isLooping;

    [Header("Enemy Pursue Settings")]
    [SerializeField] float pursueDistance;
    [SerializeField] float loseDistance;

    [Header("Enemy Settings")]
    [SerializeField] State myState;
    [SerializeField] float movementSpeed;
    //[SerializeField] float rotationSpeed;
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
        CalculateNextPath(currentIdlePoint.position);
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
    }
    void CalculateNextPath(Vector3 _position)
    {
        if (agent.CalculatePath(_position, currentPath))
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
        //Vector3 direction = transform.forward;
        //Vector3 origin = transform.position;
        //RaycastHit hit;

        //if (Physics.Raycast(origin, direction, out hit, pursueDistance))
        //{
        //    if (hit.collider.gameObject.tag == "Player")
        //    {
        //        myState = State.pursue;
        //    }
        //}

        Vector3 _direction = transform.forward;
        Vector3 _origin = GetComponentInChildren<Transform>().position;
        Vector3 toPlayer = (playerPosition.position - transform.position).normalized;

        RaycastHit hit;

        float _distance = (playerPosition.position - transform.position).magnitude;
        float dotProduct = Vector3.Dot(_direction, toPlayer);
        dot = dotProduct;

        if (dotProduct > fieldOfView && _distance < pursueDistance)
        {
            if (Physics.Raycast(_origin, toPlayer, out hit, pursueDistance))
            { 
                if (hit.collider.gameObject.tag == "Player")
                    myState = State.pursue;
            }
        }
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
            CalculateNextPath(currentIdlePoint.position);
        }
        else
        {
            idlePointsIndex--;
            currentIdlePoint = idlePoints[idlePointsIndex];
            agent.enabled = true;
            CalculateNextPath(currentIdlePoint.position);
        }
    }
    public void Alerted(Vector3 position)
    {
        if (myState != State.pursue)
        {
            transform.LookAt(position);
            //StartCoroutine(RotateTowardsPlayer());
            myState = State.idle;
        }
    }
    //IEnumerator RotateTowardsPlayer()
    //{
    //    Quaternion targetRotation = Quaternion.LookRotation(playerPosition.position);
    //    while (transform.rotation != targetRotation)
    //    {
    //        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    //        yield return null;
    //    }
    //}
    IEnumerator IdleBeforeNextIdlePoint()
    {
        yield return new WaitForSeconds(timeBeforeMovingToNextIdlePoint);
        GetNextIdlePoint();
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
        StopAllCoroutines();
        if (timeSinceLastDestination > 0.1f)
        {
            agent.enabled = true;
            CalculateNextPath(playerPosition.position);
        }
        timeSinceLastDestination += Time.deltaTime;
        float distanceToNext = Vector3.Distance(transform.position, currentCorner);
        if (distanceToNext < 0.5)
        {
            if (remainingPoints.Count > 0)
            {
                currentCorner = remainingPoints.Dequeue();
                currentCorner.y = transform.position.y;
            }
        }
        float distanceToPlayer = Vector3.Distance(transform.position, playerPosition.position);
        if (distanceToPlayer > loseDistance)
        {
            myState = State.idle;
        }
        else
        {
            Vector3 newForward = (currentCorner - transform.position).normalized;
            newForward.y = 0;
            transform.forward = newForward;
        }
        rigidBody.linearVelocity = transform.forward * movementSpeed;
        
        float distance = Vector3.Distance(playerPosition.position, transform.position);
        if (distance < 1.5)
        {
            myState = State.dead;
        }
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
        float distanceToIdleNode = Vector3.Distance(transform.position, currentIdlePoint.position);
        if (distanceToIdleNode < 0.5)
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
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, playerPosition.position);

        Vector3 endPoint = transform.position + transform.forward * 10;

        if (dot > 0.8f)
            Gizmos.color = Color.green;
        else
            Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, endPoint);
    }
}
