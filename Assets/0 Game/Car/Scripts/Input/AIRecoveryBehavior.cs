using UnityEngine;
using Game.Car;

namespace Game.Car.Input
{
    public class AIRecoveryBehavior : IAIBehaviorComponent
    {
        private CarController _carController;
        private Transform _cachedTransform;
        private Rigidbody _cachedRigidbody;
        
        private float _stuckSpeedThreshold = 0.5f;
        private float _stuckDistanceThreshold = 0.05f;
        private float _stuckTimeThreshold = 3f;
        private float _recoveryDuration = 0.5f;
        
        private float _recoveryTimer;
        private float _stuckTimer;
        private Vector3 _lastPosition;
        private bool _isInitialized;
        private AIBehaviorMetrics _cachedMetrics;
        private AIBehaviorMetrics _zeroMetrics;
        private AIBehaviorMetrics _recoveryMetrics;
        
        private float _sqrStuckThreshold;

        public void Initialize(CarController car)
        {
            _carController = car;
            if (car != null)
            {
                _cachedTransform = car.transform;
                _cachedRigidbody = car.CarRigidbody;
                _lastPosition = _cachedTransform.position;
                _isInitialized = true;
            }
            _sqrStuckThreshold = _stuckDistanceThreshold * _stuckDistanceThreshold;
            _zeroMetrics = AIBehaviorMetrics.Zero;
            _recoveryMetrics = new AIBehaviorMetrics
            {
                SteerInput = 0f,
                ThrottleWeight = 0f,
                BrakeWeight = 1f,
                Priority = 3f
            };
            _cachedMetrics = _zeroMetrics;
        }

        public AIBehaviorMetrics Calculate(float deltaTime)
        {
            if (!_isInitialized || _cachedTransform == null || _cachedRigidbody == null)
            {
                return _zeroMetrics;
            }

            Vector3 position = _cachedTransform.position;
            float currentSpeed = _cachedRigidbody.linearVelocity.magnitude;

            HandleStuckRecovery(position, currentSpeed, deltaTime);

            if (_recoveryTimer > 0f)
            {
                _recoveryTimer -= deltaTime;
                _cachedMetrics = _recoveryMetrics;
            }
            else
            {
                _cachedMetrics = _zeroMetrics;
            }

            return _cachedMetrics;
        }

        private void HandleStuckRecovery(Vector3 currentPosition, float currentSpeed, float deltaTime)
        {
            Vector3 positionDelta = currentPosition - _lastPosition;
            float sqrDistanceMoved = positionDelta.sqrMagnitude;
            
            if (currentSpeed < _stuckSpeedThreshold && sqrDistanceMoved < _sqrStuckThreshold)
            {
                _stuckTimer += deltaTime;
                
                if (_stuckTimer > _stuckTimeThreshold)
                {
                    _recoveryTimer = _recoveryDuration;
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
