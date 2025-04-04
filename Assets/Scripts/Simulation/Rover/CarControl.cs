using UnityEngine;
using System.Collections;
using System;

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
    private bool innerCircleMode = false;   // Indicates if we are doing an inner circle scan
    private Vector3 normalHomeBase;         // To backup the normal home base
    private float normalRadius;             // To backup the normal currentRadius
    private float lastGemTime = -1f;  // Stores the time of the first gem in the current window


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
    }
    
    void Update()
    {
        if (!isInitialized) return;
        
        if (!useAI){
            ManualControl();
        }
        else{
            CircleAIUpdate();
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
        // If we're not already avoiding a rock, compute the normal circle target.
        Vector3 targetPosition;
        if (!avoidingRock)
        {
            // Calculate target position on the current circle
            float angleRad = currentAngle * Mathf.Deg2Rad;
            targetPosition = homeBasePosition + new Vector3(
                Mathf.Sin(angleRad) * currentRadius,
                0f,
                Mathf.Cos(angleRad) * currentRadius
            );

            // Check if a rock is detected on our path.
            Vector3 rockAvoidanceOffset = CheckForRockAvoidance();
            if (rockAvoidanceOffset != Vector3.zero)
            {
                // Rock detected: set avoidance mode.
                avoidingRock = true;
                avoidanceTarget = transform.position + rockAvoidanceOffset;
                //Debug.Log("Rock detected. Switching to avoidance mode.");
            }
        }
        else
        {
            targetPosition = avoidanceTarget;
            // Check if we have reached or passed the avoidance target.
            if (Vector3.Distance(transform.position, avoidanceTarget) < avoidanceThreshold)
            {
                avoidingRock = false;
                //Debug.Log("Avoidance complete. Resuming circle path.");
            }
        }

        currentAngle += circleSpeed * Time.deltaTime;
        if (currentAngle >= 360f)
        {
                currentAngle = 0f;
                if (innerCircleMode)
            {
                // Inner circle scan is complete.
                innerCircleMode = false;
                // Restore the normal scanning parameters.
                homeBasePosition = normalHomeBase;
                currentRadius = normalRadius;
                //Debug.Log("Inner circle scan complete. Resuming normal circle scan.");
            }
            else
            {
                // Normal mode: Increase radius until max is reached.
                if (currentRadius < maxRadius)
                {
                    currentRadius += maxRadius * radiusIncrement;
                    //Debug.Log("Increasing circle radius to: " + currentRadius);
                }
                else
                {
                    // Once the circle at maximum radius is complete, shift the home base.
                    Vector3 newBase = homeBasePosition + Vector3.right * (2 * currentRadius);
                    homeBasePosition = newBase;
                    //Debug.Log("New home base set at: " + homeBasePosition);
                    // Reset the circle parameters for a new exploration.
                    currentRadius = maxRadius * radiusIncrement;
                }
            }
        }

        // Calculate steering and throttle toward the (possibly adjusted) target position.
        Vector3 toTarget = targetPosition - transform.position;
        toTarget.y = 0; // Ignore height differences

        Vector3 localTarget = transform.InverseTransformPoint(targetPosition);
        float steerAmount = Mathf.Clamp(localTarget.x / 5f, -1f, 1f);
        float distanceToTarget = toTarget.magnitude;
        float throttleAmount = Mathf.Clamp01(distanceToTarget / 10f);
        float maxThrottleLimit = 0.5f; // 50% throttle max
        throttleAmount = Mathf.Min(throttleAmount, maxThrottleLimit);

        ApplyControlsToWheels(throttleAmount, steerAmount);

        // Debug visualization: Draw the target position.
        Debug.DrawLine(transform.position, targetPosition, Color.red);
        Debug.DrawLine(homeBasePosition, targetPosition, Color.blue);
    }
     // This function detects rocks ahead and returns an avoidance offset if a rock is detected.
    private Vector3 CheckForRockAvoidance()
    {
        float detectionDistance = 30f;  // Increase as needed
        float sphereRadius = 2f;        // Adjust to match your rover’s width
        Vector3 origin = transform.position;
        Vector3 forwardDir = transform.forward;
        int rockLayerMask = 1 << LayerMask.NameToLayer("Rock");

        RaycastHit hit;
        // SphereCast returns true if it hits any rock within the detection distance
        if (Physics.SphereCast(origin, sphereRadius, forwardDir, out hit, detectionDistance, rockLayerMask))
        {
            //Debug.Log($"Rock detected via SphereCast: {hit.collider.name}, distance: {hit.distance}");
            Vector3 rockPosition = hit.collider.transform.position;
            Vector3 toRock = rockPosition - transform.position;
            toRock.y = 0f;  // Ignore vertical differences

            // Only avoid if the rock is almost directly ahead (within a narrow angle)
            float angleToRock = Vector3.Angle(transform.forward, toRock);
            if(angleToRock > 20f) // adjust this threshold as needed
            {
                // If the rock is off to the side, you might choose not to avoid it.
                return Vector3.zero;
            }

            // Determine which side the rock is on relative to the rover.
            float dot = Vector3.Dot(transform.right, toRock);
            Vector3 avoidanceDirection = dot < 0 ? transform.right : -transform.right;

            // Scale the avoidance offset by how close the rock is.
            float linearStrength = Mathf.Clamp01((detectionDistance - hit.distance) / detectionDistance);
            float avoidanceStrength = linearStrength * linearStrength; // Squared for a more aggressive response

            float avoidanceMagnitude = 5f; // Tweak this to determine how far to steer away.

            Vector3 offset = avoidanceDirection * avoidanceMagnitude * avoidanceStrength;
            //Debug.Log($"Calculated avoidance offset: {offset}");
            return offset;
        }
        // Debug.Log("No rock detected via SphereCast.");
        return Vector3.zero;
    }
    IEnumerator RecoveryManeuver()
    {
        inRecovery = true;

        // --- Phase 1: Normal recovery (reverse + slight steer) ---
        float firstRecoveryTime = 2.5f; // Duration of the first attempt
        float timer = 0f;
        while (timer < firstRecoveryTime)
        {
            // Reverse throttle + steer
            float reverseThrottle = -0.5f;
            float steerInput = 0.2f;
            ApplyControlsToWheels(reverseThrottle, steerInput);

            timer += Time.deltaTime;
            yield return null;
        }
        Debug.Log("First recovery attempt complete. Checking movement...");

        // --- Phase 2: Check if the rover is moving after 1 second ---
        float checkDuration = 1f;        // Wait 1 second to see if it moves
        float checkTimer = 0f;
        Vector3 startPos = transform.position;

        while (checkTimer < checkDuration)
        {
            checkTimer += Time.deltaTime;
            yield return null;
        }

        float distanceMoved = Vector3.Distance(transform.position, startPos);
        if (distanceMoved < 1.5f) // threshold for "moved enough"
        {
            // --- Phase 3: Still stuck, attempt a second recovery ---
            Debug.Log("Rover still stuck. Attempting second recovery maneuver.");
            float secondRecoveryTime = 4f; // Maybe a bit longer
            float secondTimer = 0f;
            while (secondTimer < secondRecoveryTime)
            {
                // Reverse throttle + steer the other way, for example
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
        float currentTime = Time.time;  // Get the current time

        // If this is the first gem or the previous window has expired, start a new window.
        if (lastGemTime < 0f || currentTime - lastGemTime > gemDetectionWindow)
        {
            gemCount = 1;
            lastGemTime = currentTime;
        }
        else
        {
            // Otherwise, we're still within the current 2-second window.
            gemCount++;
        }
        
        Debug.Log($"Gem detected! gemCount = {gemCount}");

        // If the gem count meets or exceeds the required count, trigger inner circle scan.
        if (gemCount >= requiredGemCount)
        {
            TriggerInnerCircleScan();
            // Reset the gem detection variables so we start fresh for the next batch.
            gemCount = 0;
            lastGemTime = -1f;
        }
    }
    void TriggerInnerCircleScan()
    {
        Debug.Log("Triggering Inner Circle Scan!");
        
        // Save the current (normal) scanning parameters so we can revert later.
        normalHomeBase = homeBasePosition;
        normalRadius = currentRadius;
        
        // Set the home base to the current position (or gem position, if desired)
        homeBasePosition = transform.position;
        
        // Set the inner circle radius (e.g., 10% of maxRadius)
        currentRadius = maxRadius * innerCircleFactor;
        
        // Reset the angle to start a new circle
        currentAngle = 0f;
        
        // Indicate that we are in inner circle mode.
        innerCircleMode = true;
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