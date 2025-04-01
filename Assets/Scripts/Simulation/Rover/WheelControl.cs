using UnityEngine;

public class WheelControl : MonoBehaviour
{
    public Transform wheelModel;
    [HideInInspector] public WheelCollider WheelCollider;
    
    public bool steerable;
    public bool motorized;
    
    public float motorTorque = 2000f;
    public float steerAngle = 50f;
    public float brakeTorque = 3000f;
    public float rollingResistance = 5f;
    public float dragCoefficient = 0.3f;
    
    private Vector3 position;
    private Quaternion rotation;
    private float currentMotorTorque;
    
    private void Start()
    {
        WheelCollider = GetComponent<WheelCollider>();
        // set up wheel friction curves for better grip
        WheelFrictionCurve fwdFriction = WheelCollider.forwardFriction;
        fwdFriction.extremumSlip = 0.4f;
        fwdFriction.extremumValue = 1f;
        fwdFriction.asymptoteSlip = 0.8f;
        fwdFriction.asymptoteValue = 0.5f;
        fwdFriction.stiffness = 1f;
        WheelCollider.forwardFriction = fwdFriction;
    }
    
    void Update()
    {
        WheelCollider.GetWorldPose(out position, out rotation);
        wheelModel.transform.position = position;
        wheelModel.transform.rotation = rotation;
        
        ApplyRollingResistance();
    }
    
    // added steering input
    public void HandleSteering(float steerInput)
    {
        if (steerable)
        {
            WheelCollider.steerAngle = steerInput;
        }
    }
    
    public void HandleMotor(float throttleInput, float availableTorque)
    {
        if (motorized)
        {
            // apply motor torque based on throttle input
            float targetTorque = throttleInput * availableTorque;
            currentMotorTorque = Mathf.Lerp(currentMotorTorque, targetTorque, Time.deltaTime * 3f);
            WheelCollider.motorTorque = currentMotorTorque;
        }
    }
    
    public void HandleBraking(bool isBraking)
    {
        // progressive braking based on current speed
        if (isBraking)
        {
            float speed = transform.parent.GetComponent<Rigidbody>().linearVelocity.magnitude;
            float brakeFactor = Mathf.Lerp(0.2f, 1f, speed / 10f); // stronger braking at higher speeds
            WheelCollider.brakeTorque = brakeTorque * brakeFactor;
        }
        else
        {
            WheelCollider.brakeTorque = 0f;
        }
    }
    
    // added rolling resistance and air drag
    private void ApplyRollingResistance()
    {
        if (WheelCollider.isGrounded)
        {
            Rigidbody rb = transform.parent.GetComponent<Rigidbody>();
            float speed = rb.linearVelocity.magnitude;
            
            float resistance = rollingResistance * speed; // apply rolling resistance (proportional to velocity)
            float drag = dragCoefficient * speed * speed; // apply air resistance (proportional to velocity squared)

            
            // Convert to deceleration force
            Vector3 resistanceForce = -rb.linearVelocity.normalized * (resistance + drag);
            rb.AddForceAtPosition(resistanceForce, transform.position);
        }
    }
}