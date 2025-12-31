using UnityEngine;
using Game.Map;
using System.Collections.Generic;
using Game;
using Game.Car;

namespace Game.Car.Input
{
    public class AICarInputStrategy : ICarInputStrategy
    {
        private readonly ICarController _carController;
        private readonly List<IAIBehaviorComponent> _behaviors;
        private readonly List<IUpdatable> _updatableBehaviors;

        private float _throttleDuration;
        private float _brakeDuration;
        private float _currentSteerInput;
        private float _targetSteerInput;

        private float _steerSmoothing = 12f;
        private float _sharpCurveThreshold = 0.35f;


        public AICarInputStrategy(ICarController carController)
        {
            _carController = carController;
            _behaviors = new List<IAIBehaviorComponent>();
            _updatableBehaviors = new List<IUpdatable>();

            InitializeBehaviors();
        }

        private void InitializeBehaviors()
        {
            var pathFollowing = new AIPathFollowingBehavior();
            pathFollowing.Initialize(_carController);
            _behaviors.Add(pathFollowing);
            _updatableBehaviors.Add(pathFollowing);

            var obstacleAvoidance = new AIObstacleAvoidanceBehavior();
            obstacleAvoidance.Initialize(_carController);
            _behaviors.Add(obstacleAvoidance);

            var speedControl = new AISpeedControlBehavior();
            speedControl.Initialize(_carController);
            speedControl.SetPathDataProvider(pathFollowing);
            _behaviors.Add(speedControl);

            var recovery = new AIRecoveryBehavior();
            recovery.Initialize(_carController);
            _behaviors.Add(recovery);
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

            foreach (var updatableBehavior in _updatableBehaviors)
            {
                updatableBehavior.OnUpdate(deltaTime);
            }

            AggregateBehaviorMetrics(deltaTime);
        }

        private void AggregateBehaviorMetrics(float deltaTime)
        {
            float totalSteerInput = 0f;
            float totalThrottleWeight = 0f;
            float totalBrakeWeight = 0f;
            float totalPriority = 0f;

            foreach (var behavior in _behaviors)
            {
                var metrics = behavior.Calculate(deltaTime);
                if (metrics.Priority <= 0f) continue;

                float weight = metrics.Priority;
                totalSteerInput += metrics.SteerInput * weight;
                totalThrottleWeight += metrics.ThrottleWeight * weight;
                totalBrakeWeight += metrics.BrakeWeight * weight;
                totalPriority += weight;
            }

            if (totalPriority > 0f)
            {
                float invPriority = 1f / totalPriority;
                totalSteerInput *= invPriority;
                totalThrottleWeight *= invPriority;
                totalBrakeWeight *= invPriority;
            }

            _targetSteerInput = Mathf.Clamp(totalSteerInput, -1f, 1f);
            float smoothing = _steerSmoothing;
            if (_behaviors.Count > 0 && _behaviors[0] is IAIPathDataProvider pathProvider)
            {
                float curvature = pathProvider.GetCachedCurvature();
                if (curvature > _sharpCurveThreshold)
                {
                    smoothing = _steerSmoothing * 1.5f;
                }
            }
            _currentSteerInput = Mathf.Lerp(_currentSteerInput, _targetSteerInput, smoothing * deltaTime);

            if (totalBrakeWeight > 0f)
            {
                _throttleDuration = 0f;
                _brakeDuration += deltaTime;
            }
            else if (totalThrottleWeight > 0f)
            {
                _throttleDuration += deltaTime;
                _brakeDuration = 0f;
            }
            else
            {
                _throttleDuration = 0f;
                _brakeDuration = 0f;
            }
        }

    }
}

