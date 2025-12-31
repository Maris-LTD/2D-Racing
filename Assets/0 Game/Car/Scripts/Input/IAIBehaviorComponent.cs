using UnityEngine;
using Game.Map;
using Game.Car;
using Game;

namespace Game.Car.Input
{
    public struct AIBehaviorMetrics
    {
        public float SteerInput;
        public float ThrottleWeight;
        public float BrakeWeight;
        public float Priority;
        
        public static AIBehaviorMetrics Zero => new AIBehaviorMetrics
        {
            SteerInput = 0f,
            ThrottleWeight = 0f,
            BrakeWeight = 0f,
            Priority = 0f
        };
    }

    public interface IAIBehaviorComponent
    {
        void Initialize(ICarController car);
        AIBehaviorMetrics Calculate(float deltaTime);
    }

    public interface IAIPathDataProvider
    {
        ITrackData GetTrackData();
        float GetCachedProgress();
        Vector3 GetCachedSplineCenter();
        float GetCachedCurvature();
    }
}
