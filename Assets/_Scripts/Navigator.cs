using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Unity.VisualScripting;

public class Navigator : MonoBehaviour
{
    NavMeshAgent agent;
    Rigidbody rb;

    [SerializeField] Transform target;
    [SerializeField] float moveSpeed = 3f;
    float timeSinceLastDestination = 0;
    
    NavMeshPath currentPath;

    Queue<Vector3> remainingPoints;
    Vector3 currentCorner;
    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();

        currentPath = new NavMeshPath();
        remainingPoints = new Queue<Vector3>();
    }
    private void Update()
    {
        if (timeSinceLastDestination > 0.1f)
        {
            //agent.SetDestination(target.position);
            timeSinceLastDestination = 0;

            agent.enabled = true;

            if (agent.CalculatePath(target.position, currentPath))
            {
                remainingPoints.Clear();
                Debug.Log("Calculated path successfully");
                
                foreach (Vector3 point in currentPath.corners)
                {
                    remainingPoints.Enqueue(point);
                }

                currentCorner = remainingPoints.Dequeue();
                //currentCorner = remainingPoints.Dequeue();
                currentCorner.y = transform.position.y;

                agent.enabled = false;
            }
        }

        timeSinceLastDestination += Time.deltaTime;

        Vector3 newForward = (currentCorner - transform.position).normalized;
        newForward.y = 0;
        transform.forward = newForward;

        float distanceToNext = Vector3.Distance(transform.position, currentCorner);
        if (distanceToNext < 0.5)
        {
            if (remainingPoints.Count > 0)
            {
                currentCorner = remainingPoints.Dequeue();
                currentCorner.y = transform.position.y;

            }
        }
    }
    private void FixedUpdate()
    {
        rb.linearVelocity = transform.forward * moveSpeed;
    }
    private void OnDrawGizmos()
    {
        if (currentPath == null)
            { return; }

        Gizmos.color = Color.yellow;

        foreach (Vector3 point in currentPath.corners)
        {
            Gizmos.DrawSphere(point, 1);
        }
    }
}
