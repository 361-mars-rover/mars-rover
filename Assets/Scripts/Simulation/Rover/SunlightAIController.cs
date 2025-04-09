// using UnityEngine;
// using System.Collections;
// using System.Reflection;
// using AI;

// public class SunlightAIController : AIController<(Vector3 targetPosition, float updatedAngle, bool isAvoidingRock, Vector3 avoidanceTarget, float rockDistance)>
// {
//     // Parameters for the controller
//     private Transform carTransform;
//     private float currentAngle;
//     private float darknessTolerance = 2.0f;
//     private float adjustmentAngle = 45f;
//     private float forwardDistance = 20f;
//     private float avoidanceThreshold = 2f;
//     private bool avoidingRock = false;
//     private Vector3 avoidanceTarget = Vector3.zero;
//     private float lastRockDistance = Mathf.Infinity;
//     private float lastDarknessPercentage = 0f;
//     private StartupSpawner startupSpawner;
//     private float logTimer = 0f;
//     private float logInterval = 1.0f;
//     private bool isInitialized = false;

//     // Method to initialize the controller with necessary parameters
//     public void Initialize(Transform transform, float angle, float tolerance, 
//                           float adjustment, float forward, float avoidThreshold,
//                           StartupSpawner spawner)
//     {
//         carTransform = transform;
//         currentAngle = angle;
//         darknessTolerance = tolerance;
//         adjustmentAngle = adjustment;
//         forwardDistance = forward;
//         avoidanceThreshold = avoidThreshold;
//         startupSpawner = spawner;
//         lastDarknessPercentage = GetSunlightDarknessPercentage();
//         isInitialized = true;
//     }

//     public void SetAvoidanceState(bool isAvoiding, Vector3 avoidTarget)
//     {
//         avoidingRock = isAvoiding;
//         avoidanceTarget = avoidTarget;
//     }

//     public (Vector3 targetPosition, float updatedAngle, bool isAvoidingRock, Vector3 avoidanceTarget, float rockDistance) 
//     SunlightAIUpdate()
//     {
//         if (!isInitialized)
//         {
//             return (carTransform.position + carTransform.forward * forwardDistance, currentAngle, 
//                    avoidingRock, avoidanceTarget, lastRockDistance);
//         }
        
//         // Get current sunlight darkness percentage
//         float currentDarkness = GetSunlightDarknessPercentage();
        
//         // Determine whether we're moving toward darker area
//         bool movingToDarkerArea = currentDarkness > lastDarknessPercentage + darknessTolerance;
        
//         // Track our target position
//         Vector3 targetPosition;
        
//         // If we're moving to a darker area, adjust direction
//         if (movingToDarkerArea)
//         {
//             // Change direction by adjusting our heading
//             currentAngle += adjustmentAngle;
//             Debug.Log($"Moving to darker area (prev: {lastDarknessPercentage:F2}%, current: {currentDarkness:F2}%). Adjusting direction.");
            
//             // Reset avoidance state if we were avoiding something
//             avoidingRock = false;
//         }
//         else
//         {
//             // Keep moving in current direction, but check for obstacles
//             if (!avoidingRock)
//             {
//                 // Check if a rock is detected on our path
//                 Vector3 rockAvoidanceOffset = CheckForRockAvoidance();
//                 if (rockAvoidanceOffset != Vector3.zero)
//                 {
//                     // Rock detected: set avoidance mode
//                     avoidingRock = true;
//                     avoidanceTarget = carTransform.position + rockAvoidanceOffset;
//                     Debug.Log("Rock detected. Switching to avoidance mode.");
//                 }
//             }
//             else
//             {
//                 // If we're already avoiding a rock, check if we've cleared it
//                 if (Vector3.Distance(carTransform.position, avoidanceTarget) < avoidanceThreshold)
//                 {
//                     avoidingRock = false;
//                     Debug.Log("Avoidance complete. Resuming sunlight-seeking.");
//                 }
//             }
//         }
        
//         // Calculate our target position based on current angle and state
//         if (avoidingRock)
//         {
//             targetPosition = avoidanceTarget;
//         }
//         else
//         {
//             // Calculate target position in forward direction
//             float angleRad = currentAngle * Mathf.Deg2Rad;
//             targetPosition = carTransform.position + new Vector3(
//                 Mathf.Sin(angleRad),
//                 0f,
//                 Mathf.Cos(angleRad)
//             ) * forwardDistance;
//         }
        
//         // Update the last darkness value for the next frame
//         lastDarknessPercentage = currentDarkness;
        
//         // Debug visualization
//         Debug.DrawLine(carTransform.position, targetPosition, Color.yellow);
        
//         return (targetPosition, currentAngle, avoidingRock, avoidanceTarget, lastRockDistance);
//     }

//     // Helper method to get the sunlight darkness percentage
//     private float GetSunlightDarknessPercentage()
//     {
//         if (startupSpawner != null)
//         {
//             // Access dust_coloring using reflection since it's private
//             FieldInfo fieldInfo = typeof(StartupSpawner).GetField("dust_coloring", 
//                 BindingFlags.NonPublic | BindingFlags.Instance);
            
//             if (fieldInfo != null)
//             {
//                 Color dustColor = (Color)fieldInfo.GetValue(startupSpawner);
                
//                 // Calculate perceived brightness using standard luminance formula
//                 float brightness = 0.299f * dustColor.r + 0.587f * dustColor.g + 0.114f * dustColor.b;
                
//                 // Invert the scale (0 = bright = 0% darkness, 1 = dark = 100% darkness)
//                 float darknessPercentage = (1f - brightness) * 100f;
                
//                 // Log once per second using a timer
//                 logTimer += Time.deltaTime;
//                 if (logTimer >= logInterval)
//                 {
//                     Debug.Log($"Dust color: R:{dustColor.r:F2}, G:{dustColor.g:F2}, B:{dustColor.b:F2}");
//                     Debug.Log($"Sunlight darkness: {darknessPercentage:F2}%");
//                     logTimer = 0f;
//                 }
                
//                 return darknessPercentage;
//             }
//         }
        
//         return 50f; // Default value if we can't get the actual darkness
//     }
    
//     private Vector3 CheckForRockAvoidance()
//     {
//         float detectionDistance = 30f;
//         float sphereRadius = 2f;
//         Vector3 origin = carTransform.position;
//         Vector3 forwardDir = carTransform.forward;
//         int rockLayerMask = 1 << LayerMask.NameToLayer("Rock");

//         RaycastHit hit;
//         if (Physics.SphereCast(origin, sphereRadius, forwardDir, out hit, detectionDistance, rockLayerMask))
//         {
//             lastRockDistance = hit.distance;
//             Debug.Log($"Rock detected via SphereCast: {hit.collider.name}, distance: {hit.distance}");
//             Vector3 rockPosition = hit.collider.transform.position;
//             Vector3 toRock = rockPosition - carTransform.position;
//             toRock.y = 0f;
//             float angleToRock = Vector3.Angle(carTransform.forward, toRock);
//             if (angleToRock > 25f)
//             {
//                 return Vector3.zero;
//             }
//             float dot = Vector3.Dot(carTransform.right, toRock);
//             Vector3 avoidanceDirection = (dot < 0) ? carTransform.right : -carTransform.right;
//             float linearStrength = Mathf.Clamp01((detectionDistance - hit.distance) / detectionDistance);
//             float avoidanceStrength = linearStrength * linearStrength;
//             float avoidanceMagnitude = 5f;
//             Vector3 offset = avoidanceDirection * avoidanceMagnitude * avoidanceStrength;
//             Debug.Log($"Calculated avoidance offset: {offset}");
//             return offset;
//         }
//         else
//         {
//             lastRockDistance = detectionDistance;
//             return Vector3.zero;
//         }
//     }
    
//     public float GetLastRockDistance()
//     {
//         return lastRockDistance;
//     }

//     public void SetCurrentAngle(float angle)
//     {
//         currentAngle = angle;
//     }

//     public override (Vector3 targetPosition, float updatedAngle, bool isAvoidingRock, Vector3 avoidanceTarget, float rockDistance) UpdateAI()
//     {
//                 if (!isInitialized)
//         {
//             return (carTransform.position + carTransform.forward * forwardDistance, currentAngle, 
//                    avoidingRock, avoidanceTarget, lastRockDistance);
//         }
        
//         // Get current sunlight darkness percentage
//         float currentDarkness = GetSunlightDarknessPercentage();
        
//         // Determine whether we're moving toward darker area
//         bool movingToDarkerArea = currentDarkness > lastDarknessPercentage + darknessTolerance;
        
//         // Track our target position
//         Vector3 targetPosition;
        
//         // If we're moving to a darker area, adjust direction
//         if (movingToDarkerArea)
//         {
//             // Change direction by adjusting our heading
//             currentAngle += adjustmentAngle;
//             Debug.Log($"Moving to darker area (prev: {lastDarknessPercentage:F2}%, current: {currentDarkness:F2}%). Adjusting direction.");
            
//             // Reset avoidance state if we were avoiding something
//             avoidingRock = false;
//         }
//         else
//         {
//             // Keep moving in current direction, but check for obstacles
//             if (!avoidingRock)
//             {
//                 // Check if a rock is detected on our path
//                 Vector3 rockAvoidanceOffset = CheckForRockAvoidance();
//                 if (rockAvoidanceOffset != Vector3.zero)
//                 {
//                     // Rock detected: set avoidance mode
//                     avoidingRock = true;
//                     avoidanceTarget = carTransform.position + rockAvoidanceOffset;
//                     Debug.Log("Rock detected. Switching to avoidance mode.");
//                 }
//             }
//             else
//             {
//                 // If we're already avoiding a rock, check if we've cleared it
//                 if (Vector3.Distance(carTransform.position, avoidanceTarget) < avoidanceThreshold)
//                 {
//                     avoidingRock = false;
//                     Debug.Log("Avoidance complete. Resuming sunlight-seeking.");
//                 }
//             }
//         }
        
//         // Calculate our target position based on current angle and state
//         if (avoidingRock)
//         {
//             targetPosition = avoidanceTarget;
//         }
//         else
//         {
//             // Calculate target position in forward direction
//             float angleRad = currentAngle * Mathf.Deg2Rad;
//             targetPosition = carTransform.position + new Vector3(
//                 Mathf.Sin(angleRad),
//                 0f,
//                 Mathf.Cos(angleRad)
//             ) * forwardDistance;
//         }
        
//         // Update the last darkness value for the next frame
//         lastDarknessPercentage = currentDarkness;
        
//         // Debug visualization
//         Debug.DrawLine(carTransform.position, targetPosition, Color.yellow);
        
//         return (targetPosition, currentAngle, avoidingRock, avoidanceTarget, lastRockDistance);
//     }

//     protected override void ProcessResult((Vector3 targetPosition, float updatedAngle, bool isAvoidingRock, Vector3 avoidanceTarget, float rockDistance) result)
//     {
        
//         // Update our local variables with the values returned from the controller
//         Vector3 targetPosition = result.targetPosition;
//         currentAngle = result.updatedAngle;
//         avoidingRock = result.isAvoidingRock;
//         avoidanceTarget = result.avoidanceTarget;
//         lastRockDistance = result.rockDistance;
        
//         // Update the controller with our state
//         SetCurrentAngle(currentAngle);
//         SetAvoidanceState(avoidingRock, avoidanceTarget);
        
//         // Calculate steering and throttle toward the target position
//         Vector3 toTarget = targetPosition - transform.position;
//         toTarget.y = 0; // Ignore height differences
        
//         Vector3 localTarget = transform.InverseTransformPoint(targetPosition);
//         float steerAmount = Mathf.Clamp(localTarget.x / 5f, -1f, 1f);
//         float distanceToTarget = toTarget.magnitude;
//         float throttleAmount = Mathf.Clamp01(distanceToTarget / 10f);
        
//         // Apply a maximum throttle limit for safety
//         float maxThrottleLimit = 0.5f; // 50% throttle max
//         throttleAmount = Mathf.Min(throttleAmount, maxThrottleLimit);
        
//         // Handle rock avoidance maneuvers
//         if (lastRockDistance < 5f)
//         {
//             // If the rock is extremely close, trigger a reverse maneuver if not already in one
//             if (!inReverseManeuver)
//             {
//                 Debug.Log("Rock extremely close. Initiating reverse maneuver.");
//                 StartCoroutine(ReverseManeuver());
//             }
//             return;
//         }
//         else if (lastRockDistance < 15f)
//         {
//             Debug.Log("Rock detected at moderate range. Reducing throttle.");
//             throttleAmount *= 0.5f; // Reduce throttle by half
//         }
        
//         // Apply controls to the wheels
//         ApplyControlsToWheels(throttleAmount, steerAmount);
//     }

//     }
// }

using UnityEngine;
using System.Reflection;

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
        Debug.Log("Computing darkness");
        float currentDarkness = GetSunlightDarknessPercentage();
        bool movingToDarkerArea = currentDarkness > lastDarknessPercentage + darknessTolerance;
        Vector3 desiredTarget;

        if (movingToDarkerArea)
        {
            // Adjust heading
            currentAngle += adjustmentAngle;
            Debug.Log($"Moving to darker area (prev: {lastDarknessPercentage:F2}%, current: {currentDarkness:F2}%). Adjusting direction.");
            avoidingRock = false;
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
        float throttle = Mathf.Clamp01(distance / 10f);
        throttle = Mathf.Min(throttle, 0.5f);

        // Optionally adjust throttle if a rock is very close.
        if (lastRockDistance < 5f)
        {
            // You might want to initiate a reverse maneuver.
            // For simplicity, we send zero throttle.
            throttle = 0f;
        }
        else if (lastRockDistance < 15f)
        {
            throttle *= 0.5f;
        }

        // Finally, send the computed control inputs back to the car.
        aiInput.SetControls(throttle, steer);
    }

    private float GetSunlightDarknessPercentage()
    {

        if (startupSpawner != null)
        {
            Debug.Log("startup spawner is not null");
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
}