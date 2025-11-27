using UnityEngine;

[CreateAssetMenu(fileName = "NewKartConfig", menuName = "Kart/KartConfig")]
public class KartConfig : ScriptableObject
{
    [Header("General")]
    public float mass = 1000f;
    public float centerOfMassY = -0.2f; // Смещение центра масс вниз для устойчивости

    [Header("Wheels & Grip")]
    public float frontAxleShare = 0.5f;
    public float wheelRadius = 0.3f;
    public float rollingResistance = 0.5f;
    public float frictionCoefficient = 1.0f; // Limit (mu)
    public float frontLateralStiffness = 80f; // C_alpha front
    public float rearLateralStiffness = 80f;  // C_alpha rear

    [Header("Steering")]
    public float maxSteerAngle = 30f;

    [Header("Engine & Drivetrain")]
    public float gearRatio = 8f;
    public float drivetrainEfficiency = 0.9f;
    public float maxSpeed = 30f; // м/с

    [Header("Engine Physics")]
    public float engineInertia = 0.2f;
    public float idleRpm = 1000f;
    public float maxRpm = 8000f;
    public float revLimiterRpm = 7500f;
    public AnimationCurve torqueCurve;
}