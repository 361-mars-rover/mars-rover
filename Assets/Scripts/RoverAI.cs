using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class RoverAI : MonoBehaviour
{
    public Transform mainBase;
    public ExplorationTracker tracker;
    public float baseRadius = 100f; // Size of temporary base
    public float searchStep = 10f; // Distance between each mineral check
    public float mineralDetectionRadius = 50f;
    public int maxMinerals = 10;
    public float maxTime = 1800f; // 30 minutes

    private NavMeshAgent agent;
    private float startTime;
    private int collectedMinerals = 0;
    private Vector3 currentBase;
    private float currentSearchRadius = 10f;
    private float currentAngle = 0f; // Angle for circular movement

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        startTime = Time.time;
        currentBase = mainBase.position;
        StartCoroutine(ExploreRoutine()); 
    }

    IEnumerator ExploreRoutine()
    {
        while (true)
        {
            // If minerals are full, return to main base to deposit before continuing
            if (collectedMinerals >= maxMinerals)
            {
                yield return ReturnToMainBase();
            }

            // If we complete the full circle, expand it and restart
            if (currentAngle >= 360f)
            {
                currentSearchRadius += searchStep; // Move to next outer circle
                currentAngle = 0f; // Reset angle for new loop
            }

            // If we reach the base radius limit, move to a new base
            if (currentSearchRadius > baseRadius)
            {
                MoveToNewTemporaryBase();
                continue;
            }

            // Move to the next point along the current circle
            Vector3 targetLocation = GetNextCirclePoint();
            agent.SetDestination(targetLocation);

            // Wait until we reach the destination
            while (Vector3.Distance(transform.position, targetLocation) > 2f)
                yield return null;

            // Mark this area as explored
            tracker.MarkCircleAsExplored(targetLocation, currentSearchRadius);

            // Check for minerals with 5% probability
            if (Random.Range(0, 100) < 5)
            {
                tracker.SaveMineral(transform.position);
                collectedMinerals++;
                yield return SearchMineralRegion(transform.position, mineralDetectionRadius);
            }

            // Move to the next angle in the circle
            currentAngle += Mathf.Rad2Deg * (searchStep / currentSearchRadius); // Adjust angle step based on radius
        }
    }

    IEnumerator ReturnToMainBase()
    {
        agent.SetDestination(mainBase.position);
        while (Vector3.Distance(transform.position, mainBase.position) > 2f)
            yield return null;

        collectedMinerals = 0;
    }

    IEnumerator SearchMineralRegion(Vector3 mineralPos, float radius)
    {
        for (int i = 0; i < 3; i++)
        {
            Vector3 nearbySpot = mineralPos + new Vector3(
                Random.Range(-radius, radius),
                0,
                Random.Range(-radius, radius)
            );

            if (!tracker.IsCircleExplored(nearbySpot, radius))
            {
                agent.SetDestination(nearbySpot);
                yield return new WaitForSeconds(5);
                tracker.MarkCircleAsExplored(nearbySpot, radius);
            }
        }
    }

    Vector3 GetNextCirclePoint()
    {
        float x = currentBase.x + currentSearchRadius * Mathf.Cos(currentAngle * Mathf.Deg2Rad);
        float z = currentBase.z + currentSearchRadius * Mathf.Sin(currentAngle * Mathf.Deg2Rad);
        return new Vector3(x, transform.position.y, z);
    }

    void MoveToNewTemporaryBase()
    {
        Vector3 newBase = FindNewBasePosition();
        currentBase = newBase;
        currentSearchRadius = searchStep;
        currentAngle = 0f;
    }

    Vector3 FindNewBasePosition()
    {
        float angle = Random.Range(0, 360);
        float newX = currentBase.x + baseRadius * Mathf.Cos(angle * Mathf.Deg2Rad);
        float newZ = currentBase.z + baseRadius * Mathf.Sin(angle * Mathf.Deg2Rad);

        Vector3 newBase = new Vector3(newX, transform.position.y, newZ);
        while (tracker.IsBaseExplored(newBase))
        {
            angle = Random.Range(0, 360);
            newX = currentBase.x + baseRadius * Mathf.Cos(angle * Mathf.Deg2Rad);
            newZ = currentBase.z + baseRadius * Mathf.Sin(angle * Mathf.Deg2Rad);
            newBase = new Vector3(newX, transform.position.y, newZ);
        }

        tracker.MarkBaseAsExplored(newBase);
        return newBase;
    }
}