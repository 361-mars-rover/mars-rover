using System.Collections;
using UnityEngine;

public class SunlightAIController : BaseAIController
{
    private float lastDarknessPercentage = 0f;
    private SimulationStart startupSpawner;
    private float logTimer = 0f;
    private float logInterval = 1.0f;
    private bool isInitialized = false;

    // Wall detection and avoidance variables (same as from CircleAI)
    private bool avoidingWall = false;
    private float wallDetectionDistance = 2f; // Distance to detect walls
    private float wallAvoidanceTimeout = 5f; // Time to spend in avoidance mode
    private float wallAvoidanceTimer = 0f;
    // States for wall avoidance sequence
    private enum WallAvoidanceState { Reversing, Turning, Resuming }
    private WallAvoidanceState wallAvoidanceState = WallAvoidanceState.Reversing;
    private float reverseTime = 5.0f; // Time to spend reversing



    public void Initialize(Transform transform, float angle, float tolerance, 
                           float adjustment, float forward, float avoidThreshold,
                           SimulationStart spawner, IAIInput input)
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

    // Wall detection method (same as from CircleAI)
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
        Debug.Log("Updating rover");

        if (!isInitialized)
        {
            Debug.Log("Rover is not initialized");
            aiInput.SetControls(0.0f, 0.0f);
            return;
        }

        Vector3 desiredTarget = carTransform.position; // Initialize

        // --- Wall Detection (highest priority) ---
        if (!avoidingWall && CheckForWallProximity())
        {
            avoidingWall = true;
            wallAvoidanceTimer = 0f;
            wallAvoidanceState = WallAvoidanceState.Reversing; // Start with reversing
            
            // Set a target behind the rover for the reverse step
            Vector3 reverseDirection = -carTransform.forward;
            avoidanceTarget = carTransform.position + (reverseDirection * 30f);
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
                        aiInput.SetControls(-1f, 0f); // Reverse at full speed
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
                    return; // Skip the rest of the update when reversing
                    
                case WallAvoidanceState.Turning:
                    // Let the regular targeting system handle the turn
                    desiredTarget = avoidanceTarget;
                    
                    // If we've reached our turn target or spent enough time turning
                    if (Vector3.Distance(carTransform.position, avoidanceTarget) < 10f || 
                        wallAvoidanceTimer >= wallAvoidanceTimeout)
                    {
                        wallAvoidanceState = WallAvoidanceState.Resuming;
                        wallAvoidanceTimer = 0f;
                        Debug.Log("Finished turning, now resuming normal path.");
                    }
                    break;
                    
                case WallAvoidanceState.Resuming:
                    // Just give a little time before completely returning to normal
                    desiredTarget = carTransform.position + carTransform.forward * 10f;
                    
                    if (wallAvoidanceTimer >= 1.0f) // Brief pause before resuming
                    {
                        avoidingWall = false;
                        Debug.Log("Wall avoidance complete. Resuming normal path.");
                    }
                    break;
            }
        }
        // Only proceed with regular logic if not handling a wall collision in reversing state
        else
        {
            // Calculate current darkness.
            float currentDarkness = GetSunlightDarknessPercentage();
            bool movingToDarkerArea = currentDarkness > lastDarknessPercentage + darknessTolerance;

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
        }

        Debug.DrawLine(carTransform.position, desiredTarget, Color.yellow);

        // Here we calculate simple control inputs from the desired target.
        Vector3 toTarget = desiredTarget - carTransform.position;
        toTarget.y = 0;
        Vector3 localTarget = carTransform.InverseTransformPoint(desiredTarget);
        float steer = Mathf.Clamp(localTarget.x / 5f, -1f, 1f);
        float distance = toTarget.magnitude;
        float throttleAmount = Mathf.Clamp01(distance / 10f);
        throttleAmount = Mathf.Min(throttleAmount, 0.5f);
        
        // Don't call HandleRockProximity if we're in wall avoidance mode
        if (!avoidingWall || wallAvoidanceState != WallAvoidanceState.Reversing)
        {
            HandleRockProximity(throttleAmount, steer);
        }
    }

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
}