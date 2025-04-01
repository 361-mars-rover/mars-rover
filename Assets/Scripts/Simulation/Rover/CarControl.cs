using UnityEngine;
using System.Collections;
using System;
using UnityEditor.Experimental.GraphView;

public class CarControl : MonoBehaviour
{
    public float motorTorque = 2f;
    public float brakeTorque = 200f;
    public float maxSpeed = 1f;
    public float steeringRange = 30f;
    public float steeringRangeAtMaxSpeed = 10f;
    public float centreOfGravityOffset = 0f;
    public AnimationCurve torqueCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0.2f);
    public float accelerationSmoothness = 0.3f;
    public float brakingSmoothness = 0.5f;
    
    // Algorithmic movement parameters
    public float maxRadius = 100f;          // Maximum radius in meters
    public float radiusIncrement = 0.1f;    // Increment as percentage of maxRadius
    public float circleSpeed = 0.1f;        // Speed of circular movement (lower = slower)
    public Vector3 homeBasePosition;        // Current home base position
    
    private WheelControl[] wheels;
    private Rigidbody rigidBody;
    private float currentSpeedFactor;
    private float currentRadius;
    private float targetAngle;
    private float currentAngle;
    
    private bool isInitialized = false;

    // Define the control mode enum
    public enum ControlMode
    {
        Human,
        CircleAI,
        SunlightAI
    }
    
    // Replace the boolean with the enum
    public ControlMode currentControlMode = ControlMode.Human;

    // Don't comment out right away since its being used in Behaviour tree
    public bool useAI = false;

    public static string id;
    public bool allowInputs = true;
    
    public bool navigateToTarget = false;
    private bool avoidingRock = false;
    private bool inRecovery = false;
    private Vector3 avoidanceTarget = Vector3.zero;
    // A threshold distance to determine when we've reached the avoidance target.
    public float avoidanceThreshold = 2f;
    private int gemCount = 0;
    private float gemTimer = 0f;
    public float gemDetectionWindow = 2f;  // 2 seconds
    public int requiredGemCount = 1;       // 3 gems
    public float innerCircleFactor = 0.1f; // 10% of maxRadius
    public bool innerCircleMode = false;   // Indicates if we are doing an inner circle scan
    private Vector3 normalHomeBase;         // To backup the normal home base
    private float normalRadius;             // To backup the normal currentRadius
    private float lastGemTime = -1f;  // Stores the time of the first gem in the current window
    private int innerCircleLapsCompleted = 0;
    public int totalInnerCircleLaps = 10;
    private float lastRockDistance = Mathf.Infinity;
    private bool inReverseManeuver = false;
    private bool lookingForGem = false;
    // For the dust data
    private StartupSpawner startupSpawner;
    // For sunlight-seeking behavior
    private float lastDarknessPercentage = 0f;
    private float darknessTolerance = 2.0f; // How much darker it needs to get before changing direction
    private float adjustmentAngle = 45f; // How much to turn when finding a darker area (in degrees)
    private float forwardDistance = 20f; // How far ahead to set the target position
    private float logTimer = 0f;
    private float logInterval = 1.0f; // Log every 1 second
    void Awake()
    {
        Time.fixedDeltaTime = 0.01f; // Smaller value for more precise physics
        rigidBody = GetComponent<Rigidbody>();
        rigidBody.centerOfMass += Vector3.up * centreOfGravityOffset;
        wheels = GetComponentsInChildren<WheelControl>();
        id = Guid.NewGuid().ToString();
    }
    
    void Start()
    {
        // Initialize home base to starting position
        homeBasePosition = transform.position;
        
        // Start with smallest radius (10% of max)
        currentRadius = maxRadius * radiusIncrement;
        
        // Start the circle movement
        isInitialized = true;
        startupSpawner = FindFirstObjectByType<StartupSpawner>();
        if (startupSpawner == null)
        {
            Debug.LogError("StartupSpawner not found!");
        }
    }
    
    void Update()
    {
        /*if (!isInitialized) return;
        
        if (!useAI){
            ManualControl();
        }
        else{
            CircleAIUpdate();
        }*/

        if (!isInitialized) return;

        switch (currentControlMode)
        {
            case ControlMode.Human:
                ManualControl();
                break;
            case ControlMode.CircleAI:
                CircleAIUpdate();
                break;
            case ControlMode.SunlightAI:
                SunlightAIUpdate(); // You'll need to implement this method
                break;
        }
        
        // // Calculate target position on the circle
        // currentAngle += circleSpeed * Time.deltaTime;
        // if (currentAngle >= 360f)
        // {
        //     // Complete a full circle - move to the next radius
        //     currentAngle = 0f;
        //     currentRadius += maxRadius * radiusIncrement;
            
        //     // Check if we've reached max radius
        //     if (currentRadius > maxRadius)
        //     {
        //         // Terminate algorithm or reset to start again
        //         Debug.Log("Reached maximum radius. Algorithm complete.");
        //         isInitialized = false;
        //         return;
        //     }
            
        //     Debug.Log("Moving to next radius: " + currentRadius);
        // }
        
        // // Calculate target position on the current circle
        // float angleRad = currentAngle * Mathf.Deg2Rad;
        // Vector3 targetPosition = homeBasePosition + new Vector3(
        //     Mathf.Sin(angleRad) * currentRadius,
        //     0f,
        //     Mathf.Cos(angleRad) * currentRadius
        // );
        
        // // Calculate steering and acceleration to reach the target
        // Vector3 toTarget = targetPosition - transform.position;
        // toTarget.y = 0; // Ignore height differences
        
        // // Convert to local space for easier steering calculations
        // Vector3 localTarget = transform.InverseTransformPoint(targetPosition);
        
        // // Calculate steering amount (-1 to 1)
        // float steerAmount = Mathf.Clamp(localTarget.x / 5f, -1f, 1f);
        
        // // Calculate throttle amount (0 to 1)
        // float distanceToTarget = toTarget.magnitude;
        // float throttleAmount = Mathf.Clamp01(distanceToTarget / 10f);
        
        // // Apply steering and throttle to wheels
        // ApplyControlsToWheels(throttleAmount, steerAmount);
        
        // // Debug visualization
        // Debug.DrawLine(transform.position, targetPosition, Color.red);
        // Debug.DrawLine(homeBasePosition, targetPosition, Color.blue);
    }
    public void CircleAIUpdate()
    {
        Vector3 targetPosition;
        float gemRayDistance = 50f; // Increased detection range
        float gemSphereRadius = 2f; // Adjust to widen the detection area
        int gemLayerMask = 1 << LayerMask.NameToLayer("SphereGem");

        RaycastHit gemHit;
        if (Physics.SphereCast(transform.position, gemSphereRadius, transform.forward, out gemHit, gemRayDistance, gemLayerMask))
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
                avoidanceTarget = transform.position + rockAvoidanceOffset;
                Debug.Log("Rock detected. Switching to avoidance mode.");
            }
        }
        else
        {
            targetPosition = avoidanceTarget;
            if (Vector3.Distance(transform.position, avoidanceTarget) < avoidanceThreshold)
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

        // Compute steering and throttle.
        Vector3 toTarget = targetPosition - transform.position;
        toTarget.y = 0f;
        Vector3 localTarget = transform.InverseTransformPoint(targetPosition);
        float steerAmount = Mathf.Clamp(localTarget.x / 5f, -1f, 1f);
        float distanceToTarget = toTarget.magnitude;
        float throttleAmount = Mathf.Clamp01(distanceToTarget / 10f);
        float maxThrottleLimit = 0.5f;
        throttleAmount = Mathf.Min(throttleAmount, maxThrottleLimit);

        // Adjust throttle based on rock proximity.
        if (lastRockDistance < 5f)
        {
            Debug.Log("Rock extremely close. Applying full brakes and initiating reverse maneuver.");
            throttleAmount = 0f;
            if (!inReverseManeuver)
            {
                StartCoroutine(ReverseManeuver());
            }
            return;
        }
        else if (lastRockDistance < 15f)
        {
            Debug.Log("Rock detected at moderate range. Reducing throttle.");
            throttleAmount *= 0.5f;
        }

        ApplyControlsToWheels(throttleAmount, steerAmount);
        Debug.DrawLine(transform.position, targetPosition, Color.red);
        Debug.DrawLine(homeBasePosition, targetPosition, Color.blue);
    }
    private Vector3 CheckForRockAvoidance()
    {
        float detectionDistance = 30f;
        float sphereRadius = 2f;
        Vector3 origin = transform.position;
        Vector3 forwardDir = transform.forward;
        int rockLayerMask = 1 << LayerMask.NameToLayer("Rock");

        RaycastHit hit;
        if (Physics.SphereCast(origin, sphereRadius, forwardDir, out hit, detectionDistance, rockLayerMask))
        {
            lastRockDistance = hit.distance;
            Debug.Log($"Rock detected via SphereCast: {hit.collider.name}, distance: {hit.distance}");
            Vector3 rockPosition = hit.collider.transform.position;
            Vector3 toRock = rockPosition - transform.position;
            toRock.y = 0f;
            float angleToRock = Vector3.Angle(transform.forward, toRock);
            if (angleToRock > 25f)
            {
                return Vector3.zero;
            }
            float dot = Vector3.Dot(transform.right, toRock);
            Vector3 avoidanceDirection = (dot < 0) ? transform.right : -transform.right;
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
        inReverseManeuver = true;
        float reverseDuration = 1f;  // Reverse for 1 second
        float timer = 0f;
        while (timer < reverseDuration)
        {
            float reverseThrottle = -0.5f;
            float steerInput = 0f;  // No steering while reversing (adjust if needed)
            ApplyControlsToWheels(reverseThrottle, steerInput);
            timer += Time.deltaTime;
            yield return null;
        }
        
        // After reverse, reset lastRockDistance so the avoidance logic doesn't trigger immediately.
        lastRockDistance = 30f; // Reset to full detection distance (or a value of your choosing)
        
        inReverseManeuver = false;
        Debug.Log("Reverse maneuver complete. Resuming normal behavior.");
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
            ApplyControlsToWheels(reverseThrottle, steerInput);
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
                ApplyControlsToWheels(reverseThrottle, steerInput);
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

    public void GemDetected()
    {
        float currentTime = Time.time;
        if (lastGemTime < 0f || currentTime - lastGemTime > gemDetectionWindow)
        {
            gemCount = 1;
            lastGemTime = currentTime;
        }
        else
        {
            gemCount++;
        }
        Debug.Log($"Gem detected! gemCount = {gemCount}");
        if (gemCount >= requiredGemCount)
        {
            TriggerInnerCircleScan();
            gemCount = 0;
            lastGemTime = -1f;
        }
    }

    void TriggerInnerCircleScan()
    {
        Debug.Log("Triggering Inner Circle Scan!");
        normalHomeBase = homeBasePosition;
        normalRadius = currentRadius;
        homeBasePosition = transform.position;
        currentRadius = maxRadius * innerCircleFactor;
        currentAngle = 0f;
        innerCircleMode = true;
        innerCircleLapsCompleted = 0; // Reset lap count
    }
    //get as much sunligh as possible
    public void SunlightAIUpdate()
    {
        // Get current sunlight darkness percentage
        float currentDarkness = GetSunlightDarknessPercentage();
        
        // Store the darkness value for comparison in next frame
        if (!isInitialized)
        {
            lastDarknessPercentage = currentDarkness;
            isInitialized = true;
            return;
        }
        
        // Determine whether we're moving toward darker area
        bool movingToDarkerArea = currentDarkness > lastDarknessPercentage + darknessTolerance;
        
        // Track our target position
        Vector3 targetPosition;
        
        // If we're moving to a darker area, adjust direction
        if (movingToDarkerArea)
        {
            // Change direction by adjusting our heading
            // We'll use a simple strategy of turning by a fixed angle
            currentAngle += adjustmentAngle;
            Debug.Log($"Moving to darker area (prev: {lastDarknessPercentage:F2}%, current: {currentDarkness:F2}%). Adjusting direction.");
            
            // Reset avoidance state if we were avoiding something
            avoidingRock = false;
        }
        else
        {
            // Keep moving in current direction, but check for obstacles
            if (!avoidingRock)
            {
                // Check if a rock is detected on our path
                Vector3 rockAvoidanceOffset = CheckForRockAvoidance();
                if (rockAvoidanceOffset != Vector3.zero)
                {
                    // Rock detected: set avoidance mode
                    avoidingRock = true;
                    avoidanceTarget = transform.position + rockAvoidanceOffset;
                    Debug.Log("Rock detected. Switching to avoidance mode.");
                }
            }
            else
            {
                // If we're already avoiding a rock, check if we've cleared it
                if (Vector3.Distance(transform.position, avoidanceTarget) < avoidanceThreshold)
                {
                    avoidingRock = false;
                    Debug.Log("Avoidance complete. Resuming sunlight-seeking.");
                }
            }
        }
        
        // Calculate our target position based on current angle and state
        if (avoidingRock)
        {
            targetPosition = avoidanceTarget;
        }
        else
        {
            // Calculate target position in forward direction
            float angleRad = currentAngle * Mathf.Deg2Rad;
            targetPosition = transform.position + new Vector3(
                Mathf.Sin(angleRad),
                0f,
                Mathf.Cos(angleRad)
            ) * forwardDistance;
        }
        
        // Calculate steering and throttle toward the target position
        Vector3 toTarget = targetPosition - transform.position;
        toTarget.y = 0; // Ignore height differences
        
        Vector3 localTarget = transform.InverseTransformPoint(targetPosition);
        float steerAmount = Mathf.Clamp(localTarget.x / 5f, -1f, 1f);
        float distanceToTarget = toTarget.magnitude;
        float throttleAmount = Mathf.Clamp01(distanceToTarget / 10f);
        
        // Apply a maximum throttle limit for safety
        float maxThrottleLimit = 0.5f; // 50% throttle max
        throttleAmount = Mathf.Min(throttleAmount, maxThrottleLimit);
        
        // Handle rock avoidance maneuvers
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
        
        // Apply controls to the wheels
        ApplyControlsToWheels(throttleAmount, steerAmount);
        
        // Update the last darkness value for the next frame
        lastDarknessPercentage = currentDarkness;
        
        // Debug visualization
        Debug.DrawLine(transform.position, targetPosition, Color.yellow);
    }

    // Helper method to get the sunlight darkness percentage
    private float GetSunlightDarknessPercentage()
    {
        if (startupSpawner != null)
        {
            // Access dust_coloring using reflection since it's private
            System.Reflection.FieldInfo fieldInfo = typeof(StartupSpawner).GetField("dust_coloring", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (fieldInfo != null)
            {
                Color dustColor = (Color)fieldInfo.GetValue(startupSpawner);
                
                // Calculate perceived brightness using standard luminance formula
                float brightness = 0.299f * dustColor.r + 0.587f * dustColor.g + 0.114f * dustColor.b;
                
                // Invert the scale (0 = bright = 0% darkness, 1 = dark = 100% darkness)
                float darknessPercentage = (1f - brightness) * 100f;
                
                // Log once per second using a timer
                logTimer += Time.deltaTime;
                if (logTimer >= logInterval)
                {
                    Debug.Log($"Dust color: R:{dustColor.r:F2}, G:{dustColor.g:F2}, B:{dustColor.b:F2}");
                    Debug.Log($"Sunlight darkness: {darknessPercentage:F2}%");
                    logTimer = 0f;
                }
                
                return darknessPercentage;
            }
        }
        
        return 50f; // Default value if we can't get the actual darkness
    }
    void ApplyControlsToWheels(float throttleInput, float steerInput)
    {
        // Calculate forward speed relative to car's orientation
        float forwardSpeed = Vector3.Dot(transform.forward, rigidBody.linearVelocity);
        
        // Calculate speed factor for current speed
        float targetSpeedFactor = Mathf.InverseLerp(0, maxSpeed, Mathf.Abs(forwardSpeed));
        currentSpeedFactor = Mathf.Lerp(currentSpeedFactor, targetSpeedFactor, Time.deltaTime / accelerationSmoothness);
        
        // Apply torque curve for more realistic power delivery
        float torqueMultiplier = torqueCurve.Evaluate(currentSpeedFactor);
        float currentMotorTorque = motorTorque * torqueMultiplier;
        float currentSteerRange = Mathf.Lerp(steeringRange, steeringRangeAtMaxSpeed, currentSpeedFactor);
        
        // Determine if we need to brake based on distance to target or to change direction
        bool isReversing = forwardSpeed < -0.1f;
        bool needsToReverse = (throttleInput < 0 && forwardSpeed > 0.1f) || (throttleInput > 0 && isReversing);
        
        foreach (var wheel in wheels)
        {
            if (wheel.steerable)
            {
                wheel.HandleSteering(steerInput * currentSteerRange);
            }
            
            if (needsToReverse)
            {
                // Apply brakes when changing direction
                wheel.HandleMotor(0f, 0f);
                wheel.HandleBraking(true);
            }
            else
            {
                wheel.HandleMotor(throttleInput, currentMotorTorque);
                wheel.HandleBraking(false);
            }
        }
    }
    
    // Optional: This function can be called to set a new home base position
    public void SetNewHomeBase(Vector3 newPosition)
    {
        homeBasePosition = newPosition;
        currentRadius = maxRadius * radiusIncrement; // Reset to smallest radius
        currentAngle = 0f;
        isInitialized = true;
    }

    public void ManualControl()
    {
        float vInput = Input.GetAxis("Vertical");
        float hInput = Input.GetAxis("Horizontal");
        
        // calculate forward speed relative to car's orientation
        float forwardSpeed = Vector3.Dot(transform.forward, rigidBody.linearVelocity);
        
        // calculate speed factor for current speed
        float targetSpeedFactor = Mathf.InverseLerp(0, maxSpeed, Mathf.Abs(forwardSpeed));
        currentSpeedFactor = Mathf.Lerp(currentSpeedFactor, targetSpeedFactor, Time.deltaTime / accelerationSmoothness);
        
        // apply torque curve for more realistic power delivery
        float torqueMultiplier = torqueCurve.Evaluate(currentSpeedFactor);
        float currentMotorTorque = motorTorque * torqueMultiplier;
        float currentSteerRange = Mathf.Lerp(steeringRange, steeringRangeAtMaxSpeed, currentSpeedFactor);
        
        // determine if we're accelerating in the same direction as current movement
        bool isAccelerating = Mathf.Abs(vInput) > 0.1f;
        bool isReversing = forwardSpeed < -0.1f;
        bool isChangingDirection = (vInput > 0 && isReversing) || (vInput < 0 && forwardSpeed > 0.1f);
        
        foreach (var wheel in wheels)
        {
            if (wheel.steerable)
            {
                wheel.HandleSteering(hInput * currentSteerRange);
            }
            
            if (isChangingDirection)
            {
                // apply brakes when changing direction
                wheel.HandleMotor(0f, 0f);
                wheel.HandleBraking(true);
            }
            else if (isAccelerating)
            {
                wheel.HandleMotor(vInput, currentMotorTorque);
                wheel.HandleBraking(false);
            }
            else
            {
                // apply brakes when not accelerating or reversing
                wheel.HandleMotor(0f, 0f);
                wheel.HandleBraking(false);
            }
        }
    }
}