
using UnityEngine;
using System.Collections;
using System;
using Unity.VisualScripting;

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

    private CircleAIController circleAIController;

    private SunlightAIController sunlightAIController;
    public IAIController aiController;

    void Awake()
    {
        Time.fixedDeltaTime = 0.01f;
        rigidBody = GetComponent<Rigidbody>();
        rigidBody.centerOfMass += Vector3.up * centreOfGravityOffset;
        wheels = GetComponentsInChildren<WheelControl>();
        id = Guid.NewGuid().ToString();
    }

    public void PrepareAIControllers(StartupSpawner startupSpawner)
    {
        // Set up home base at starting position
        Vector3 homeBasePosition = transform.position;

        // Calculate initial circle radius
        float currentRadius = CircleAIParams.MAX_RADIUS * CircleAIParams.RADIUS_INCREMENT;
        float currentAngle = 0f;

        // Add and initialize Circle AI
        if (circleAIController == null)
        {
            circleAIController = gameObject.AddComponent<CircleAIController>();
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
        }

        // Add and initialize Sunlight AI
        if (sunlightAIController == null)
        {
            sunlightAIController = gameObject.AddComponent<SunlightAIController>();
            sunlightAIController.Initialize(
                transform,
                currentAngle,
                SunlightAIParams.DARKNESS_TOLERANCE,
                SunlightAIParams.ADJUSTMENT_ANGLE,
                SunlightAIParams.FORWARD_DISTANCE,
                CarParams.AVOIDANCE_THRESHOLD,
                startupSpawner,  // You can assign StartupSpawner if necessary, but it’s not critical here
                this
            );
        }

        Debug.Log("AI controllers are prepared and ready.");
    }

    void Start()
    {
    }

    public void SetAI(AIMode mode){
        switch (mode)
        {
            case AIMode.SunlightAI:
                Debug.Log("AI set to sunglight");
                aiController = sunlightAIController;
                break;
            case AIMode.CircleAI:
                Debug.Log("AI set to circle");
                aiController = circleAIController;
                break;
        }
    }

    void Update()
    {
        if (currentControlMode == ControlMode.Human)
        {
            ManualControl();
        }
        else // AI Control
        {
            if (aiController == null){
                Debug.Log("AI isn't set yet");
            }
            Debug.Log("Updating AI");
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
        float forwardSpeed = Vector3.Dot(transform.forward, rigidBody.linearVelocity);
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
        float forwardSpeed = Vector3.Dot(transform.forward, rigidBody.linearVelocity);
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