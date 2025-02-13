using System.Collections;
using UnityEngine;

public class CarControl : MonoBehaviour
{
    public float motorTorque = 10000;
    public float brakeTorque = 2000;
    public float maxSpeed = 50;
    public float steeringRange = 30;
    public float steeringRangeAtMaxSpeed = 10;
    public float centreOfGravityOffset = -1f;

    WheelControl[] wheels;
    Rigidbody rigidBody;

    public Terrain marsTerrain;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(WaitForTerrainAndSpawn());
    }

    IEnumerator WaitForTerrainAndSpawn()
    {
        // Wait until the terrain is loaded
        while (marsTerrain.terrainData == null)
        {
            // marsTerrain = Terrain.activeTerrain; // Try to find the terrain
            yield return null; // Wait for the next frame
        }
        Debug.Log("terrain loaded!");

        rigidBody = GetComponent<Rigidbody>();
        // Adjust center of mass vertically, to help prevent the car from rolling
        rigidBody.centerOfMass += Vector3.up * centreOfGravityOffset;
        // Find all child GameObjects that have the WheelControl script attached
        wheels = GetComponentsInChildren<WheelControl>();

        // transform.position = new Vector3(27421, 5600, 26325);
        float terrainHeight = marsTerrain.SampleHeight(new Vector3(27421, 0, 26325));
        Vector3 newPosition = new Vector3(27421, terrainHeight + 1f, 26325);
        transform.position = newPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (marsTerrain == null) return;

        float vInput = Input.GetAxis("Vertical");
        float hInput = Input.GetAxis("Horizontal");

        // Calculate current speed in relation to the forward direction of the car
        // (this returns a negative number when traveling backwards)
        float forwardSpeed = Vector3.Dot(transform.forward, rigidBody.linearVelocity);


        // Calculate how close the car is to top speed
        // as a number from zero to one
        float speedFactor = Mathf.InverseLerp(0, maxSpeed, forwardSpeed);

        // Use that to calculate how much torque is available 
        // (zero torque at top speed)
        float currentMotorTorque = Mathf.Lerp(motorTorque, 0, speedFactor);

        // …and to calculate how much to steer 
        // (the car steers more gently at top speed)
        float currentSteerRange = Mathf.Lerp(steeringRange, steeringRangeAtMaxSpeed, speedFactor);

        // Check whether the user input is in the same direction 
        // as the car's velocity
        bool isAccelerating = Mathf.Sign(vInput) == Mathf.Sign(forwardSpeed);

        foreach (var wheel in wheels)
        {
            // Apply steering to Wheel colliders that have "Steerable" enabled
            if (wheel.steerable)
            {
                wheel.WheelCollider.steerAngle = hInput * currentSteerRange;
            }
            
            if (isAccelerating)
            {
                // Apply torque to Wheel colliders that have "Motorized" enabled
                if (wheel.motorized)
                {
                    wheel.WheelCollider.motorTorque = vInput * currentMotorTorque;
                }
                wheel.WheelCollider.brakeTorque = 0;
            }
            else
            {
                // If the user is trying to go in the opposite direction
                // apply brakes to all wheels
                wheel.WheelCollider.brakeTorque = Mathf.Abs(vInput) * brakeTorque;
                wheel.WheelCollider.motorTorque = 0;
            }
        }
    }
}