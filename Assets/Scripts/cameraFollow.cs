using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public KeyCode switchKey = KeyCode.C;

    public float sizeScale = 0.001f;

    private Vector3 followOffset = new Vector3(0, 3, -8) * 0.1f;
    private float followPosSmooth = 0.5f;
    private float followRotSmooth = 2f;
    private float lookAheadDistance = 10f;

    private Vector3 orbitOffset = new Vector3(0, 2, -10);
    private float orbitRotationSpeed = 200f;
    private float orbitPosSmooth = 0.5f;
    private float orbitRotSmooth = 2f;
    [Range(5f, 45f)] private float minPitch = 10f, maxPitch = 35f;

    private enum CameraMode { FollowBehind, FreeOrbit }
    private CameraMode currentMode = CameraMode.FollowBehind;

    private float yaw;
    private float pitch;
    private Vector3 followVelocity;
    private Vector3 orbitVelocity;
    private Quaternion targetRotation;

    void LateUpdate()
    {
        if (target == null) return;

        HandleModeSwitch();
        UpdateCamera();
    }

    void HandleModeSwitch()
    {
        if (Input.GetKeyDown(switchKey))
        {
            currentMode = currentMode == CameraMode.FollowBehind 
                        ? CameraMode.FreeOrbit : CameraMode.FollowBehind;
        }
    }

    void UpdateCamera()
    {
        switch (currentMode)
        {
            case CameraMode.FollowBehind:
                FollowBehindMode();
                break;
            case CameraMode.FreeOrbit:
                FreeOrbitMode();
                break;
        }
    }

    void FollowBehindMode()
    {
        // Position
        Vector3 targetPosition = target.position + 
                               target.forward * followOffset.z + 
                               target.up * followOffset.y;

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, 
                                               ref followVelocity, followPosSmooth);

        // Rotation
        Vector3 lookDirection = target.forward * lookAheadDistance;
        targetRotation = Quaternion.LookRotation((target.position + lookDirection) - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 
                                            followRotSmooth * Time.deltaTime);
    }

    void FreeOrbitMode()
    {
        // Rotation Input
        yaw += Input.GetAxis("Mouse X") * orbitRotationSpeed * Time.deltaTime;
        pitch -= Input.GetAxis("Mouse Y") * orbitRotationSpeed * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // Position
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 desiredPosition = target.position + rotation * orbitOffset;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, 
                                              ref orbitVelocity, orbitPosSmooth);

        // Rotation
        targetRotation = Quaternion.LookRotation(target.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 
                                            orbitRotSmooth * Time.deltaTime);
    }
}