using UnityEngine;
using Game.Map;
using Game.Car.Data;
using Game.LevelManagement;

namespace Game.Car
{
    public interface ICarController
    {
        Transform transform { get; }
        int CurrentCarIndex { get; }
        CarDataList CarDataList { get; }
        SpriteRenderer CarSpriteRenderer { get; }
        Rigidbody CarRigidbody { get; }
        IMapProvider MapProvider { get; }
        bool IsAIControlled { get; }
        bool CanRace { get; }
        int GetInstanceID();
        IRacer Racer { get; }
    }
}
