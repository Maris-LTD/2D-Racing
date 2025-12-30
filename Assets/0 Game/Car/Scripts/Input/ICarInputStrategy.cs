namespace Game.Car.Input
{
    public interface ICarInputStrategy
    {
        float GetSteerInput();
        float GetThrottleDuration();
        float GetBrakeDuration();
        void UpdateInput(float deltaTime);
    }
}
