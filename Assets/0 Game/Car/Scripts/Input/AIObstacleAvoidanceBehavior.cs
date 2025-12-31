using UnityEngine;
using Game.Car;

namespace Game.Car.Input
{
    public class AIObstacleAvoidanceBehavior : IAIBehaviorComponent
    {
        private ICarController _carController;
        
        private float _obstacleDetectionDistance = 2f;
        private float _avoidanceStrength = 0.5f;
        
        private bool _cachedObstacleDetected;
        private float _obstacleCheckTimer;
        private float _obstacleCheckInterval = 0.2f;
        
        private int _obstacleLayerMask = -1;
        private float _obstacleHeightOffset = 0.5f;
        private AIBehaviorMetrics _cachedMetrics;
        private AIBehaviorMetrics _zeroMetrics;

        public void Initialize(ICarController car)
        {
            _carController = car;
            _obstacleLayerMask = Physics.DefaultRaycastLayers;
            _zeroMetrics = AIBehaviorMetrics.Zero;
            _cachedMetrics = _zeroMetrics;
        }

        public AIBehaviorMetrics Calculate(float deltaTime)
        {
            if (_carController == null)
            {
                return _zeroMetrics;
            }

            _obstacleCheckTimer += deltaTime;
            
            if (_obstacleCheckTimer >= _obstacleCheckInterval)
            {
                Transform transform = _carController.transform;
                Vector3 position = transform.position + Vector3.up * _obstacleHeightOffset;
                Vector3 forward = transform.forward;
                
                RaycastHit hit;
                _cachedObstacleDetected = Physics.Raycast(position, forward, out hit, 
                    _obstacleDetectionDistance, _obstacleLayerMask);
                
                if (_cachedObstacleDetected)
                {
                    _cachedMetrics.SteerInput = _avoidanceStrength;
                    _cachedMetrics.ThrottleWeight = 0f;
                    _cachedMetrics.BrakeWeight = 0.3f;
                    _cachedMetrics.Priority = 2f;
                }
                else
                {
                    _cachedMetrics = _zeroMetrics;
                }
                
                _obstacleCheckTimer = 0f;
            }

            return _cachedMetrics;
        }
    }
}
