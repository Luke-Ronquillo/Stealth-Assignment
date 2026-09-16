using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Collections;
public class EnemyPatrolling : MonoBehaviour
{
    NavMeshAgent agent;
    NavMeshPath currentPath;
    Queue<Vector3> remainingPoints;
    Vector3 currentCorner;
    Rigidbody rigidBody;
    Transform target;
    int targetsIndex;
    bool isGoingForward;
    bool reachedTarget;

    [Header("Enemy Settings")]
    [SerializeField] Transform[] targets;
    [SerializeField] bool isLooping;
    [SerializeField] float timeBeforeMovingToNextTarget;
    [SerializeField] float movementSpeed;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        rigidBody = GetComponent<Rigidbody>();

        currentPath = new NavMeshPath();
        remainingPoints = new Queue<Vector3>();

        targetsIndex = 0;
        reachedTarget = false;
        GetNextTarget();
    }
    private void Update()
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
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        if (distanceToTarget < 0.5 && reachedTarget == false)
        {
            reachedTarget = true;
            StartCoroutine(IdleBeforeNextWanderPoint());
        }
        else
        {
            Vector3 newForward = (currentCorner - transform.position).normalized;
            newForward.y = 0;
            transform.forward = newForward;
        }
    }
    private void FixedUpdate()
    {
        if (reachedTarget == false)
           rigidBody.linearVelocity = transform.forward * movementSpeed;
        Debug.Log("isGoingForward: " + isGoingForward);
        Debug.Log("hasReachedTarget: " + reachedTarget);
    }
    IEnumerator IdleBeforeNextWanderPoint()
    {
        yield return new WaitForSeconds(timeBeforeMovingToNextTarget);
        Debug.Log("IM TSUPID");
        GetNextTarget();
        reachedTarget = false;
        yield return null;
    }
    void GetNextTarget()
    {
        if (!isLooping)
        { 
            if (targetsIndex == targets.Length - 1)
                isGoingForward = false;
            else if (targetsIndex == 0)
                isGoingForward = true;
        }
        else
        {
            isGoingForward = true;
            if (targetsIndex > targets.Length - 1)
                targetsIndex = 0;
        }

        if (isGoingForward)
        {
            target = targets[targetsIndex];
            targetsIndex++;
            agent.enabled = true;

            if (agent.CalculatePath(target.position, currentPath))
            {
                remainingPoints.Clear();
                foreach (Vector3 point in currentPath.corners)
                {
                    remainingPoints.Enqueue(point);
                }

                currentCorner = remainingPoints.Dequeue();
                currentCorner.y = transform.position.y;

                agent.enabled = false;
            }
        }
        else
        {
            target = targets[targetsIndex];
            targetsIndex--;
            agent.enabled = true;

            if (agent.CalculatePath(target.position, currentPath))
            {
                remainingPoints.Clear();
                foreach (Vector3 point in currentPath.corners)
                {
                    remainingPoints.Enqueue(point);
                }

                currentCorner = remainingPoints.Dequeue();
                currentCorner.y = transform.position.y;

                agent.enabled = false;
            }
        }
    }
}
