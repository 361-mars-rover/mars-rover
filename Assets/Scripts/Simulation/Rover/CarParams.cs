using UnityEngine;

public static class CarParams{
    public static readonly float MOTOR_TORQUE = 2f;
    public static readonly float BRAKE_TORQUE = 200f;
    public static readonly float MAX_SPEED = 10f;
    public static readonly float STEERING_RANGE = 30f;
    public static readonly float STEERING_RANGE_AT_MAX_SPEED = 10f;
    public static readonly float CENTRE_OF_GRAVITY_OFFSET = 0f;
    public static readonly AnimationCurve TORQUE_CURVE = AnimationCurve.EaseInOut(0f, 1f, 1f, 0.2f);
    public static readonly float ACCELERATION_SMOOTHNESS = 0.3f;
    public static readonly float BRAKING_SMOOTHNESS = 0.5f;
    public static readonly float AVOIDANCE_THRESHOLD = 2f;
}