using System.Collections;
using UnityEngine;

/*
JIKAEL + DOV + SAL
Defines rock avoidance behaviour. This is inherited by all AI classes.
*/

public abstract class BaseAIController : MonoBehaviour, IAIController
{
    protected Transform carTransform;
    protected float currentAngle;
    protected float darknessTolerance = 2.0f;
    protected float adjustmentAngle = 45f;
    protected float forwardDistance = 20f;
    protected float avoidanceThreshold = 2f;
    protected bool avoidingRock = false;
    protected Vector3 avoidanceTarget = Vector3.zero;
    protected float lastRockDistance = Mathf.Infinity;
    protected bool inReverseManeuver = false;
    protected bool inRecovery = false;
    protected IAIInput aiInput;
    
    public abstract void UpdateRover();

    public Vector3 CheckForRockAvoidance()
    {
        float detectionDistance = 30f;
        float sphereRadius = 2f;
        Vector3 origin = carTransform.position;
        Vector3 forwardDir = carTransform.forward;
        int rockLayerMask = 1 << LayerMask.NameToLayer("Rock");

        RaycastHit hit;
        if (Physics.SphereCast(origin, sphereRadius, forwardDir, out hit, detectionDistance, rockLayerMask))
        {
            lastRockDistance = hit.distance;
            Debug.Log($"Rock detected via SphereCast: {hit.collider.name}, distance: {hit.distance}");
            Vector3 rockPosition = hit.collider.transform.position;
            Vector3 toRock = rockPosition - carTransform.position;
            toRock.y = 0f;
            float angleToRock = Vector3.Angle(carTransform.forward, toRock);
            if (angleToRock > 25f)
            {
                return Vector3.zero;
            }
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

    protected IEnumerator ReverseManeuver()
    {
        inReverseManeuver = true;
        float reverseDuration = 2f;
        float timer = 0f;
        while (timer < reverseDuration)
        {
            float reverseThrottle = -0.5f;
            aiInput.SetControls(reverseThrottle, 0f);
            timer += Time.deltaTime;
            yield return null;
        }

        lastRockDistance = 30f; // Reset detection distance after reverse
        inReverseManeuver = false;
    }

    protected void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Rock"))
        {
            if (!inRecovery)
            {
                StartCoroutine(RecoveryManeuver());
            }
        }
    }

    protected IEnumerator RecoveryManeuver()
    {
        inRecovery = true;
        float firstRecoveryTime = 3f;
        float timer = 0f;
        while (timer < firstRecoveryTime)
        {
            float reverseThrottle = -2.5f;
            aiInput.SetControls(reverseThrottle, 0f);
            timer += Time.deltaTime;
            yield return null;
        }

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
            float secondRecoveryTime = 4f;
            float secondTimer = 0f;
            while (secondTimer < secondRecoveryTime)
            {
                aiInput.SetControls(-0.5f, -0.2f);
                secondTimer += Time.deltaTime;
                yield return null;
            }
        }

        inRecovery = false;
    }

    protected void HandleRockProximity(float throttleAmount, float steer)
    {
        if (lastRockDistance < 5f)
        {
            if (!inReverseManeuver)
            {
                Debug.Log("Rock extremely close. Initiating reverse maneuver.");
                StartCoroutine(ReverseManeuver());
            }
            return ;
        }
        else if (lastRockDistance < 15f)
        {
            // Reduce throttle by half if rock is at moderate range
            Debug.Log("Rock detected at moderate range. Reducing throttle.");
            throttleAmount *= 0.5f; // Adjust this as needed
        }
        aiInput.SetControls(throttleAmount, steer);
    }

    protected void SetControls(float throttleAmount, float steer)
    {
        aiInput.SetControls(throttleAmount, steer);
    }
}
