using System.Collections;
using UnityEngine;

public class SunlightAIController : BaseAIController
{
    private float lastDarknessPercentage = 0f;
    private StartupSpawner startupSpawner;
    private float logTimer = 0f;
    private float logInterval = 1.0f;
    private bool isInitialized = false;

    // Reference to the car's control input interface.

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

    public override void UpdateRover()
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
        HandleRockProximity(throttleAmount, steer);
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