using UnityEngine;
using System.Collections;

public class CarControl : MonoBehaviour
{
    public float motorTorque = 10000f;
    public float brakeTorque = 2000f;
    public float maxSpeed = 50000f;
    public float steeringRange = 30f;
    public float steeringRangeAtMaxSpeed = 10f;
    public float centreOfGravityOffset = -1f;
    public AnimationCurve torqueCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0.2f);
    public float accelerationSmoothness = 0.3f;
    public float brakingSmoothness = 0.5f;
    
    // Algorithmic movement parameters
    public float maxRadius = 100f;          // Maximum radius in meters
    public float radiusIncrement = 0.1f;    // Increment as percentage of maxRadius
    public float circleSpeed = 0.5f;        // Speed of circular movement (lower = slower)
    public Vector3 homeBasePosition;        // Current home base position
    
    private WheelControl[] wheels;
    private Rigidbody rigidBody;
    private float currentSpeedFactor;
    private float currentRadius;
    private float targetAngle;
    private float currentAngle;
    
    private bool isInitialized = false;
    
    void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        rigidBody.centerOfMass += Vector3.up * centreOfGravityOffset;
        wheels = GetComponentsInChildren<WheelControl>();
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
        
        // Calculate target position on the circle
        currentAngle += circleSpeed * Time.deltaTime;
        if (currentAngle >= 360f)
        {
            // Complete a full circle - move to the next radius
            currentAngle = 0f;
            currentRadius += maxRadius * radiusIncrement;
            
            // Check if we've reached max radius
            if (currentRadius > maxRadius)
            {
                // Terminate algorithm or reset to start again
                Debug.Log("Reached maximum radius. Algorithm complete.");
                isInitialized = false;
                return;
            }
            
            Debug.Log("Moving to next radius: " + currentRadius);
        }
        
        // Calculate target position on the current circle
        float angleRad = currentAngle * Mathf.Deg2Rad;
        Vector3 targetPosition = homeBasePosition + new Vector3(
            Mathf.Sin(angleRad) * currentRadius,
            0f,
            Mathf.Cos(angleRad) * currentRadius
        );
        
        // Calculate steering and acceleration to reach the target
        Vector3 toTarget = targetPosition - transform.position;
        toTarget.y = 0; // Ignore height differences
        
        // Convert to local space for easier steering calculations
        Vector3 localTarget = transform.InverseTransformPoint(targetPosition);
        
        // Calculate steering amount (-1 to 1)
        float steerAmount = Mathf.Clamp(localTarget.x / 5f, -1f, 1f);
        
        // Calculate throttle amount (0 to 1)
        float distanceToTarget = toTarget.magnitude;
        float throttleAmount = Mathf.Clamp01(distanceToTarget / 10f);
        
        // Apply steering and throttle to wheels
        ApplyControlsToWheels(throttleAmount, steerAmount);
        
        // Debug visualization
        Debug.DrawLine(transform.position, targetPosition, Color.red);
        Debug.DrawLine(homeBasePosition, targetPosition, Color.blue);
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
}

//OLD CAR CONTROL
// using UnityEngine;

// public class CarControl : MonoBehaviour
// {
//     public float motorTorque = 10000f;
//     public float brakeTorque = 2000f;
//     public float maxSpeed = 50f;
//     public float steeringRange = 30f;
//     public float steeringRangeAtMaxSpeed = 10f;
//     public float centreOfGravityOffset = -1f;
//     public AnimationCurve torqueCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0.2f);
//     public float accelerationSmoothness = 0.3f;
//     public float brakingSmoothness = 0.5f;
    
//     private WheelControl[] wheels;
//     private Rigidbody rigidBody;
//     private float currentSpeedFactor;
    
//     void Awake()
//     {
//         rigidBody = GetComponent<Rigidbody>();
//         rigidBody.centerOfMass += Vector3.up * centreOfGravityOffset;
//         wheels = GetComponentsInChildren<WheelControl>();
//     }

//     void Update()
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