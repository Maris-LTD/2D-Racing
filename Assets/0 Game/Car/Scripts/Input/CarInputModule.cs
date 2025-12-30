using System;
using UnityEngine;

namespace Game.Car.Input
{
    public class CarInputModule : CarModule, ICarInputModule, IUpdatable
    {
        private ICarInputStrategy _inputStrategy;
        private CarInputData _lastBroadcastInput;
        private bool _hasBroadcastInput;

        public float SteerInput => _inputStrategy?.GetSteerInput() ?? 0f;
        public float ThrottleDuration => _inputStrategy?.GetThrottleDuration() ?? 0f;
        public float BrakeDuration => _inputStrategy?.GetBrakeDuration() ?? 0f;

        public override void OnCarInit()
        {
            InitializeStrategy();
        }

        public void SetStrategy(ICarInputStrategy strategy)
        {
            if (ReferenceEquals(_inputStrategy, strategy))
            {
                return;
            }

            CleanupStrategy();
            _inputStrategy = strategy;
        }

        private CarInputData _currentInput;

        public void OnUpdate(float deltaTime)
        {
            if (_controller == null || !_controller.CanRace)
            {
                return;
            }

            if (_inputStrategy == null)
            {
                return;
            }

            _inputStrategy.UpdateInput(deltaTime);

            _currentInput.CarId = _controller.GetInstanceID();
            _currentInput.SteerInput = SteerInput;
            _currentInput.ThrottleDuration = ThrottleDuration;
            _currentInput.BrakeDuration = BrakeDuration;

            if (!_hasBroadcastInput || !InputsEqual(_currentInput, _lastBroadcastInput))
            {
                _lastBroadcastInput = _currentInput;
                _hasBroadcastInput = true;
                Observer.Notify(_currentInput);
            }
        }

        public override void OnCarDestroy()
        {
            CleanupStrategy();
            base.OnCarDestroy();
        }

        private void InitializeStrategy()
        {
            if (_controller == null)
            {
                return;
            }

            if (_controller.IsAIControlled)
            {
                SetStrategy(new AICarInputStrategy(_controller));
            }
            else
            {
                SetStrategy(new NormalCarInputStrategy());
            }
        }

        private static bool InputsEqual(in CarInputData left, in CarInputData right)
        {
            return left.CarId == right.CarId &&
                   Mathf.Approximately(left.SteerInput, right.SteerInput) &&
                   Mathf.Approximately(left.ThrottleDuration, right.ThrottleDuration) &&
                   Mathf.Approximately(left.BrakeDuration, right.BrakeDuration);
        }

        private void CleanupStrategy()
        {
            if (_inputStrategy is IDisposable disposable)
            {
                disposable.Dispose();
            }

            _inputStrategy = null;
        }
    }
}
