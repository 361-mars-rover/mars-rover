using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PhysicsManager : MonoBehaviour
{
    public GameObject car;
    

    public float springValue;
    public float damperValue;



    public void Start()
    {
        ChangeCarSettings();
    }

    public void ChangeCarSettings(){
        WheelCollider[] wheelColliders = FindObjectsOfType<WheelCollider>();

        foreach (WheelCollider wheel in wheelColliders)
        {
            //             // Get the current suspension spring
            // JointSpring spring = wheelCollider.suspensionSpring;
            
            // // Modify the spring and damper values
            // spring.spring = springValue;  // Set your desired spring value
            // spring.damper = damperValue;   // Set your desired damper value
            
            // // Apply the modified suspension spring back to the wheel collider
            // wheelCollider.suspensionSpring = spring;
            
            // // Log the changes
            // Debug.Log($"Updated Wheel: {wheelCollider.gameObject.name} - Spring: {spring.spring}, Damper: {spring.damper}");
                // Decrease forward friction stiffness to reduce "grippiness"
            WheelFrictionCurve forwardFriction = wheel.forwardFriction;
            forwardFriction.stiffness = 0.7f; // Lower value (default is typically 1.0)
            wheel.forwardFriction = forwardFriction;
            
            // Decrease sideways friction to prevent tight turns
            WheelFrictionCurve sidewaysFriction = wheel.sidewaysFriction;
            sidewaysFriction.stiffness = 0.7f;
            wheel.sidewaysFriction = sidewaysFriction;
            
            // Adjust suspension for smoother rides
            JointSpring spring = wheel.suspensionSpring;
            spring.spring = 15000f;
            spring.damper = 2000f;
            wheel.suspensionSpring = spring;
        }

        Rigidbody carRigidbody = car.GetComponent<Rigidbody>();
        if (carRigidbody != null)
        {
            // Lower the center of mass to improve stability
            carRigidbody.centerOfMass = new Vector3(0, -0.5f, 0);
            
            // Increase mass to make it more stable
            carRigidbody.mass *= 1.5f;
        }

        CarControl carControl = GetComponentInParent<CarControl>();
    }
}


