namespace Game.Map
{
    using UnityEngine;
    using System.Collections.Generic;
    using Pooling;
    using GameFlow.Events;

    public class CarSpawner : MonoBehaviour
    {
        [SerializeField] private MapController _mapController;
        [SerializeField] private GameObject _playerCarPrefab;
        [SerializeField] private GameObject _aiCarPrefab;
        [SerializeField] private bool _spawnOnStart = true;
        [SerializeField] private Transform _cameraTarget;

        private List<GameObject> _spawnedCars = new List<GameObject>();
        private GameObject _playerCar;

        private void Start()
        {
            if (_spawnOnStart)
            {
                SpawnAllCars();
            }
        }

        public void SpawnAllCars()
        {
            ClearSpawnedCars();

            if (_mapController == null)
            {
                return;
            }

            var spawnPoints = _mapController.GetSpawnPoints();
            GameObject firstPlayerCar = null;
            
            foreach (var spawnPoint in spawnPoints)
            {
                if (spawnPoint == null) continue;

                GameObject carPrefab = spawnPoint.IsAISpawn ? _aiCarPrefab : _playerCarPrefab;
                
                if (carPrefab == null)
                {
                    continue;
                }

                GameObject car = GlobalObjectPoolManager.Instance.GetGameObject(carPrefab, spawnPoint.Position, spawnPoint.Rotation);
                if (car == null)
                {
                    continue;
                }

                car.name = spawnPoint.IsAISpawn ? $"AI_Car_{spawnPoint.SpawnIndex}" : $"Player_Car_{spawnPoint.SpawnIndex}";
                
                _spawnedCars.Add(car);
                
                if (!spawnPoint.IsAISpawn && firstPlayerCar == null)
                {
                    firstPlayerCar = car;
                }
            }

            if (firstPlayerCar != null)
            {
                _playerCar = firstPlayerCar;
                AssignCameraTarget(_playerCar);
            }

            Observer.Notify(new CarsSpawnedEvent());
        }

        private void AssignCameraTarget(GameObject playerCar)
        {
            if (_cameraTarget == null)
            {
                return;
            }

            _cameraTarget.position = playerCar.transform.position;
            _cameraTarget.SetParent(playerCar.transform);
        }

        public void ClearSpawnedCars()
        {
            foreach (var car in _spawnedCars)
            {
                if (car != null)
                {
                    var racer = car.GetComponent<LevelManagement.IRacer>();
                    if (racer != null)
                    {
                        racer.ResetProgress();
                    }

                    PooledObject pooledObject = car.GetComponent<PooledObject>();
                    if (pooledObject != null)
                    {
                        pooledObject.ReturnToPool();
                    }
                    else
                    {
                        Destroy(car);
                    }
                }
            }
            _spawnedCars.Clear();
            _playerCar = null;
        }

        public List<GameObject> GetSpawnedCars()
        {
            return _spawnedCars;
        }

        public GameObject GetPlayerCar()
        {
            return _playerCar;
        }

#if UNITY_EDITOR
        [ContextMenu("Spawn Cars")]
        private void EditorSpawnCars()
        {
            SpawnAllCars();
        }

        [ContextMenu("Clear Spawned Cars")]
        private void EditorClearCars()
        {
            ClearSpawnedCars();
        }
#endif
    }
}
