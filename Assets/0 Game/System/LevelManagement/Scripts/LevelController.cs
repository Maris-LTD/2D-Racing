namespace Game.LevelManagement
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Game.Map;
    using Game.LevelManagement.States;
    using Game.GameFlow.Events;

    public class LevelController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int _totalLaps = 3;
        [SerializeField] private float _countdownTime = 3f;
        [SerializeField] private string _trackName = "Racing Track";

        private ILevelState _currentState;
        private IMapProvider _mapProvider;
        private List<IRacer> _racers = new List<IRacer>();
        private TrackLoader _trackLoader;
        private GameObject _currentTrack;
        private CarSpawner _carSpawner;

        public float CountdownTime => _countdownTime;
        public int TotalLaps => _totalLaps;
        public List<IRacer> Racers => _racers;

        private void OnEnable()
        {
            Observer.AddObserver<StartLevelEvent>(OnStartLevel);
            Observer.AddObserver<CarsSpawnedEvent>(OnCarsSpawned);
            Observer.AddObserver<CleanupLevelEvent>(OnCleanupLevel);
        }

        private void OnDisable()
        {
            Observer.RemoveObserver<StartLevelEvent>(OnStartLevel);
            Observer.RemoveObserver<CarsSpawnedEvent>(OnCarsSpawned);
            Observer.RemoveObserver<CleanupLevelEvent>(OnCleanupLevel);
        }

        private void OnStartLevel(StartLevelEvent evt)
        {
            StartLevel();
        }

        public void StartLevel()
        {
            InitializeLevel();
        }

        private void InitializeLevel()
        {
            if (_trackLoader == null)
            {
                _trackLoader = new TrackLoader();
            }

            StartCoroutine(LoadTrackCoroutine());
        }

        private IEnumerator LoadTrackCoroutine()
        {
            GameObject loadedTrack = null;
            yield return StartCoroutine(_trackLoader.LoadTrackCoroutine(_trackName, (track) => loadedTrack = track));

            _currentTrack = loadedTrack;

            if (_currentTrack == null)
            {
                yield break;
            }

            _carSpawner = _currentTrack.GetComponentInChildren<CarSpawner>();
            _mapProvider = _currentTrack.GetComponentInChildren<MapController>();
        }

        private void OnCarsSpawned(CarsSpawnedEvent evt)
        {
            InitializeRacers();
        }

        private void OnCleanupLevel(CleanupLevelEvent evt)
        {
            CleanupLevel();
        }

        public void CleanupLevel()
        {
            if (_currentState != null)
            {
                _currentState.Exit();
                _currentState = null;
            }

            _racers.Clear();
            _mapProvider = null;

            if (_currentTrack != null)
            {
                _carSpawner?.ClearSpawnedCars();
                Destroy(_currentTrack);
                _currentTrack = null;
                _carSpawner = null;
            }
        }

        private void InitializeRacers()
        {
            if (_carSpawner == null || _mapProvider == null)
            {
                return;
            }

            _racers.Clear();

            foreach (var spawnedCar in _carSpawner.GetSpawnedCars())
            {
                if (spawnedCar == null)
                {
                    continue;
                }

                var racer = spawnedCar.GetComponent<IRacer>();
                if (racer != null)
                {
                    _racers.Add(racer);
                }
            }

            foreach (var racer in _racers)
            {
                racer.Initialize(_mapProvider);
            }

            ChangeState(new CountdownState());
        }

        private void Update()
        {
            if (_currentState != null)
            {
                _currentState.Update();
            }
        }

        public void ChangeState(ILevelState newState)
        {
            if (_currentState != null)
            {
                _currentState.Exit();
            }

            _currentState = newState;
            _currentState.Enter(this);
        }
    }
}