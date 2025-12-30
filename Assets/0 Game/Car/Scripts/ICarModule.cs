using UnityEngine;

namespace Game.Car{
    public interface ICarModule{
        void OnCarInit();
        void OnCarDestroy();
    }

    public interface ICarStatModule : ICarModule{
        public float GetSpeed();
        public float GetAcceleration();
    }

    public interface ICarDisplayModule : ICarModule{

    }

    public interface ICarMovementModule : ICarModule{

    }

    public interface ICarInputModule : ICarModule{
        float SteerInput { get; }
        float ThrottleDuration { get; }
        float BrakeDuration { get; }
    }
}