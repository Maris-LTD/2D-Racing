using UnityEngine;
using Game.Car;

namespace Game.Car.Input
{
    public class AISpeedControlBehavior : IAIBehaviorComponent
    {
        private CarController _carController;
        private IAIPathDataProvider _pathDataProvider;
        private Transform _cachedTransform;
        private Rigidbody _cachedRigidbody;
        
        private float _baseMaxSpeed = 30f;
        private float _minSpeed = 5f;
        private float _curvatureThreshold = 0.15f;
        private float _sharpCurveThreshold = 0.35f;
        private float _maxDistanceFromCenter = 5f;
        private float _speedReductionFactor = 0.2f;
        private float _speedSmoothingFactor = 0.1f;
        private float _brakeThreshold = 2f;
        
        private float _cachedTargetSpeed;
        private float _smoothedTargetSpeed;
        private float _speedUpdateTimer;
        private float _speedUpdateInterval = 0.1f;
        private AIBehaviorMetrics _cachedMetrics;
        
        private const float SHARP_CURVE_SPEED_MULTIPLIER = 0.3f;
        private const float NORMAL_CURVE_SPEED_MULTIPLIER = 0.7f;
        private const float BRAKE_INTENSITY_DIVISOR = 10f;

        public void Initialize(CarController car)
        {
            _carController = car;
            if (car != null)
            {
                _cachedTransform = car.transform;
                _cachedRigidbody = car.CarRigidbody;
            }
            _cachedTargetSpeed = _baseMaxSpeed;
            _smoothedTargetSpeed = _baseMaxSpeed;
        }
        
        public void SetPathDataProvider(IAIPathDataProvider provider)
        {
            _pathDataProvider = provider;
        }

        public AIBehaviorMetrics Calculate(float deltaTime)
        {
            if (_carController == null || _pathDataProvider == null || _cachedTransform == null || _cachedRigidbody == null)
            {
                return AIBehaviorMetrics.Zero;
            }

            Vector3 position = _cachedTransform.position;
            float currentSpeed = _cachedRigidbody.linearVelocity.magnitude;

            _speedUpdateTimer += deltaTime;
            
            if (_speedUpdateTimer >= _speedUpdateInterval)
            {
                float curvature = _pathDataProvider.GetCachedCurvature();
                Vector3 splineCenter = _pathDataProvider.GetCachedSplineCenter();
                _cachedTargetSpeed = CalculateTargetSpeed(curvature, position, splineCenter);
                _cachedTargetSpeed = Mathf.Max(_cachedTargetSpeed, _minSpeed);
                _speedUpdateTimer = 0f;
            }
            
            _smoothedTargetSpeed = Mathf.Lerp(_smoothedTargetSpeed, _cachedTargetSpeed, _speedSmoothingFactor);

            float speedDifference = currentSpeed - _smoothedTargetSpeed;
            bool shouldBrake = speedDifference > _brakeThreshold;
            float brakingIntensity = shouldBrake ? Mathf.Clamp01(speedDifference / BRAKE_INTENSITY_DIVISOR) : 0f;

            _cachedMetrics.SteerInput = 0f;
            _cachedMetrics.ThrottleWeight = shouldBrake ? 0f : 1f;
            _cachedMetrics.BrakeWeight = brakingIntensity;
            _cachedMetrics.Priority = 1f;

            return _cachedMetrics;
        }

        private float CalculateTargetSpeed(float curvature, Vector3 carPosition, Vector3 splineCenter)
        {
            float baseSpeed = _baseMaxSpeed;
            
            if (curvature > _sharpCurveThreshold)
            {
                baseSpeed = _baseMaxSpeed * SHARP_CURVE_SPEED_MULTIPLIER;
            }
            else if (curvature > _curvatureThreshold)
            {
                float curvatureFactor = (curvature - _curvatureThreshold) / 
                    (_sharpCurveThreshold - _curvatureThreshold);
                baseSpeed = Mathf.Lerp(_baseMaxSpeed * NORMAL_CURVE_SPEED_MULTIPLIER, 
                    _baseMaxSpeed * SHARP_CURVE_SPEED_MULTIPLIER, curvatureFactor);
            }
            
            Vector3 offsetFromCenter = carPosition - splineCenter;
            float sqrDistanceFromCenter = offsetFromCenter.sqrMagnitude;
            float maxSqrDistance = _maxDistanceFromCenter * _maxDistanceFromCenter;
            
            if (sqrDistanceFromCenter > maxSqrDistance)
            {
                baseSpeed *= _speedReductionFactor;
            }
            
            return baseSpeed;
        }
    }
}
