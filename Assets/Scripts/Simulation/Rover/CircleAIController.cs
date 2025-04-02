using UnityEngine;
using System.Collections;

public class CircleAIController : MonoBehaviour
{
    // These will be set by CarControl when calling the CircleAIUpdate function
    private Transform carTransform;
    private Vector3 homeBasePosition;
    private float currentRadius;
    private float currentAngle;
    private float circleSpeed;
    private float maxRadius;
    private float radiusIncrement;
    private float avoidanceThreshold;
    private bool avoidingRock;
    private Vector3 avoidanceTarget;
    private float lastRockDistance = Mathf.Infinity;
    private bool inReverseManeuver = false;
    private bool lookingForGem = false;
    private bool innerCircleMode = false;
    private Vector3 normalHomeBase;
    private float normalRadius;
    private int innerCircleLapsCompleted = 0;
    private int totalInnerCircleLaps = 10;
    private float innerCircleFactor = 0.1f;


    // Method to initialize the controller with necessary parameters
    public void Initialize(Transform transform, Vector3 homeBase, float radius, float angle, 
        float speed, float maxRad, float radIncrement, float avoidThreshold)
    {
        carTransform = transform;
        homeBasePosition = homeBase;
        currentRadius = radius;
        currentAngle = angle;
        circleSpeed = speed;
        maxRadius = maxRad;
        radiusIncrement = radIncrement;
        avoidanceThreshold = avoidThreshold;
    }

    public void SetCircleParameters(float radius, float angle, bool isInnerCircleMode, int lapsCompleted)
    {
        currentRadius = radius;
        currentAngle = angle;
        innerCircleMode = isInnerCircleMode;
        innerCircleLapsCompleted = lapsCompleted;
    }

    public void SetHomeBasePositions(Vector3 homeBase, Vector3 normalBase)
    {
        homeBasePosition = homeBase;
        normalHomeBase = normalBase;
    }

    public void SetInnerCircleParameters(float normalRad, float innerFactor, int totalLaps)
    {
        normalRadius = normalRad;
        innerCircleFactor = innerFactor;
        totalInnerCircleLaps = totalLaps;
    }

    public void SetAvoidanceState(bool isAvoiding, Vector3 avoidTarget)
    {
        avoidingRock = isAvoiding;
        avoidanceTarget = avoidTarget;
    }

    public (Vector3 targetPosition, float updatedAngle, bool isAvoidingRock, Vector3 avoidanceTarget, 
        bool isInnerCircle, float updatedRadius, int updatedLaps) 
    CircleAIUpdate()
    {
        Vector3 targetPosition;
        float gemRayDistance = 50f; // Increased detection range
        float gemSphereRadius = 2f; // Adjust to widen the detection area
        int gemLayerMask = 1 << LayerMask.NameToLayer("SphereGem");

        RaycastHit gemHit;
        if (Physics.SphereCast(carTransform.position, gemSphereRadius, carTransform.forward, out gemHit, gemRayDistance, gemLayerMask))
        {
            lookingForGem = true;
            targetPosition = gemHit.collider.transform.position;
            Debug.Log("Gem detected via sphere cast. Moving toward gem.");
        } 
        else if (!avoidingRock)
        {
            // Normal circle target calculation.
            Vector3 rockAvoidanceOffset = CheckForRockAvoidance();
            float angleRad = currentAngle * Mathf.Deg2Rad;
            targetPosition = homeBasePosition + new Vector3(
                Mathf.Sin(angleRad) * currentRadius,
                0f,
                Mathf.Cos(angleRad) * currentRadius
            ); 
            if (rockAvoidanceOffset != Vector3.zero)
            {
                avoidingRock = true;
                avoidanceTarget = carTransform.position + rockAvoidanceOffset;
                Debug.Log("Rock detected. Switching to avoidance mode.");
            }
        }
        else
        {
            targetPosition = avoidanceTarget;
            if (Vector3.Distance(carTransform.position, avoidanceTarget) < avoidanceThreshold)
            {
                avoidingRock = false;
                Debug.Log("Avoidance complete. Resuming circle path.");
            }
        }

        currentAngle += circleSpeed * Time.deltaTime;
        if (currentAngle >= 360f)
        {
            currentAngle = 0f;
            if (innerCircleMode)
            {
                innerCircleLapsCompleted++;
                Debug.Log("Inner circle lap " + innerCircleLapsCompleted + " completed.");
                // Gradually expand the inner circle radius each lap.
                if (innerCircleLapsCompleted < totalInnerCircleLaps)
                {
                    // Increase by a fraction of maxRadius per lap.
                    float innerIncrement = maxRadius * (innerCircleFactor / totalInnerCircleLaps);
                    currentRadius += innerIncrement;
                    Debug.Log("Expanding inner circle radius to: " + currentRadius);
                }
                else
                {
                    // After completing required laps, exit inner circle mode.
                    innerCircleMode = false;
                    innerCircleLapsCompleted = 0;
                    homeBasePosition = normalHomeBase;
                    currentRadius = normalRadius;
                    Debug.Log("Inner circle scan complete. Resuming normal circle scan.");
                }
            }
            else
            {
                // Normal mode: Increase radius until max is reached, then shift home base.
                if (currentRadius < maxRadius)
                {
                    currentRadius += maxRadius * radiusIncrement;
                    Debug.Log("Increasing circle radius to: " + currentRadius);
                }
                else
                {
                    Vector3 newBase = homeBasePosition + Vector3.right * (2 * currentRadius);
                    homeBasePosition = newBase;
                    Debug.Log("New home base set at: " + homeBasePosition);
                    currentRadius = maxRadius * radiusIncrement;
                }
            }
        }

        // Return all the updated values and the target position
        return (targetPosition, currentAngle, avoidingRock, avoidanceTarget, 
            innerCircleMode, currentRadius, innerCircleLapsCompleted);
    }

    private Vector3 CheckForRockAvoidance()
    {
        float detectionDistance = 30f;
        float sphereRadius = 2f;
        Vector3 origin = carTransform.position;
        Vector3 forwardDir = carTransform.forward;
        int rockLayerMask = 1 << LayerMask.NameToLayer("Rock");

        RaycastHit hit;
        if (Physics.SphereCast(origin, sphereRadius, forwardDir, out hit, detectionDistance, rockLayerMask))
        {
            lastRockDistance = hit.distance;
            Debug.Log($"Rock detected via SphereCast: {hit.collider.name}, distance: {hit.distance}");
            Vector3 rockPosition = hit.collider.transform.position;
            Vector3 toRock = rockPosition - carTransform.position;
            toRock.y = 0f;
            float angleToRock = Vector3.Angle(carTransform.forward, toRock);
            if (angleToRock > 25f)
            {
                return Vector3.zero;
            }
            float dot = Vector3.Dot(carTransform.right, toRock);
            Vector3 avoidanceDirection = (dot < 0) ? carTransform.right : -carTransform.right;
            float linearStrength = Mathf.Clamp01((detectionDistance - hit.distance) / detectionDistance);
            float avoidanceStrength = linearStrength * linearStrength;
            float avoidanceMagnitude = 5f;
            Vector3 offset = avoidanceDirection * avoidanceMagnitude * avoidanceStrength;
            Debug.Log($"Calculated avoidance offset: {offset}");
            return offset;
        }
        else
        {
            lastRockDistance = detectionDistance;
            return Vector3.zero;
        }
    }

    public float GetLastRockDistance()
    {
        return lastRockDistance;
    }

    public bool IsInReverseManeuver()
    {
        return inReverseManeuver;
    }

    public void SetReverseManeuverState(bool state)
    {
        inReverseManeuver = state;
    }
}