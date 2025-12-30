namespace Game.Car.Input
{
    public struct CarInputSteerEvent
    {
        public float Value;
    }

    public struct CarInputThrottleEvent
    {
        public bool IsPressed;
    }

    public struct CarInputBrakeEvent
    {
        public bool IsPressed;
    }
}
