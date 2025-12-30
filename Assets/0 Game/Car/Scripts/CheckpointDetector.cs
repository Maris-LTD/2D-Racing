namespace Game.Car
{
    using Map;
    using UnityEngine;

    public class CheckpointDetector : MonoBehaviour
    {
        private CarController _carController;

        private void Awake()
        {
            _carController = GetComponent<CarController>();
        }

        private void OnTriggerEnter(Collider other)
        {
            _carController?.OnTriggerEnter(other);
        }
    }
}

