// using UnityEngine;
// using System.Collections;
// using System;
// using UnityEditor.Experimental.GraphView;
// using AI;
// using System.Runtime.InteropServices;

// public class CarControl : MonoBehaviour
// {

//     public class Factory : ParameterizedMonoBehaviour{
//         public static CarControl Create(Rigidbody rigidbody, WheelControl[] wheels, GameObject gameObject = null){
//             Debug.Log("Creating car control");
//             CarControl c = Create<CarControl>(gameObject);
//             Time.fixedDeltaTime = 0.01f; // Smaller value for more precise physics
//             c.rigidBody = rigidbody;
//             c.wheels = wheels;
//             c.id = Guid.NewGuid().ToString();
//             return c;
//         }
//     }

//     public float motorTorque = 2f;
//     public float brakeTorque = 200f;
//     public float maxSpeed = 1f;
//     public float steeringRange = 30f;
//     public float steeringRangeAtMaxSpeed = 10f;
//     public float centreOfGravityOffset = 0f;
//     public AnimationCurve torqueCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0.2f);
//     public float accelerationSmoothness = 0.3f;
//     public float brakingSmoothness = 0.5f;
    
//     // Algorithmic movement parameters
//     public float maxRadius = 100f;          // Maximum radius in meters
//     public float radiusIncrement = 0.1f;    // Increment as percentage of maxRadius
//     public float circleSpeed = 0.1f;        // Speed of circular movement (lower = slower)
//     public Vector3 homeBasePosition;        // Current home base position
    
//     private WheelControl[] wheels;
//     private Rigidbody rigidBody;
//     private float currentSpeedFactor;
//     private float currentRadius;
//     private float targetAngle;
//     private float currentAngle;
    
//     private bool isInitialized = false;

//     // Define the control mode enum
//     public enum ControlMode
//     {
//         Human,
//         CircleAI,
//         SunlightAI
//     }
    
//     // Replace the boolean with the enum
//     public ControlMode currentControlMode = ControlMode.Human;

//     // Don't comment out right away since its being used in Behaviour tree
//     public bool useAI = false;

//     public string id;
//     public bool allowInputs = true;
    
//     public bool navigateToTarget = false;
//     private bool avoidingRock = false;
//     private bool inRecovery = false;
//     private Vector3 avoidanceTarget = Vector3.zero;
//     // A threshold distance to determine when we've reached the avoidance target.
//     public float avoidanceThreshold = 2f;
//     private int gemCount = 0;
//     private float gemTimer = 0f;
//     public float gemDetectionWindow = 2f;  // 2 seconds
//     public int requiredGemCount = 1;       // 3 gems
//     public float innerCircleFactor = 0.1f; // 10% of maxRadius
//     public bool innerCircleMode = false;   // Indicates if we are doing an inner circle scan
//     private Vector3 normalHomeBase;         // To backup the normal home base
//     private float normalRadius;             // To backup the normal currentRadius
//     private float lastGemTime = -1f;  // Stores the time of the first gem in the current window
//     private int innerCircleLapsCompleted = 0;
//     public int totalInnerCircleLaps = 10;
//     private float lastRockDistance = Mathf.Infinity;
//     private bool inReverseManeuver = false;
//     private bool lookingForGem = false;
//     // For the dust data
//     private StartupSpawner startupSpawner;
//     // For sunlight-seeking behavior
//     private float lastDarknessPercentage = 0f;
//     private float darknessTolerance = 2.0f; // How much darker it needs to get before changing direction
//     private float adjustmentAngle = 45f; // How much to turn when finding a darker area (in degrees)
//     private float forwardDistance = 20f; // How far ahead to set the target position
//     private float logTimer = 0f;
//     private float logInterval = 1.0f; // Log every 1 second

//     // Reference to the CircleAIController
//     private CircleAIController circleAIController;

//     private SunlightAIController sunlightAIController;

//     private AIControllerBase aIController;

//     void Awake()
//     {
//         Time.fixedDeltaTime = 0.01f; // Smaller value for more precise physics
//         rigidBody = GetComponent<Rigidbody>();
//         rigidBody.centerOfMass += Vector3.up * centreOfGravityOffset;
//         wheels = GetComponentsInChildren<WheelControl>();
//         id = Guid.NewGuid().ToString();
//     }
    
//     void Start()
//     {
//         // Initialize home base to starting position
//         homeBasePosition = transform.position;
        
//         // Start with smallest radius (10% of max)
//         currentRadius = maxRadius * radiusIncrement;
        
//         // Create and initialize the CircleAIController
//         circleAIController = gameObject.AddComponent<CircleAIController>();
//         circleAIController.Initialize(transform, homeBasePosition, currentRadius, currentAngle, 
//             circleSpeed, maxRadius, radiusIncrement, avoidanceThreshold);
//         circleAIController.SetInnerCircleParameters(normalRadius, innerCircleFactor, totalInnerCircleLaps);
//         circleAIController.SetHomeBasePositions(homeBasePosition, normalHomeBase);
//         circleAIController.SetCircleParameters(currentRadius, currentAngle, innerCircleMode, innerCircleLapsCompleted);
//         circleAIController.SetAvoidanceState(avoidingRock, avoidanceTarget);
        
//         // Start the circle movement
//         isInitialized = true;
//         startupSpawner = FindFirstObjectByType<StartupSpawner>();
//         if (startupSpawner == null)
//         {
//             Debug.LogError("StartupSpawner not found!");
//         }

//         sunlightAIController = gameObject.AddComponent<SunlightAIController>();
//         sunlightAIController.Initialize(transform, currentAngle, darknessTolerance, adjustmentAngle, forwardDistance, avoidanceThreshold, startupSpawner);
//         sunlightAIController.SetAvoidanceState(avoidingRock, avoidanceTarget);
//     }
    
//     void Update()
//     {
//         if (!isInitialized) return;

//         switch (currentControlMode)
//         {
//             case ControlMode.Human:
//                 ManualControl();
//                 break;
//             case ControlMode.CircleAI:
//                 CircleAIUpdate();
//                 break;
//             case ControlMode.SunlightAI:
//                 SunlightAIUpdate(); // You'll need to implement this method
//                 break;
//         }
//     }

//     public void CircleAIUpdate()
//     {
//         // Call the CircleAIUpdate function from the controller and get the results
//         var result = circleAIController.UpdateAI();
        
//         // Update our local variables with the values returned from the controller
//         Vector3 targetPosition = result.targetPosition;
//         currentAngle = result.updatedAngle;
//         avoidingRock = result.isAvoidingRock;
//         avoidanceTarget = result.avoidanceTarget;
//         innerCircleMode = result.isInnerCircle;
//         currentRadius = result.updatedRadius;
//         innerCircleLapsCompleted = result.updatedLaps;
        
//         // Update the controller with our state
//         circleAIController.SetCircleParameters(currentRadius, currentAngle, innerCircleMode, innerCircleLapsCompleted);
//         circleAIController.SetHomeBasePositions(homeBasePosition, normalHomeBase);
//         circleAIController.SetAvoidanceState(avoidingRock, avoidanceTarget);
        
//         // Get rock distance from the controller
//         lastRockDistance = circleAIController.GetLastRockDistance();
//         inReverseManeuver = circleAIController.IsInReverseManeuver();

//         // Compute steering and throttle.
//         Vector3 toTarget = targetPosition - transform.position;
//         toTarget.y = 0f;
//         Vector3 localTarget = transform.InverseTransformPoint(targetPosition);
//         float steerAmount = Mathf.Clamp(localTarget.x / 5f, -1f, 1f);
//         float distanceToTarget = toTarget.magnitude;
//         float throttleAmount = Mathf.Clamp01(distanceToTarget / 10f);
//         float maxThrottleLimit = 0.5f;
//         throttleAmount = Mathf.Min(throttleAmount, maxThrottleLimit);

//         // Adjust throttle based on rock proximity.
//         if (lastRockDistance < 5f)
//         {
//             Debug.Log("Rock extremely close. Applying full brakes and initiating reverse maneuver.");
//             throttleAmount = 0f;
//             if (!inReverseManeuver)
//             {
//                 StartCoroutine(ReverseManeuver());
//             }
//             return;
//         }
//         else if (lastRockDistance < 15f)
//         {
//             Debug.Log("Rock detected at moderate range. Reducing throttle.");
//             throttleAmount *= 0.5f;
//         }

//         ApplyControlsToWheels(throttleAmount, steerAmount);
//         Debug.DrawLine(transform.position, targetPosition, Color.red);
//         Debug.DrawLine(homeBasePosition, targetPosition, Color.blue);
//     }

//     IEnumerator ReverseManeuver()
//     {
//         circleAIController.SetReverseManeuverState(true);
//         inReverseManeuver = true;
//         float reverseDuration = 1f;  // Reverse for 1 second
//         float timer = 0f;
//         while (timer < reverseDuration)
//         {
//             float reverseThrottle = -0.5f;
//             float steerInput = 0f;  // No steering while reversing (adjust if needed)
//             ApplyControlsToWheels(reverseThrottle, steerInput);
//             timer += Time.deltaTime;
//             yield return null;
//         }
        
//         // After reverse, reset lastRockDistance so the avoidance logic doesn't trigger immediately.
//         lastRockDistance = 30f; // Reset to full detection distance (or a value of your choosing)
        
//         inReverseManeuver = false;
//         circleAIController.SetReverseManeuverState(false);
//         Debug.Log("Reverse maneuver complete. Resuming normal behavior.");
//     }

//     IEnumerator RecoveryManeuver()
//     {
//         inRecovery = true;
//         float firstRecoveryTime = 2.5f;
//         float timer = 0f;
//         while (timer < firstRecoveryTime)
//         {
//             float reverseThrottle = -0.5f;
//             float steerInput = 0.2f;
//             ApplyControlsToWheels(reverseThrottle, steerInput);
//             timer += Time.deltaTime;
//             yield return null;
//         }
//         Debug.Log("First recovery attempt complete. Checking movement...");
//         float checkDuration = 1f;
//         float checkTimer = 0f;
//         Vector3 startPos = transform.position;
//         while (checkTimer < checkDuration)
//         {
//             checkTimer += Time.deltaTime;
//             yield return null;
//         }
//         float distanceMoved = Vector3.Distance(transform.position, startPos);
//         if (distanceMoved < 1.5f)
//         {
//             Debug.Log("Rover still stuck. Attempting second recovery maneuver.");
//             float secondRecoveryTime = 4f;
//             float secondTimer = 0f;
//             while (secondTimer < secondRecoveryTime)
//             {
//                 float reverseThrottle = -0.5f;
//                 float steerInput = -0.2f;
//                 ApplyControlsToWheels(reverseThrottle, steerInput);
//                 secondTimer += Time.deltaTime;
//                 yield return null;
//             }
//             Debug.Log("Second recovery attempt complete. Resuming normal behavior.");
//         }
//         else
//         {
//             Debug.Log("Rover moved sufficiently after first recovery attempt. Resuming normal behavior.");
//         }
//         inRecovery = false;
//     }

//     void OnCollisionEnter(Collision collision)
//     {
//         if (collision.gameObject.layer == LayerMask.NameToLayer("Rock"))
//         {
//             if (!inRecovery)
//             {
//                 Debug.Log("Collision with rock detected. Starting recovery maneuver.");
//                 StartCoroutine(RecoveryManeuver());
//             }
//         }
//     }

//     public void GemDetected()
//     {
//         float currentTime = Time.time;
//         if (lastGemTime < 0f || currentTime - lastGemTime > gemDetectionWindow)
//         {
//             gemCount = 1;
//             lastGemTime = currentTime;
//         }
//         else
//         {
//             gemCount++;
//         }
//         Debug.Log($"Gem detected! gemCount = {gemCount}");
//         if (gemCount >= requiredGemCount)
//         {
//             TriggerInnerCircleScan();
//             gemCount = 0;
//             lastGemTime = -1f;
//         }
//     }

//     void TriggerInnerCircleScan()
//     {
//         Debug.Log("Triggering Inner Circle Scan!");
//         normalHomeBase = homeBasePosition;
//         normalRadius = currentRadius;
//         homeBasePosition = transform.position;
//         currentRadius = maxRadius * innerCircleFactor;
//         currentAngle = 0f;
//         innerCircleMode = true;
//         innerCircleLapsCompleted = 0; // Reset lap count
        
//         // Update the CircleAIController with the new state
//         circleAIController.SetCircleParameters(currentRadius, currentAngle, innerCircleMode, innerCircleLapsCompleted);
//         circleAIController.SetHomeBasePositions(homeBasePosition, normalHomeBase);
//         circleAIController.SetInnerCircleParameters(normalRadius, innerCircleFactor, totalInnerCircleLaps);
//     }
    
//     //get as much sunligh as possible
//     public void SunlightAIUpdate()
//     {
//         // Call the SunlightAIUpdate function from the controller and get the results
//         var result = sunlightAIController.SunlightAIUpdate();
        
//         // Update our local variables with the values returned from the controller
//         Vector3 targetPosition = result.targetPosition;
//         currentAngle = result.updatedAngle;
//         avoidingRock = result.isAvoidingRock;
//         avoidanceTarget = result.avoidanceTarget;
//         lastRockDistance = result.rockDistance;
        
//         // Update the controller with our state
//         sunlightAIController.SetCurrentAngle(currentAngle);
//         sunlightAIController.SetAvoidanceState(avoidingRock, avoidanceTarget);
        
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

//     // Helper method to get the sunlight darkness percentage
//     /*private float GetSunlightDarknessPercentage()
//     {
//         if (startupSpawner != null)
//         {
//             // Access dust_coloring using reflection since it's private
//             System.Reflection.FieldInfo fieldInfo = typeof(StartupSpawner).GetField("dust_coloring", 
//                 System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
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
//     }*/
    
//     /*private Vector3 CheckForRockAvoidance()
//     {
//         float detectionDistance = 30f;
//         float sphereRadius = 2f;
//         Vector3 origin = transform.position;
//         Vector3 forwardDir = transform.forward;
//         int rockLayerMask = 1 << LayerMask.NameToLayer("Rock");

//         RaycastHit hit;
//         if (Physics.SphereCast(origin, sphereRadius, forwardDir, out hit, detectionDistance, rockLayerMask))
//         {
//             lastRockDistance = hit.distance;
//             Debug.Log($"Rock detected via SphereCast: {hit.collider.name}, distance: {hit.distance}");
//             Vector3 rockPosition = hit.collider.transform.position;
//             Vector3 toRock = rockPosition - transform.position;
//             toRock.y = 0f;
//             float angleToRock = Vector3.Angle(transform.forward, toRock);
//             if (angleToRock > 25f)
//             {
//                 return Vector3.zero;
//             }
//             float dot = Vector3.Dot(transform.right, toRock);
//             Vector3 avoidanceDirection = (dot < 0) ? transform.right : -transform.right;
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
//     }*/

//     void ApplyControlsToWheels(float throttleInput, float steerInput)
//     {
//         // Calculate forward speed relative to car's orientation
//         float forwardSpeed = Vector3.Dot(transform.forward, rigidBody.linearVelocity);
        
//         // Calculate speed factor for current speed
//         float targetSpeedFactor = Mathf.InverseLerp(0, maxSpeed, Mathf.Abs(forwardSpeed));
//         currentSpeedFactor = Mathf.Lerp(currentSpeedFactor, targetSpeedFactor, Time.deltaTime / accelerationSmoothness);
        
//         // Apply torque curve for more realistic power delivery
//         float torqueMultiplier = torqueCurve.Evaluate(currentSpeedFactor);
//         float currentMotorTorque = motorTorque * torqueMultiplier;
//         float currentSteerRange = Mathf.Lerp(steeringRange, steeringRangeAtMaxSpeed, currentSpeedFactor);
        
//         // Determine if we need to brake based on distance to target or to change direction
//         bool isReversing = forwardSpeed < -0.1f;
//         bool needsToReverse = (throttleInput < 0 && forwardSpeed > 0.1f) || (throttleInput > 0 && isReversing);
        
//         foreach (var wheel in wheels)
//         {
//             if (wheel.steerable)
//             {
//                 wheel.HandleSteering(steerInput * currentSteerRange);
//             }
            
//             if (needsToReverse)
//             {
//                 // Apply brakes when changing direction
//                 wheel.HandleMotor(0f, 0f);
//                 wheel.HandleBraking(true);
//             }
//             else
//             {
//                 wheel.HandleMotor(throttleInput, currentMotorTorque);
//                 wheel.HandleBraking(false);
//             }
//         }
//     }
    
//     // Optional: This function can be called to set a new home base position
//     public void SetNewHomeBase(Vector3 newPosition)
//     {
//         homeBasePosition = newPosition;
//         currentRadius = maxRadius * radiusIncrement; // Reset to smallest radius
//         currentAngle = 0f;
//         isInitialized = true;
//     }

//     public void ManualControl()
//     {
//         float vInput = Input.GetAxis("Vertical");
//         float hInput = Input.GetAxis("Horizontal");
        
//         // calculate forward speed relative to car's orientation
//         float forwardSpeed = Vector3.Dot(transform.forward, rigidBody.linearVelocity);
        
//         // calculate speed factor for current speed
//         float targetSpeedFactor = Mathf.InverseLerp(0, maxSpeed, Mathf.Abs(forwardSpeed));
//         currentSpeedFactor = Mathf.Lerp(currentSpeedFactor, targetSpeedFactor, Time.deltaTime / accelerationSmoothness);
        
//         // apply torque curve for more realistic power delivery
//         float torqueMultiplier = torqueCurve.Evaluate(currentSpeedFactor);
//         float currentMotorTorque = motorTorque * torqueMultiplier;
//         float currentSteerRange = Mathf.Lerp(steeringRange, steeringRangeAtMaxSpeed, currentSpeedFactor);
        
//         // determine if we're accelerating in the same direction as current movement
//         bool isAccelerating = Mathf.Abs(vInput) > 0.1f;
//         bool isReversing = forwardSpeed < -0.1f;
//         bool isChangingDirection = (vInput > 0 && isReversing) || (vInput < 0 && forwardSpeed > 0.1f);
        
//         foreach (var wheel in wheels)
//         {
//             if (wheel.steerable)
//             {
//                 wheel.HandleSteering(hInput * currentSteerRange);
//             }
            
//             if (isChangingDirection)
//             {
//                 // apply brakes when changing direction
//                 wheel.HandleMotor(0f, 0f);
//                 wheel.HandleBraking(true);
//             }
//             else if (isAccelerating)
//             {
//                 wheel.HandleMotor(vInput, currentMotorTorque);
//                 wheel.HandleBraking(false);
//             }
//             else
//             {
//                 // apply brakes when not accelerating or reversing
//                 wheel.HandleMotor(0f, 0f);
//                 wheel.HandleBraking(false);
//             }
//         }
//     }
// }

using UnityEngine;
using System.Collections;
using System;

public class CarControl : MonoBehaviour, IAIInput
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
    public bool navigateToTarget = false;

    public bool useAI = true;

    private WheelControl[] wheels;
    private Rigidbody rigidBody;
    private float currentSpeedFactor;

    public enum ControlMode { Human, AI }
    public ControlMode currentControlMode = ControlMode.Human;
    public string id;

    // Reference to the active AI controller.
    public IAIController aiController;

    void Awake()
    {
        Time.fixedDeltaTime = 0.01f;
        rigidBody = GetComponent<Rigidbody>();
        rigidBody.centerOfMass += Vector3.up * centreOfGravityOffset;
        wheels = GetComponentsInChildren<WheelControl>();
        id = Guid.NewGuid().ToString();
    }

    void Start()
    {
        // Set up home base at starting position
        Vector3 homeBasePosition = transform.position;

        // Calculate initial circle radius
        float currentRadius = CircleAIParams.MAX_RADIUS * CircleAIParams.RADIUS_INCREMENT;
        float currentAngle = 0f;

        // Add and initialize Circle AI
        CircleAIController circleAIController = gameObject.AddComponent<CircleAIController>();
        circleAIController.Initialize(
            transform,
            homeBasePosition,
            currentRadius,
            currentAngle,
            CircleAIParams.CIRCLE_SPEED,
            CircleAIParams.MAX_RADIUS,
            CircleAIParams.RADIUS_INCREMENT,
            CarParams.AVOIDANCE_THRESHOLD,
            this
        );

        // Try to find the StartupSpawner in the scene
        StartupSpawner startupSpawner = FindFirstObjectByType<StartupSpawner>();
        if (startupSpawner == null)
        {
            Debug.LogError("StartupSpawner not found!");
        }

        // Add and initialize Sunlight AI
        SunlightAIController sunlightAIController = gameObject.AddComponent<SunlightAIController>();
        sunlightAIController.Initialize(
            transform,
            currentAngle,
            SunlightAIParams.DARKNESS_TOLERANCE,
            SunlightAIParams.ADJUSTMENT_ANGLE,
            SunlightAIParams.FORWARD_DISTANCE,
            CarParams.AVOIDANCE_THRESHOLD,
            startupSpawner,
            this
        );

        // Optional: if SunlightAIController has this method
        // sunlightAIController.SetAvoidanceState(false, Vector3.zero);

        // Pick which AI to start with
        aiController = sunlightAIController;  // or sunlightAIController
    }

    void Update()
    {
        if (currentControlMode == ControlMode.Human)
        {
            ManualControl();
        }
        else // AI Control
        {
            aiController.UpdateRover();
        }
    }
    
    // This method will be called by the AI controllers.
    public void SetControls(float throttle, float steer)
    {
        // Here you could add any extra processing or constraints if needed.
        ApplyControlsToWheels(throttle, steer);
    }

    void ApplyControlsToWheels(float throttleInput, float steerInput)
    {
        float forwardSpeed = Vector3.Dot(transform.forward, rigidBody.velocity);
        float targetSpeedFactor = Mathf.InverseLerp(0, maxSpeed, Mathf.Abs(forwardSpeed));
        currentSpeedFactor = Mathf.Lerp(currentSpeedFactor, targetSpeedFactor, Time.deltaTime / accelerationSmoothness);
        float torqueMultiplier = torqueCurve.Evaluate(currentSpeedFactor);
        float currentMotorTorque = motorTorque * torqueMultiplier;
        float currentSteerRange = Mathf.Lerp(steeringRange, steeringRangeAtMaxSpeed, currentSpeedFactor);
        
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

    public void ManualControl()
    {
        float vInput = Input.GetAxis("Vertical");
        float hInput = Input.GetAxis("Horizontal");
        float forwardSpeed = Vector3.Dot(transform.forward, rigidBody.velocity);
        float targetSpeedFactor = Mathf.InverseLerp(0, maxSpeed, Mathf.Abs(forwardSpeed));
        currentSpeedFactor = Mathf.Lerp(currentSpeedFactor, targetSpeedFactor, Time.deltaTime / accelerationSmoothness);
        float torqueMultiplier = torqueCurve.Evaluate(currentSpeedFactor);
        float currentMotorTorque = motorTorque * torqueMultiplier;
        float currentSteerRange = Mathf.Lerp(steeringRange, steeringRangeAtMaxSpeed, currentSpeedFactor);
        
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
                wheel.HandleMotor(0f, 0f);
                wheel.HandleBraking(false);
            }
        }
    }
}