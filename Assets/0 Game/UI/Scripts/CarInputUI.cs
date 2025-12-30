using UnityEngine;
using UnityEngine.EventSystems;
using Game.Car.Input;

namespace Game.UI
{
    public class CarInputUI : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private GameObject _gasButton;
        [SerializeField] private GameObject _brakeButton;
        [SerializeField] private GameObject _leftButton;
        [SerializeField] private GameObject _rightButton;

        private bool _holdingLeft;
        private bool _holdingRight;

        private CarInputThrottleEvent _lastThrottleEvent;
        private CarInputBrakeEvent _lastBrakeEvent;
        private CarInputSteerEvent _lastSteerEvent;

        private bool _keyboardThrottlePressed;
        private bool _keyboardBrakePressed;
        private float _keyboardSteerValue;

        private bool _pointerThrottlePressed;
        private bool _pointerBrakePressed;

        private const KeyCode GAS_KEY = KeyCode.UpArrow;
        private const KeyCode BRAKE_KEY = KeyCode.DownArrow;
        private const KeyCode LEFT_KEY = KeyCode.LeftArrow;
        private const KeyCode RIGHT_KEY = KeyCode.RightArrow;

        private void Start()
        {
            SetupButton(_gasButton, OnGasDown, OnGasUp);
            SetupButton(_brakeButton, OnBrakeDown, OnBrakeUp);
            SetupButton(_leftButton, OnLeftDown, OnLeftUp);
            SetupButton(_rightButton, OnRightDown, OnRightUp);
        }

        private void Update()
        {
            HandleKeyboardThrottle();
            HandleKeyboardBrake();
            HandleKeyboardSteer();
        }

        private void SetupButton(GameObject button, System.Action onDown, System.Action onUp)
        {
            if (button == null) return;

            EventTrigger trigger = button.GetComponent<EventTrigger>();
            if (trigger == null) trigger = button.AddComponent<EventTrigger>();

            EventTrigger.Entry downEntry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerDown
            };
            downEntry.callback.AddListener((_) => { onDown?.Invoke(); });
            trigger.triggers.Add(downEntry);

            EventTrigger.Entry upEntry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerUp
            };
            upEntry.callback.AddListener((_) => { onUp?.Invoke(); });
            trigger.triggers.Add(upEntry);
        }

        private void OnGasDown()
        {
            _pointerThrottlePressed = true;
            TryNotifyThrottle(CurrentThrottleState);
        }

        private void OnGasUp()
        {
            _pointerThrottlePressed = false;
            TryNotifyThrottle(CurrentThrottleState);
        }

        private void OnBrakeDown()
        {
            _pointerBrakePressed = true;
            TryNotifyBrake(CurrentBrakeState);
        }

        private void OnBrakeUp()
        {
            _pointerBrakePressed = false;
            TryNotifyBrake(CurrentBrakeState);
        }

        private void OnLeftDown()
        {
            _holdingLeft = true;
            UpdateSteer();
        }

        private void OnLeftUp()
        {
            _holdingLeft = false;
            UpdateSteer();
        }

        private void OnRightDown()
        {
            _holdingRight = true;
            UpdateSteer();
        }

        private void OnRightUp()
        {
            _holdingRight = false;
            UpdateSteer();
        }

        private void UpdateSteer()
        {
            float pointerValue = 0f;
            if (_holdingLeft) pointerValue -= 1f;
            if (_holdingRight) pointerValue += 1f;

            float valueToSend = !Mathf.Approximately(pointerValue, 0f) ? pointerValue : _keyboardSteerValue;
            TryNotifySteer(valueToSend);
        }

        private void HandleKeyboardThrottle()
        {
            bool keyboardThrottle = Input.GetKey(GAS_KEY);
            if (keyboardThrottle == _keyboardThrottlePressed)
            {
                return;
            }

            _keyboardThrottlePressed = keyboardThrottle;
            TryNotifyThrottle(CurrentThrottleState);
        }

        private void HandleKeyboardBrake()
        {
            bool keyboardBrake = Input.GetKey(BRAKE_KEY);
            if (keyboardBrake == _keyboardBrakePressed)
            {
                return;
            }

            _keyboardBrakePressed = keyboardBrake;
            TryNotifyBrake(CurrentBrakeState);
        }

        private void HandleKeyboardSteer()
        {
            bool keyboardLeft = Input.GetKey(LEFT_KEY);
            bool keyboardRight = Input.GetKey(RIGHT_KEY);

            float steerValue = 0f;
            if (keyboardLeft) steerValue -= 1f;
            if (keyboardRight) steerValue += 1f;

            if (Mathf.Approximately(steerValue, _keyboardSteerValue))
            {
                return;
            }

            _keyboardSteerValue = steerValue;
            float currentValue = GetCurrentSteerValue();
            TryNotifySteer(currentValue);
        }

        private float GetCurrentSteerValue()
        {
            float pointerValue = 0f;
            if (_holdingLeft) pointerValue -= 1f;
            if (_holdingRight) pointerValue += 1f;

            return !Mathf.Approximately(pointerValue, 0f) ? pointerValue : _keyboardSteerValue;
        }

        private bool CurrentThrottleState => _keyboardThrottlePressed || _pointerThrottlePressed;
        private bool CurrentBrakeState => _keyboardBrakePressed || _pointerBrakePressed;

        private void TryNotifyThrottle(bool isPressed)
        {
            if (_lastThrottleEvent.IsPressed == isPressed)
            {
                return;
            }

            _lastThrottleEvent.IsPressed = isPressed;
            Observer.Notify(_lastThrottleEvent);
        }

        private void TryNotifyBrake(bool isPressed)
        {
            if (_lastBrakeEvent.IsPressed == isPressed)
            {
                return;
            }

            _lastBrakeEvent.IsPressed = isPressed;
            Observer.Notify(_lastBrakeEvent);
        }

        private void TryNotifySteer(float value)
        {
            if (Mathf.Approximately(_lastSteerEvent.Value, value))
            {
                return;
            }

            _lastSteerEvent.Value = value;
            Observer.Notify(_lastSteerEvent);
        }
    }
}

