using UnityEngine;
using Game.Car.Input;

namespace Game.Car.Movement
{
    public class CarMovementController : CarModule, ICarMovementModule, IFixedUpdatable
    {
        private readonly ICarStatModule _statModule;
        private Rigidbody _rb;
        private CarInputData _currentInput;

        private readonly float _driftFactor = 0.95f;
        private readonly float _turnSpeed = 150f;

        public CarMovementController(ICarStatModule statModule)
        {
            _statModule = statModule;
        }

        public override void OnCarInit()
        {
            _rb = _controller.GetComponent<Rigidbody>();

            Observer.AddObserver<CarInputData>(OnInputReceived);
        }

        public override void OnCarDestroy()
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            Observer.RemoveObserver<CarInputData>(OnInputReceived);
        }

        private void OnInputReceived(CarInputData data)
        {
            if (data.CarId == _controller.GetInstanceID())
            {
                _currentInput = data;
            }
        }

        public void OnFixedUpdate(float deltaTime)
        {
            if (_statModule == null || _rb == null) return;

            HandleMovement(deltaTime);
        }

        private void HandleMovement(float deltaTime)
        {
            float steerInput = _currentInput.SteerInput;
            if (Mathf.Abs(steerInput) > 0.01f)
            {
                float turnAmount = steerInput * _turnSpeed * deltaTime;
                Quaternion turnRotation = Quaternion.Euler(0f, turnAmount, 0f);
                _rb.MoveRotation(_rb.rotation * turnRotation);
            }

            float maxSpeed = _statModule.GetSpeed();
            float acceleration = _statModule.GetAcceleration();
            
            bool isAccelerating = _currentInput.ThrottleDuration > 0;
            bool isBraking = _currentInput.BrakeDuration > 0;

            Vector3 forwardDir = _rb.transform.forward;
            float forwardSpeed = Vector3.Dot(_rb.linearVelocity, forwardDir);

            _rb.linearDamping = 1f;

            if (isAccelerating)
            {
                _rb.linearDamping = 0f;
                if (forwardSpeed < maxSpeed)
                {
                    Vector3 force = forwardDir * (acceleration * _rb.mass); 
                    _rb.AddForce(force);
                }
            }
            
            if (isBraking)
            {
                if (forwardSpeed > 0.1f)
                {
                    _rb.linearDamping = 3f;
                }
                else
                {
                    _rb.linearDamping = 0f;
                    if (forwardSpeed > -maxSpeed * 0.5f)
                    {
                         Vector3 reverseForce = -forwardDir * (acceleration * _rb.mass);
                         _rb.AddForce(reverseForce);
                    }
                }
            }
            else if (isAccelerating)
            {
                 _rb.linearDamping = 0f;
            }

            ApplyLateralFriction();
        }

        private void ApplyLateralFriction()
        {
            Vector3 rightNormal = _rb.transform.right;
            float lateralVelocity = Vector3.Dot(_rb.linearVelocity, rightNormal);
            
            if (Mathf.Abs(lateralVelocity) > 0.01f)
            {
                 Vector3 lateralFrictionImpulse = -rightNormal * (lateralVelocity * _driftFactor);
                 _rb.AddForce(lateralFrictionImpulse * _rb.mass, ForceMode.Impulse);
            }
        }
    }
}
