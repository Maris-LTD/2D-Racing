using UnityEngine;
using Game.Map;
using Game.Car;
using Game;

namespace Game.Car.Input
{
    public class AIPathFollowingBehavior : IAIBehaviorComponent, IUpdatable, IAIPathDataProvider
    {
        private CarController _carController;
        private ITrackData _cachedTrackData;
        
        private float _minLookAheadDistance = 5f;
        private float _maxLookAheadDistance = 15f;
        private float _steerStrength = 2f;
        private float _pathCorrectionStrength = 1f;
        private float _baseMaxSpeed = 30f;
        private float _sharpCurveThreshold = 0.35f;
        
        private float _curvatureUpdateInterval = 0.15f;
        private float _pathUpdateInterval = 0.1f;
        
        private float _cachedCurvature;
        private float _curvatureUpdateTimer;
        
        private float _cachedProgress;
        private Vector3 _cachedSplineCenter;
        private float _pathUpdateTimer;
        
        private Vector3 _cachedLookAheadPoint;
        private float _lookAheadUpdateTimer;
        private float _lookAheadUpdateInterval = 0.05f;
        
        private AIBehaviorMetrics _cachedMetrics;
        private float _cachedSteerInput;
        
        private const float CURVATURE_PROGRESS_STEP = 0.03f;
        private const float INV_CURVATURE_STEP = 1f / CURVATURE_PROGRESS_STEP;
        private const float LOOK_AHEAD_STEP_NORMAL = 0.015f;
        private const float LOOK_AHEAD_STEP_SHARP = 0.005f;
        private const float STEER_ANGLE_DIVISOR = 45f;

        public void Initialize(CarController car)
        {
            _carController = car;
            if (car?.MapProvider != null)
            {
                _cachedTrackData = car.MapProvider.GetTrackData();
            }
        }
        
        public void OnUpdate(float deltaTime)
        {
            if (_carController == null)
            {
                return;
            }

            if (_cachedTrackData == null)
            {
                if (_carController.MapProvider != null)
                {
                    _cachedTrackData = _carController.MapProvider.GetTrackData();
                }
                if (_cachedTrackData == null)
                {
                    return;
                }
            }
            
            Vector3 carPosition = _carController.transform.position;
            
            _pathUpdateTimer += deltaTime;
            
            if (_pathUpdateTimer >= _pathUpdateInterval)
            {
                _cachedProgress = _cachedTrackData.GetProgress(carPosition);
                _cachedSplineCenter = _cachedTrackData.GetNearestPoint(carPosition);
                _pathUpdateTimer = 0f;
            }
            
            _curvatureUpdateTimer += deltaTime;
            
            if (_curvatureUpdateTimer >= _curvatureUpdateInterval)
            {
                _cachedCurvature = CalculateTrackCurvature(_cachedTrackData, _cachedProgress);
                _curvatureUpdateTimer = 0f;
            }
        }
        
        public ITrackData GetTrackData() => _cachedTrackData;
        public float GetCachedProgress() => _cachedProgress;
        public Vector3 GetCachedSplineCenter() => _cachedSplineCenter;
        public float GetCachedCurvature() => _cachedCurvature;
        
        private float CalculateTrackCurvature(ITrackData trackData, float currentProgress)
        {
            float progressAhead = currentProgress + CURVATURE_PROGRESS_STEP;
            if (progressAhead > 1f) progressAhead -= 1f;
            
            Vector3 dir1 = trackData.GetDirectionAt(currentProgress);
            Vector3 dir2 = trackData.GetDirectionAt(progressAhead);
            
            return Vector3.Angle(dir1, dir2) * INV_CURVATURE_STEP;
        }

        public AIBehaviorMetrics Calculate(float deltaTime)
        {
            if (_carController == null || _cachedTrackData == null)
            {
                return AIBehaviorMetrics.Zero;
            }

            Transform transform = _carController.transform;
            Rigidbody rb = _carController.CarRigidbody;
            
            if (transform == null || rb == null)
            {
                return AIBehaviorMetrics.Zero;
            }

            Vector3 position = transform.position;
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;
            float currentSpeed = rb.linearVelocity.magnitude;

            _lookAheadUpdateTimer += deltaTime;
            
            if (_lookAheadUpdateTimer >= _lookAheadUpdateInterval)
            {
                float dynamicLookAhead = CalculateDynamicLookAhead(currentSpeed, _cachedCurvature);
                _cachedLookAheadPoint = GetLookAheadPoint(_cachedTrackData, position, 
                    _cachedProgress, dynamicLookAhead, _cachedCurvature);
                _lookAheadUpdateTimer = 0f;
            }

            _cachedSteerInput = CalculateSteeringInput(position, forward, 
                right, _cachedLookAheadPoint, _cachedSplineCenter);
            
            _cachedMetrics.SteerInput = _cachedSteerInput;
            _cachedMetrics.ThrottleWeight = 1f;
            _cachedMetrics.BrakeWeight = 0f;
            _cachedMetrics.Priority = 1f;

            return _cachedMetrics;
        }

        private float CalculateDynamicLookAhead(float currentSpeed, float curvature)
        {
            float speedFactor = Mathf.Clamp01(currentSpeed / _baseMaxSpeed);
            float curvatureFactor = Mathf.Clamp01(1f - (curvature / 10f));
            
            return Mathf.Lerp(_minLookAheadDistance, _maxLookAheadDistance, speedFactor * curvatureFactor);
        }

        private Vector3 GetLookAheadPoint(ITrackData trackData, Vector3 currentPosition, 
            float currentProgress, float lookAheadDist, float curvature)
        {
            float dynamicStep = curvature > _sharpCurveThreshold ? LOOK_AHEAD_STEP_SHARP : LOOK_AHEAD_STEP_NORMAL;
            
            float estimatedProgress = currentProgress + dynamicStep;
            if (estimatedProgress > 1f) estimatedProgress -= 1f;
            
            Vector3 direction = trackData.GetDirectionAt(estimatedProgress);
            Vector3 approximatePoint = currentPosition + direction * lookAheadDist;
            
            Vector3 targetPoint = trackData.GetNearestPoint(approximatePoint);
            
            Vector3 toTarget = targetPoint - currentPosition;
            
            if (Vector3.Dot(toTarget, direction) < 0f)
            {
                estimatedProgress = currentProgress + 0.002f;
                if (estimatedProgress > 1f) estimatedProgress -= 1f;
                
                direction = trackData.GetDirectionAt(estimatedProgress);
                approximatePoint = currentPosition + direction * (lookAheadDist * 0.3f);
                targetPoint = trackData.GetNearestPoint(approximatePoint);
            }
            
            return targetPoint;
        }

        private float CalculateSteeringInput(Vector3 carPosition, Vector3 carForward, 
            Vector3 carRight, Vector3 targetPoint, Vector3 splineCenter)
        {
            Vector3 toTarget = targetPoint - carPosition;
            float sqrDistance = toTarget.sqrMagnitude;
            
            if (sqrDistance < 0.01f)
            {
                return 0f;
            }
            
            float invDistance = 1f / Mathf.Sqrt(sqrDistance);
            Vector3 directionToTarget = toTarget * invDistance;
            
            float signedAngle = Vector3.SignedAngle(carForward, directionToTarget, Vector3.up);
            float steerInput = (signedAngle / STEER_ANGLE_DIVISOR) * _steerStrength;
            
            Vector3 offsetFromCenter = carPosition - splineCenter;
            float lateralOffset = Vector3.Dot(offsetFromCenter, carRight);
            
            float correctionSteer = -lateralOffset * _pathCorrectionStrength;
            correctionSteer = Mathf.Clamp(correctionSteer, -0.3f, 0.3f);
            
            return steerInput + correctionSteer;
        }
    }
}
