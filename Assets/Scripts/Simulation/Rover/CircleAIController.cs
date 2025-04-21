using System.Collections;
using UnityEngine;

public class CircleAIController : BaseAIController
{
    // Configuration and state fields.
    private Vector3 homeBasePosition;
    private float currentRadius;
    private float circleSpeed;
    private float maxRadius;
    private float radiusIncrement;
    private bool lookingForGem = false;
    
    private bool innerCircleMode = false;
    private Vector3 normalHomeBase;
    private float normalRadius;
    private int innerCircleLapsCompleted = 0;
    private int totalInnerCircleLaps = 10;
    private float innerCircleFactor = 0.1f;

    // Wall detection and avoidance variables
    private bool avoidingWall = false;
    private float wallDetectionDistance = 2f; // Distance to detect walls (changed from 20 to 2)
    private float wallAvoidanceTimeout = 5f; // Time to spend in avoidance mode
    private float wallAvoidanceTimer = 0f;
    // States for wall avoidance sequence
    private enum WallAvoidanceState { Reversing, Turning, Resuming }
    private WallAvoidanceState wallAvoidanceState = WallAvoidanceState.Reversing;
    private float reverseTime = 5.0f; // Time to spend reversing


    /// <summary>
    /// Initializes the circle AI with its parameters and a reference to the CarControl input.
    /// </summary>
    public void Initialize(Transform transform, Vector3 homeBase, float radius, float angle, 
                           float speed, float maxRad, float radIncrement, float avoidThreshold,
                           IAIInput input)
    {
        carTransform = transform;
        homeBasePosition = homeBase;
        currentRadius = radius;
        currentAngle = angle;
        circleSpeed = speed;
        maxRadius = maxRad;
        radiusIncrement = radIncrement;
        avoidanceThreshold = avoidThreshold;
        aiInput = input;
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

    private bool CheckForWallProximity()
    {
        // Use a spherecast to detect nearby walls
        int wallLayerMask = 1 << LayerMask.NameToLayer("Default");
        Collider[] hitColliders = Physics.OverlapSphere(carTransform.position, wallDetectionDistance, wallLayerMask);
        
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("InvisibleWall"))
            {
                Vector3 directionToWall = hitCollider.ClosestPoint(carTransform.position) - carTransform.position;
                float distanceToWall = directionToWall.magnitude;
                
                if (distanceToWall < wallDetectionDistance)
                {
                    Debug.Log("Wall detected at distance: " + distanceToWall);
                    return true;
                }
            }
        }
        
        return false;
    }

    public override void UpdateRover()
    {
        Vector3 targetPosition = carTransform.position; // Initialize
        float gemRayDistance = 100f;
        float gemSphereRadius = 2f;
        int gemLayerMask = 1 << LayerMask.NameToLayer("SphereGem");

        RaycastHit gemHit;
        // --- Wall Detection (highest priority) ---
        if (!avoidingWall && CheckForWallProximity())
        {
            avoidingWall = true;
            wallAvoidanceTimer = 0f;
            wallAvoidanceState = WallAvoidanceState.Reversing; // Start with reversing
            
            // Set a target behind the rover for the reverse step
            Vector3 reverseDirection = -carTransform.forward;
            avoidanceTarget = carTransform.position + (reverseDirection * 10f);
            Debug.Log("Wall detected. Starting reverse maneuver.");
        }

        // --- Handle ongoing wall avoidance ---
        if (avoidingWall)
        {
            wallAvoidanceTimer += Time.deltaTime;
            
            switch (wallAvoidanceState)
            {
                case WallAvoidanceState.Reversing:
                    // Apply reverse controls
                    if (aiInput != null)
                    {
                        aiInput.SetControls(-1f, 0f); // Reverse at half speed
                    }
                    
                    // After sufficient reversing time, switch to turning
                    if (wallAvoidanceTimer >= reverseTime)
                    {
                        wallAvoidanceState = WallAvoidanceState.Turning;
                        wallAvoidanceTimer = 0f;
                        
                        // Set a target 90 degrees to the right or left
                        // Randomize direction to avoid getting stuck in patterns
                        float turnDirection = (UnityEngine.Random.value > 0.5f) ? 1f : -1f;
                        Vector3 turnVector = (carTransform.right * turnDirection) + (carTransform.forward * 0.5f);
                        avoidanceTarget = carTransform.position + (turnVector.normalized * 15f);
                        Debug.Log("Finished reversing, now turning.");
                    }
                    break;
                    
                case WallAvoidanceState.Turning:
                    // Let the regular targeting system handle the turn
                    targetPosition = avoidanceTarget;
                    
                    // If we've reached our turn target or spent enough time turning
                    if (Vector3.Distance(carTransform.position, avoidanceTarget) < 5f || 
                        wallAvoidanceTimer >= wallAvoidanceTimeout)
                    {
                        wallAvoidanceState = WallAvoidanceState.Resuming;
                        wallAvoidanceTimer = 0f;
                        Debug.Log("Finished turning, now resuming normal path.");
                    }
                    break;
                    
                case WallAvoidanceState.Resuming:
                    // Just give a little time before completely returning to normal
                    targetPosition = carTransform.position + carTransform.forward * 10f;
                    
                    if (wallAvoidanceTimer >= 1.0f) // Brief pause before resuming
                    {
                        avoidingWall = false;
                        Debug.Log("Wall avoidance complete. Resuming normal path.");
                    }
                    break;
            }
        }

        // --- Gem Detection (has higher priority) ---
        else if (Physics.SphereCast(carTransform.position, gemSphereRadius, carTransform.forward, out gemHit, gemRayDistance, gemLayerMask))
        {
            lookingForGem = true;
            targetPosition = gemHit.collider.transform.position;
            Debug.Log("Gem detected via sphere cast. Moving toward gem.");
            if (!innerCircleMode){
                TriggerInnerCircleScan();
            }
        }
        // --- Rock avoidance and normal circle logic ---
        else if (!avoidingRock)
        {
            Vector3 rockAvoidanceOffset = CheckForRockAvoidance();
            float patrolRadius = innerCircleMode 
            ? currentRadius 
            : maxRadius;
            float angleRad = currentAngle * Mathf.Deg2Rad;
            targetPosition = homeBasePosition + new Vector3(
                Mathf.Sin(angleRad) * patrolRadius,
                0f,
                Mathf.Cos(angleRad) * patrolRadius
            );
            if (rockAvoidanceOffset != Vector3.zero)
            {
                avoidingRock = true;
                avoidanceTarget = carTransform.position + rockAvoidanceOffset;
                Debug.Log("Rock detected. Switching to avoidance mode!");
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

        // --- Update angle and radius based on laps and mode ---
        currentAngle += circleSpeed * 360f * Time.deltaTime;
        if (currentAngle >= 360f)
        {
            currentAngle = 0f;
            if (innerCircleMode)
            {
                innerCircleLapsCompleted++;
                Debug.Log("Inner circle lap " + innerCircleLapsCompleted + " completed.");
                if (innerCircleLapsCompleted < totalInnerCircleLaps)
                {
                    float innerIncrement = maxRadius * (innerCircleFactor / totalInnerCircleLaps);
                    currentRadius += innerIncrement;
                    Debug.Log("Expanding inner circle radius to: " + currentRadius);
                }
                else
                {
                    innerCircleMode = false;
                    innerCircleLapsCompleted = 0;
                    homeBasePosition = normalHomeBase;
                    currentRadius = normalRadius;
                    Debug.Log("Inner circle scan complete. Resuming normal circle scan.");
                }
            }
            else
            {
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

        Debug.DrawLine(carTransform.position, targetPosition, Color.blue);

        // --- Compute control inputs based on target ---
        Vector3 toTarget = targetPosition - carTransform.position;
        toTarget.y = 0;
        Vector3 localTarget = carTransform.InverseTransformPoint(targetPosition);
        float steer = Mathf.Clamp(localTarget.x / 5f, -1f, 1f);
        float distance = toTarget.magnitude;
        float throttle = Mathf.Clamp01(distance / 10f);
        throttle = Mathf.Min(throttle, 0.5f);

        // Optionally adjust throttle if a rock is extremely close.
        if (lastRockDistance < 5f)
        {
            throttle = 0f;  // You could trigger a reverse maneuver here if desired.
        }
        else if (lastRockDistance < 15f)
        {
            throttle *= 0.5f;
        }

        // --- Reverse maneuver handling ---
        if (lastRockDistance < 5f && !inReverseManeuver)
        {
            StartCoroutine(ReverseManeuver());
            return;
        }

        // --- Push calculated controls to CarControl ---
        if (aiInput != null)
        {
            aiInput.SetControls(throttle, steer);
        }
    }
    void TriggerInnerCircleScan()
    {
        normalHomeBase       = homeBasePosition;
        normalRadius         = currentRadius;
        homeBasePosition     = carTransform.position;
        currentRadius        = maxRadius * innerCircleFactor;
        currentAngle         = 0f;
        innerCircleMode      = true;
        innerCircleLapsCompleted = 0;
        Debug.Log("Inner circle scan triggered");
    }
}