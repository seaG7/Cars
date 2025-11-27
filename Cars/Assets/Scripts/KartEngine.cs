using System;
using UnityEngine;

/// <summary>
/// KartEngine.
/// Этап 1: простейший двигатель с постоянным моментом.
/// </summary>
public class KartEngine : MonoBehaviour
{
    [Header("Basic engine")]
    [SerializeField] private float _maxTorque = 400f; // Н*м при полном газе
    
    [Header("RPM settings")]
    [SerializeField] private float _idleRpm = 1000f;
    [SerializeField] private float _maxRpm = 8000f;

    [Header("Torque curve")]
    [SerializeField] private AnimationCurve _torqueCurve;

    [Header("Inertia & response")]
    [Tooltip("Момент инерции маховика J, кг*м^2.")]
    [SerializeField] private float _flywheelInertia = 0.2f;

    [Tooltip("Скорость отклика газа (1/с).")]
    [SerializeField] private float _throttleResponse = 5f;

    [Header("Losses & load")]
    [Tooltip("Внутренние потери, Н*м/ rpm.")]
    [SerializeField] private float _engineFrictionCoeff = 0.02f;

    [Tooltip("Нагрузка от машины, Н*м / (м/с).")]
    [SerializeField] private float _loadTorqueCoeff = 5f;

    public float CurrentRpm { get; private set; }
    public float CurrentTorque { get; private set; }
    public float SmoothedThrottle { get; private set; }

    private float _invInertiaFactor;

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        CurrentRpm = _idleRpm;
        _invInertiaFactor = 60f / (2f * Mathf.PI * Mathf.Max(_flywheelInertia, 0.0001f));
    }

    /// <summary>
    /// Возвращает момент на коленвале в зависимости от газа.
    /// throttleInput ожидается в диапазоне [-1; 1]. Используем только газ [0; 1].
    /// </summary>
    public float Simulate(float throttleInput, float deltaTime)
    {
        float throttle = Mathf.Clamp01(throttleInput);
        CurrentTorque = _maxTorque * throttle;
        return CurrentTorque;
    }
    
    /// <summary>
    /// throttleInput [-1;1], forwardSpeed (м/с), gearRatio, wheelRadius.
    /// </summary>
    public float Simulate(float throttleInput, float forwardSpeed, float gearRatio, float wheelRadius, float deltaTime)
    {
        float throttle = Mathf.Clamp01(throttleInput);

        // Оценка rpm по скорости (упрощённо)
        float wheelOmega = forwardSpeed / Mathf.Max(wheelRadius, 0.0001f); // рад/с
        float wheelRpm = wheelOmega * 60f / (2f * Mathf.PI);
        float kinematicRpm = Mathf.Abs(wheelRpm * gearRatio);

        // Плавно подтягиваем обороты к кинематическим + холостой ход
        CurrentRpm = Mathf.Lerp(CurrentRpm, Mathf.Max(kinematicRpm, _idleRpm), 5f * deltaTime);
        CurrentRpm = Mathf.Clamp(CurrentRpm, _idleRpm, _maxRpm);

        float maxTorqueAtRpm = _torqueCurve.Evaluate(CurrentRpm);
        CurrentTorque = maxTorqueAtRpm * throttle;

        return CurrentTorque;
    }
    
    public float Simulate(float throttleInput, float forwardSpeed, float deltaTime)
    {
        // сглаживаем газ
        float targetThrottle = Mathf.Clamp01(throttleInput);
        SmoothedThrottle = Mathf.MoveTowards(SmoothedThrottle, targetThrottle, _throttleResponse * deltaTime);

        float maxTorqueAtRpm = _torqueCurve.Evaluate(CurrentRpm);
        float driveTorque = maxTorqueAtRpm * SmoothedThrottle;

        float frictionTorque = _engineFrictionCoeff * CurrentRpm;
        float loadTorque = _loadTorqueCoeff * Mathf.Abs(forwardSpeed);

        float netTorque = driveTorque - frictionTorque - loadTorque;

        float rpmDot = netTorque * _invInertiaFactor;
        CurrentRpm += rpmDot * deltaTime;

        if (CurrentRpm < _idleRpm) CurrentRpm = _idleRpm;
        if (CurrentRpm > _maxRpm)  CurrentRpm = _maxRpm;

        CurrentTorque = driveTorque;
        return CurrentTorque;
    }
}