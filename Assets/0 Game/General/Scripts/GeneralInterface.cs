using UnityEngine;

namespace Game
{
    public interface IUpdatable
    {
        void OnUpdate(float deltaTime);
    }

    public interface IFixedUpdatable
    {
        void OnFixedUpdate(float deltaTime);
    }

    public interface ITriggerEnter
    {
        void OnTriggerEnter(Collider other);
    }
}
