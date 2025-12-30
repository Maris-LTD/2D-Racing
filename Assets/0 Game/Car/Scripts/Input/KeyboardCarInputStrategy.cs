using UnityEngine;

namespace Game.Car.Input
{
    public class KeyboardCarInputStrategy : ICarInputStrategy
    {
        private float _throttleDuration;
        private float _brakeDuration;

        private const KeyCode GAS_KEY = KeyCode.UpArrow;
        private const KeyCode BRAKE_KEY = KeyCode.DownArrow;

        public float GetSteerInput()
        {
            return UnityEngine.Input.GetAxis("Horizontal");
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
            if (UnityEngine.Input.GetKey(GAS_KEY))
            {
                _throttleDuration += deltaTime;
            }
            else
            {
                _throttleDuration = 0f;
            }

            if (UnityEngine.Input.GetKey(BRAKE_KEY))
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
