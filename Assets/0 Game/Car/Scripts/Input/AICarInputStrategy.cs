using UnityEngine;
using Game.Map;

namespace Game.Car.Input
{
    public class AICarInputStrategy : ICarInputStrategy
    {
        private readonly CarController _carController;
        private float _throttleDuration;
        private float _brakeDuration;
        private float _currentSteerInput;
        private float _targetSteerInput;

        private float _baseMaxSpeed = 30f;
        private float _minLookAheadDistance = 5f;
        private float _maxLookAheadDistance = 15f;
        private float _steerSmoothing = 12f;
        private float _steerStrength = 2f;
        private float _pathCorrectionStrength = 3f;
        
        private float _obstacleDetectionDistance = 15f;
        
        private float _curvatureThreshold = 0.15f;
        private float _sharpCurveThreshold = 0.35f;
        private float _recoveryTimer;
        private float _stuckTimer;
        private Vector3 _lastPosition;
        
        private float _cachedCurvature;
        private float _curvatureUpdateTimer;
        private float _curvatureUpdateInterval = 0.15f;
        
        private Vector3 _cachedObstacleAvoidance;
        private float _obstacleCheckTimer;
        private float _obstacleCheckInterval = 0.2f;
        
        private float _cachedProgress;
        private Vector3 _cachedSplineCenter;
        private float _pathUpdateTimer;
        private float _pathUpdateInterval = 0.1f;

        public AICarInputStrategy(CarController carController)
        {
            _carController = carController;
            _lastPosition = carController.transform.position;
        }

        public float GetSteerInput()
        {
            return _currentSteerInput;
        }

        public float GetThrottleDuration()
        {
            return _throttleDuration;
        }

        public float GetBrakeDuration()
        {
            return _brakeDuration;
        }

        public void UpdateInput(float deltaTime)
        {
            if (_carController == null || _carController.MapProvider == null)
            {
                _currentSteerInput = 0f;
                _throttleDuration = 0f;
                _brakeDuration = 0f;
                return;
            }

            ITrackData trackData = _carController.MapProvider.GetTrackData();
            if (trackData == null)
            {
                return;
            }

            Vector3 carPosition = _carController.transform.position;
            Vector3 carForward = _carController.transform.forward;
            Vector3 carRight = _carController.transform.right;
            Rigidbody carRb = _carController.CarRigidbody;
            
            float currentSpeed = carRb.linearVelocity.magnitude;
            
            _pathUpdateTimer += deltaTime;
            if (_pathUpdateTimer >= _pathUpdateInterval)
            {
                _cachedProgress = trackData.GetProgress(carPosition);
                _cachedSplineCenter = trackData.GetNearestPoint(carPosition);
                _pathUpdateTimer = 0f;
            }
            
            _curvatureUpdateTimer += deltaTime;
            if (_curvatureUpdateTimer >= _curvatureUpdateInterval)
            {
                _cachedCurvature = CalculateTrackCurvature(trackData, _cachedProgress);
                _curvatureUpdateTimer = 0f;
            }
            
            _obstacleCheckTimer += deltaTime;
            if (_obstacleCheckTimer >= _obstacleCheckInterval)
            {
                _cachedObstacleAvoidance = DetectAndAvoidObstacles(carPosition, carForward);
                _obstacleCheckTimer = 0f;
            }
            
            float dynamicLookAhead = CalculateDynamicLookAhead(currentSpeed, _cachedCurvature);
            Vector3 targetPoint = GetLookAheadPoint(trackData, carPosition, _cachedProgress, dynamicLookAhead);
            
            float steerInput = CalculateSteeringInput(carPosition, carForward, carRight, targetPoint);
            steerInput += _cachedObstacleAvoidance.x;
            
            _targetSteerInput = Mathf.Clamp(steerInput, -1f, 1f);
            
            float dynamicSmoothing = _cachedCurvature > _sharpCurveThreshold ? _steerSmoothing * 1.5f : _steerSmoothing;
            _currentSteerInput = Mathf.Lerp(_currentSteerInput, _targetSteerInput, dynamicSmoothing * deltaTime);
            
            float targetSpeed = CalculateTargetSpeed(_cachedCurvature, carPosition, _cachedSplineCenter);
            bool shouldBrake = currentSpeed > targetSpeed;
            
            HandleStuckRecovery(carPosition, currentSpeed, deltaTime);
            
            if (_recoveryTimer > 0f)
            {
                _recoveryTimer -= deltaTime;
                _brakeDuration += deltaTime;
                _throttleDuration = 0f;
            }
            else if (shouldBrake)
            {
                float brakingIntensity = Mathf.Clamp01((currentSpeed - targetSpeed) / 10f);
                _brakeDuration += deltaTime * brakingIntensity;
                _throttleDuration = 0f;
            }
            else
            {
                _throttleDuration += deltaTime;
                _brakeDuration = 0f;
            }
        }

        private float CalculateTrackCurvature(ITrackData trackData, float currentProgress)
        {
            float progressAhead = currentProgress + 0.03f;
            if (progressAhead > 1f) progressAhead -= 1f;
            
            Vector3 dir1 = trackData.GetDirectionAt(currentProgress);
            Vector3 dir2 = trackData.GetDirectionAt(progressAhead);
            
            return Vector3.Angle(dir1, dir2) / 0.03f;
        }

        private float CalculateDynamicLookAhead(float currentSpeed, float curvature)
        {
            float speedFactor = Mathf.Clamp01(currentSpeed / _baseMaxSpeed);
            float curvatureFactor = Mathf.Clamp01(1f - (curvature / 10f));
            
            return Mathf.Lerp(_minLookAheadDistance, _maxLookAheadDistance, speedFactor * curvatureFactor);
        }

        private Vector3 GetLookAheadPoint(ITrackData trackData, Vector3 currentPosition, float currentProgress, float lookAheadDist)
        {
            float dynamicStep = _cachedCurvature > _sharpCurveThreshold ? 0.005f : 0.015f;
            
            float estimatedProgress = currentProgress + dynamicStep;
            if (estimatedProgress > 1f) estimatedProgress -= 1f;
            
            Vector3 direction = trackData.GetDirectionAt(estimatedProgress);
            Vector3 approximatePoint = currentPosition + direction * lookAheadDist;
            
            Vector3 targetPoint = trackData.GetNearestPoint(approximatePoint);
            
            Vector3 toTarget = targetPoint - currentPosition;
            Vector3 carForward = _carController.transform.forward;
            
            if (Vector3.Dot(toTarget, carForward) < 0)
            {
                estimatedProgress = currentProgress + 0.002f;
                if (estimatedProgress > 1f) estimatedProgress -= 1f;
                
                direction = trackData.GetDirectionAt(estimatedProgress);
                approximatePoint = currentPosition + direction * (lookAheadDist * 0.3f);
                targetPoint = trackData.GetNearestPoint(approximatePoint);
            }
            
            return targetPoint;
        }

        private float CalculateSteeringInput(Vector3 carPosition, Vector3 carForward, Vector3 carRight, Vector3 targetPoint)
        {
            Vector3 directionToTarget = (targetPoint - carPosition).normalized;
            
            float signedAngle = Vector3.SignedAngle(carForward, directionToTarget, Vector3.up);
            float steerInput = (signedAngle / 45f) * _steerStrength;
            
            Vector3 offsetFromCenter = carPosition - _cachedSplineCenter;
            float lateralOffset = Vector3.Dot(offsetFromCenter, carRight);
            
            float correctionSteer = -lateralOffset * _pathCorrectionStrength;
            
            return steerInput + correctionSteer;
        }

        private Vector3 DetectAndAvoidObstacles(Vector3 carPosition, Vector3 carForward)
        {
            Vector3 avoidanceVector = Vector3.zero;

            if (Physics.Raycast(carPosition, carForward, out var hit, _obstacleDetectionDistance))
            {
                if (hit.collider.GetComponentInParent<CarController>() != null)
                {
                    avoidanceVector.x = 0.5f;
                }
            }
            
            return avoidanceVector;
        }

        private float CalculateTargetSpeed(float curvature, Vector3 carPosition, Vector3 splineCenter)
        {
            float baseSpeed = _baseMaxSpeed;
            
            if (curvature > _sharpCurveThreshold)
            {
                baseSpeed = _baseMaxSpeed * 0.3f;
            }
            else if (curvature > _curvatureThreshold)
            {
                float curvatureFactor = (curvature - _curvatureThreshold) / (_sharpCurveThreshold - _curvatureThreshold);
                baseSpeed = Mathf.Lerp(_baseMaxSpeed * 0.7f, _baseMaxSpeed * 0.3f, curvatureFactor);
            }
            
            float distanceFromCenter = Vector3.Distance(carPosition, splineCenter);
            if (distanceFromCenter > 5f)
            {
                baseSpeed *= 0.2f;
            }
            
            return baseSpeed;
        }

        private void HandleStuckRecovery(Vector3 currentPosition, float currentSpeed, float deltaTime)
        {
            float distanceMoved = Vector3.Distance(currentPosition, _lastPosition);
            
            if (currentSpeed < 1f && distanceMoved < 0.1f)
            {
                _stuckTimer += deltaTime;
                
                if (_stuckTimer > 2f)
                {
                    _recoveryTimer = 1.5f;
                    _stuckTimer = 0f;
                }
            }
            else
            {
                _stuckTimer = 0f;
            }
            
            _lastPosition = currentPosition;
        }
    }
}

