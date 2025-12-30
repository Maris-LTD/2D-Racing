using System;

namespace Game.Car.Input
{
    public class NormalCarInputStrategy : ICarInputStrategy, IDisposable
    {
        private float _currentSteerInput;
        private bool _isThrottling;
        private bool _isBraking;
        private float _throttleDuration;
        private float _brakeDuration;

        public NormalCarInputStrategy()
        {
            Observer.AddObserver<CarInputSteerEvent>(OnSteerInput);
            Observer.AddObserver<CarInputThrottleEvent>(OnThrottleInput);
            Observer.AddObserver<CarInputBrakeEvent>(OnBrakeInput);
        }

        public void Dispose()
        {
            Observer.RemoveObserver<CarInputSteerEvent>(OnSteerInput);
            Observer.RemoveObserver<CarInputThrottleEvent>(OnThrottleInput);
            Observer.RemoveObserver<CarInputBrakeEvent>(OnBrakeInput);
        }

        private void OnSteerInput(CarInputSteerEvent evt)
        {
            _currentSteerInput = evt.Value;
        }

        private void OnThrottleInput(CarInputThrottleEvent evt)
        {
            _isThrottling = evt.IsPressed;
        }

        private void OnBrakeInput(CarInputBrakeEvent evt)
        {
            _isBraking = evt.IsPressed;
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
            if (_isThrottling)
            {
                _throttleDuration += deltaTime;
            }
            else
            {
                _throttleDuration = 0f;
            }

            if (_isBraking)
            {
                _brakeDuration += deltaTime;
            }
            else
            {
                _brakeDuration = 0f;
            }
        }
    }
}
