using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class RoverAI : MonoBehaviour
{
    public Transform mainBase;
    public ExplorationTracker tracker;
    public float baseRadius = 100f;
    public float searchStep = 10f;
    public float mineralDetectionRadius = 50f;
    public int maxMinerals = 10;
    public float maxTime = 1800f; // 30 minutes

    private NavMeshAgent agent;
    private float startTime;
    private int collectedMinerals = 0;
    private Vector3 currentBase;
    private float currentSearchRadius = 10f;

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
            // If we reach max minerals, return to main base, drop off, then continue from the same base
            if (collectedMinerals >= maxMinerals)
            {
                yield return ReturnToMainBase();
            }

            // If the current base is fully explored, move to a new adjacent base
            if (currentSearchRadius > baseRadius)
            {
                MoveToNewTemporaryBase();
                continue;
            }

            // Move in concentric circles
            Vector3 targetLocation = FindCircularSearchPoint();
            agent.SetDestination(targetLocation);

            while (Vector3.Distance(transform.position, targetLocation) > 2f)
                yield return null;

            // Mark circle as explored
            tracker.MarkCircleAsExplored(targetLocation, currentSearchRadius);

            // Check for minerals
            if (Random.Range(0, 100) < 30) // 30% chance of finding minerals
            {
                Debug.Log("Minerals found! Prioritizing search in this area.");
                tracker.SaveMineral(transform.position);
                collectedMinerals++;

                // Search within Y radius of minerals
                yield return SearchMineralRegion(transform.position, mineralDetectionRadius);
            }
            else
            {
                currentSearchRadius += searchStep;
            }
        }
    }

    IEnumerator ReturnToMainBase()
    {
        Debug.Log("Returning to Main Base to deposit minerals...");
        agent.SetDestination(mainBase.position);
        while (Vector3.Distance(transform.position, mainBase.position) > 2f)
            yield return null;

        Debug.Log($"Deposited {collectedMinerals} minerals at the base.");
        collectedMinerals = 0;

        // Resume exploration from the same temporary base
        Debug.Log("Resuming exploration at previous temporary base...");
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

    Vector3 FindCircularSearchPoint()
    {
        float angle = Random.Range(0, 360);
        float x = currentBase.x + currentSearchRadius * Mathf.Cos(angle * Mathf.Deg2Rad);
        float z = currentBase.z + currentSearchRadius * Mathf.Sin(angle * Mathf.Deg2Rad);
        return new Vector3(x, transform.position.y, z);
    }

    void MoveToNewTemporaryBase()
    {
        Vector3 newBase = FindNewBasePosition();
        currentBase = newBase;
        currentSearchRadius = searchStep;
        Debug.Log($"Moving to new temporary base at {currentBase}");
    }

    Vector3 FindNewBasePosition()
    {
        float angle = Random.Range(0, 360);
        float newX = currentBase.x + baseRadius * Mathf.Cos(angle * Mathf.Deg2Rad);
        float newZ = currentBase.z + baseRadius * Mathf.Sin(angle * Mathf.Deg2Rad);
        
        Vector3 newBase = new Vector3(newX, transform.position.y, newZ);

        // Ensure we are not revisiting an explored base
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