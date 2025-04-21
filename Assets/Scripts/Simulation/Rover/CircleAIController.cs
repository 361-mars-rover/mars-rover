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
            if (!innerCircleMode){
                TriggerInnerCircleScan();
            }
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