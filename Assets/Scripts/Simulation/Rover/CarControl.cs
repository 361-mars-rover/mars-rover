
using UnityEngine;
using System.Collections;
using System;
using Unity.VisualScripting;
// Chloe Gavrilovic 260955835

public class CarControl : MonoBehaviour, IAIInput
{
    public float motorTorque = 2f;
    public float brakeTorque = 200f;
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
    private CircleAIController circleAIController;
    private SunlightAIController sunlightAIController;
    public IAIController aiController;

    // DELETE
    public AIMode currentAI = AIMode.CircleAI;

    void Awake()
    {
        Time.fixedDeltaTime = 0.01f;
        rigidBody = GetComponent<Rigidbody>();
        rigidBody.centerOfMass += Vector3.up * centreOfGravityOffset;
        wheels = GetComponentsInChildren<WheelControl>();
        id = Guid.NewGuid().ToString();
    }

    // assign the AI controller to the car
    public void PrepareAIControllers(SimulationStart startupSpawner)
    {
        // set up home base at starting position
        Vector3 homeBasePosition = transform.position;

        // calculate initial circle radius
        float currentRadius = CircleAIParams.MAX_RADIUS * CircleAIParams.RADIUS_INCREMENT;
        float currentAngle = 0f;

        // add and initialize circle AI
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

        // add and initialize sunlight AI
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
                startupSpawner, 
                this
            );
        }
        // DELETE
        aiController = circleAIController;
        // END DELETE
        Debug.Log("AI controllers are prepared and ready.");
    }

    void Start()
    {
    }

    // set the AI controller mode 
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
            case AIMode.Manual:
                currentControlMode = ControlMode.Human;
                break;
        }
    }

    // set the control mode to human or AI
    void Update()
    {
        if (currentControlMode == ControlMode.Human)
        {
            ManualControl();
        }
        else 
        {
            if (aiController == null){
                Debug.Log("AI isn't set yet");
            }
            Debug.Log("Updating AI");
            aiController.UpdateRover();
        }
    }

    public void SetControls(float throttle, float steer)
    {
        ApplyControlsToWheels(throttle, steer);
    }

    // apply the controls to the wheels
    void ApplyControlsToWheels(float throttleInput, float steerInput)
    {
        // calculate the current speed factor based on the forward speed and max speed
        float forwardSpeed = Vector3.Dot(transform.forward, rigidBody.linearVelocity);
        float targetSpeedFactor = Mathf.InverseLerp(0, CarParams.MAX_SPEED, Mathf.Abs(forwardSpeed));
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
        // get the input from the user
        float vInput = Input.GetAxis("Vertical");
        float hInput = Input.GetAxis("Horizontal");
        float forwardSpeed = Vector3.Dot(transform.forward, rigidBody.linearVelocity);
        float targetSpeedFactor = Mathf.InverseLerp(0, CarParams.MAX_SPEED, Mathf.Abs(forwardSpeed));
        currentSpeedFactor = Mathf.Lerp(currentSpeedFactor, targetSpeedFactor, Time.deltaTime / accelerationSmoothness);
        float torqueMultiplier = torqueCurve.Evaluate(currentSpeedFactor);
        float currentMotorTorque = motorTorque * torqueMultiplier;
        float currentSteerRange = Mathf.Lerp(steeringRange, steeringRangeAtMaxSpeed, currentSpeedFactor);
        bool isAccelerating = Mathf.Abs(vInput) > 0.1f;
        bool isReversing = forwardSpeed < -0.1f;
        bool isChangingDirection = (vInput > 0 && isReversing) || (vInput < 0 && forwardSpeed > 0.1f);
        
        // apply the controls to the wheels
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