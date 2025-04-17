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

    public override void UpdateRover()
    {
        Vector3 targetPosition;

        // --- Gem Detection (has higher priority) ---
        float gemRayDistance = 100f;
        float gemSphereRadius = 2f;
        int gemLayerMask = 1 << LayerMask.NameToLayer("SphereGem");

        RaycastHit gemHit;
        if (Physics.SphereCast(carTransform.position, gemSphereRadius, carTransform.forward, out gemHit, gemRayDistance, gemLayerMask))
        {
            lookingForGem = true;
            targetPosition = gemHit.collider.transform.position;
            Debug.Log("Gem detected via sphere cast. Moving toward gem.");
        }
        // --- Rock avoidance and normal circle logic ---
        else if (!avoidingRock)
        {
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

        // --- Update angle and radius based on laps and mode ---
        currentAngle += circleSpeed * Time.deltaTime;
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
    /// <summary>
    /// Checks for nearby rocks and returns an avoidance offset.
    /// </summary>
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

    // Optionally, you can add accessor methods if needed.
    public float GetLastRockDistance() => lastRockDistance;
    public bool IsInReverseManeuver() => inReverseManeuver;
    public void SetReverseManeuverState(bool state) => inReverseManeuver = state;

        IEnumerator ReverseManeuver()
        {
            SetReverseManeuverState(true);
            inReverseManeuver = true;
            float reverseDuration = 1f;  // Reverse for 1 second
            float timer = 0f;
            while (timer < reverseDuration)
            {
                float reverseThrottle = -0.5f;
                float steerInput = 0f;  // No steering while reversing (adjust if needed)
                aiInput.SetControls(reverseThrottle, steerInput);
                timer += Time.deltaTime;
                yield return null;
            }
            
            // After reverse, reset lastRockDistance so the avoidance logic doesn't trigger immediately.
            lastRockDistance = 30f; // Reset to full detection distance (or a value of your choosing)
            
            inReverseManeuver = false;
            SetReverseManeuverState(false);
            Debug.Log("Reverse maneuver complete. Resuming normal behavior.");
        }

        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Rock"))
            {
                if (!inRecovery)
                {
                    Debug.Log("Collision with rock detected. Starting recovery maneuver.");
                    StartCoroutine(RecoveryManeuver());
                }
            }
        }

        IEnumerator RecoveryManeuver()
        {
            inRecovery = true;
            float firstRecoveryTime = 2.5f;
            float timer = 0f;
            while (timer < firstRecoveryTime)
            {
                float reverseThrottle = -0.5f;
                float steerInput = 0.2f;
                aiInput.SetControls(reverseThrottle, steerInput);
                timer += Time.deltaTime;
                yield return null;
            }
            Debug.Log("First recovery attempt complete. Checking movement...");
            float checkDuration = 1f;
            float checkTimer = 0f;
            Vector3 startPos = transform.position;
            while (checkTimer < checkDuration)
            {
                checkTimer += Time.deltaTime;
                yield return null;
            }
            float distanceMoved = Vector3.Distance(transform.position, startPos);
            if (distanceMoved < 1.5f)
            {
                Debug.Log("Rover still stuck. Attempting second recovery maneuver.");
                float secondRecoveryTime = 4f;
                float secondTimer = 0f;
                while (secondTimer < secondRecoveryTime)
                {
                    float reverseThrottle = -0.5f;
                    float steerInput = -0.2f;
                    aiInput.SetControls(reverseThrottle, steerInput);
                    secondTimer += Time.deltaTime;
                    yield return null;
                }
                Debug.Log("Second recovery attempt complete. Resuming normal behavior.");
            }
            else
            {
                Debug.Log("Rover moved sufficiently after first recovery attempt. Resuming normal behavior.");
            }
            inRecovery = false;
        }

}