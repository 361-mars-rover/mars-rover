using System.Collections;
using UnityEngine;

public class SunlightAIController : MonoBehaviour, IAIController
{
    private Transform carTransform;
    private float currentAngle;
    private float darknessTolerance = 2.0f;
    private float adjustmentAngle = 45f;
    private float forwardDistance = 20f;
    private float avoidanceThreshold = 2f;
    private bool avoidingRock = false;
    private Vector3 avoidanceTarget = Vector3.zero;
    private float lastRockDistance = Mathf.Infinity;
    private float lastDarknessPercentage = 0f;
    private StartupSpawner startupSpawner;
    private float logTimer = 0f;
    private float logInterval = 1.0f;
    private bool isInitialized = false;

    private bool inReverseManeuver = false;

    private bool inRecovery = false;

    // Reference to the car's control input interface.
    private IAIInput aiInput;

    public void Initialize(Transform transform, float angle, float tolerance, 
                           float adjustment, float forward, float avoidThreshold,
                           StartupSpawner spawner, IAIInput input)
    {
        carTransform = transform;
        currentAngle = angle;
        darknessTolerance = tolerance;
        adjustmentAngle = adjustment;
        forwardDistance = forward;
        avoidanceThreshold = avoidThreshold;
        startupSpawner = spawner;
        lastDarknessPercentage = GetSunlightDarknessPercentage();
        isInitialized = true;
        aiInput = input;
    }

    public void UpdateRover()
    {
        Debug.Log("Updating rover");

        if (!isInitialized)
        {
            Debug.Log("Rover is not initialized");
            aiInput.SetControls(0.0f, 0.0f);
            return;
        }

        // Calculate current darkness.
        float currentDarkness = GetSunlightDarknessPercentage();
        bool movingToDarkerArea = currentDarkness > lastDarknessPercentage + darknessTolerance;
        Vector3 desiredTarget;

        if (movingToDarkerArea)
        {
            // Adjust heading
            currentAngle += adjustmentAngle;
            Debug.Log($"Moving to darker area (prev: {lastDarknessPercentage:F2}%, current: {currentDarkness:F2}%). Adjusting direction.");
            avoidingRock = false;  // Reset avoidance mode when moving to a darker area
        }
        else
        {
            if (!avoidingRock)
            {
                Vector3 rockOffset = CheckForRockAvoidance();
                if (rockOffset != Vector3.zero)
                {
                    avoidingRock = true;
                    avoidanceTarget = carTransform.position + rockOffset;
                    Debug.Log("Rock detected. Switching to avoidance mode.");
                }
            }
            else
            {
                if (Vector3.Distance(carTransform.position, avoidanceTarget) < avoidanceThreshold)
                {
                    avoidingRock = false;
                    Debug.Log("Avoidance complete. Resuming sunlight-seeking.");
                }
            }
        }

        // Determine the desired target position.
        if (avoidingRock)
        {
            desiredTarget = avoidanceTarget;
        }
        else
        {
            float angleRad = currentAngle * Mathf.Deg2Rad;
            desiredTarget = carTransform.position + new Vector3(
                Mathf.Sin(angleRad), 
                0f, 
                Mathf.Cos(angleRad)
            ) * forwardDistance;
        }

        lastDarknessPercentage = currentDarkness;
        Debug.DrawLine(carTransform.position, desiredTarget, Color.yellow);

        // Here we calculate simple control inputs from the desired target.
        Vector3 toTarget = desiredTarget - carTransform.position;
        toTarget.y = 0;
        Vector3 localTarget = carTransform.InverseTransformPoint(desiredTarget);
        float steer = Mathf.Clamp(localTarget.x / 5f, -1f, 1f);
        float distance = toTarget.magnitude;
        float throttleAmount = Mathf.Clamp01(distance / 10f);
        throttleAmount = Mathf.Min(throttleAmount, 0.5f);

        if (lastRockDistance < 5f)
        {
            // If the rock is extremely close, trigger a reverse maneuver if not already in one
            if (!inReverseManeuver)
            {
                Debug.Log("Rock extremely close. Initiating reverse maneuver.");
                StartCoroutine(ReverseManeuver());
            }
            return;
        }
        else if (lastRockDistance < 15f)
        {
            Debug.Log("Rock detected at moderate range. Reducing throttle.");
            throttleAmount *= 0.5f; // Reduce throttle by half
        }

        // Finally, send the computed control inputs back to the car.
        aiInput.SetControls(throttleAmount, steer);
    }

    public float GetLastRockDistance() => lastRockDistance;
    public bool IsInReverseManeuver() => inReverseManeuver;
    public void SetReverseManeuverState(bool state) => inReverseManeuver = state;
    private float GetSunlightDarknessPercentage()
    {

        if (startupSpawner != null)
        {
            Color dustColor = startupSpawner.GetDustColouring();
            float brightness = 0.299f * dustColor.r + 0.587f * dustColor.g + 0.114f * dustColor.b;
            float darknessPercentage = (1f - brightness) * 100f;

            logTimer += Time.deltaTime;
            if (logTimer >= logInterval)
            {
                Debug.Log($"Dust color: R:{dustColor.r:F2}, G:{dustColor.g:F2}, B:{dustColor.b:F2}");
                Debug.Log($"Sunlight darkness: {darknessPercentage:F2}%");
                logTimer = 0f;
            }
            return darknessPercentage;
        }
        return 50f; // Default fallback

    }

    private Vector3 CheckForRockAvoidance()
    {
        float detectionDistance = 30f;
        float sphereRadius = 2f;
        Vector3 origin = carTransform.position;
        Vector3 forwardDir = carTransform.forward;
        int rockLayerMask = 1 << LayerMask.NameToLayer("Rock");

        if (Physics.SphereCast(origin, sphereRadius, forwardDir, out RaycastHit hit, detectionDistance, rockLayerMask))
        {
            lastRockDistance = hit.distance;
            Debug.Log($"Rock detected via SphereCast: {hit.collider.name}, distance: {hit.distance}");
            Vector3 rockPos = hit.collider.transform.position;
            Vector3 toRock = rockPos - carTransform.position;
            toRock.y = 0f;
            float angleToRock = Vector3.Angle(carTransform.forward, toRock);
            if (angleToRock > 25f) return Vector3.zero;
            
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

        IEnumerator ReverseManeuver()
        {
            this.SetReverseManeuverState(true);
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