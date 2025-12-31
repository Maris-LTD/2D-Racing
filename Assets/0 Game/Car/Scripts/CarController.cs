namespace Game.Car
{
    using System.Collections.Generic;
    using Data;
    using Game;
    using Input;
    using Map;
    using LevelManagement;
    using UnityEngine;

    public class CarController : MonoBehaviour, IRacer, ICarController
    {
        [SerializeField] private CarDataList _carDataList;
        [SerializeField] private SpriteRenderer _carSpriteRenderer;
        [SerializeField] private Rigidbody _carRigidbody;
        [SerializeField] private bool _isAIControlled ;

        private List<ICarModule> _carModules;
        private List<IUpdatable> _updatableModules = new();
        private List<IFixedUpdatable> _fixedUpdatableModules = new();
        private List<ITriggerEnter> _triggerEnterModules = new();
        private IMapProvider _mapProvider;
        private bool _canRace ;
        public int CurrentCarIndex { get; private set; }
        public CarDataList CarDataList => _carDataList;
        public SpriteRenderer CarSpriteRenderer => _carSpriteRenderer;
        public Rigidbody CarRigidbody => _carRigidbody;
        public IMapProvider MapProvider => _mapProvider;
        public bool IsAIControlled => _isAIControlled;
        public bool CanRace => _canRace;
        public IRacer Racer => this;
        private void Initialize()
        {
            OnInit();
        }

        private void OnInit()
        {
            if (_carDataList == null || _carDataList.carDataList == null || _carDataList.carDataList.Count == 0)
            {
                Debug.LogError("CarDataList is null or empty!");
                return;
            }

            CurrentCarIndex = IsAIControlled ? Random.Range(0, _carDataList.carDataList.Count) : 0;

            var inputModule = new CarInputModule();
            var statController = new Data.CarStatController();
            var viewModule = new Display.CarView();
            var movementController = new Movement.CarMovementController(statController);
            var checkpointTracker = new CheckpointTracker();
            _carModules = new List<ICarModule>
            {
                inputModule, statController, viewModule, movementController, checkpointTracker
            };

            _updatableModules.Clear();
            _fixedUpdatableModules.Clear();

            foreach (var module in _carModules)
            {
                if (module is CarModule carModule)
                {
                    carModule.SetController(this);
                }

                module.OnCarInit();

                if (module is IUpdatable updatable)
                {
                    _updatableModules.Add(updatable);
                }

                if (module is IFixedUpdatable fixedUpdatable)
                {
                    _fixedUpdatableModules.Add(fixedUpdatable);
                }

                if (module is ITriggerEnter triggerEnter)
                {
                    _triggerEnterModules.Add(triggerEnter);
                }
            }
        }

        private void Update()
        {
            if (_updatableModules == null)
            {
                return;
            }

            foreach (var updatable in _updatableModules)
            {
                if (updatable != null)
                {
                    updatable.OnUpdate(Time.deltaTime);
                }
            }
        }

        private void FixedUpdate()
        {
            if (_fixedUpdatableModules == null)
            {
                return;
            }

            foreach (var fixedUpdatable in _fixedUpdatableModules)
            {
                if (fixedUpdatable != null)
                {
                    fixedUpdatable.OnFixedUpdate(Time.fixedDeltaTime);
                }
            }
        }

        private void OnDestroy()
        {
            if (_carModules != null)
            {
                foreach (var module in _carModules)
                {
                    module.OnCarDestroy();
                }
            }
        }

        public void Initialize(IMapProvider mapProvider)
        {
            _mapProvider = mapProvider;

            Initialize();
        }

        public void OnRaceStart()
        {
            _canRace = true;
        }

        public void OnRaceFinished()
        {
            _canRace = false;
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other == null || _triggerEnterModules == null)
            {
                return;
            }

            foreach (var triggerEnterModule in _triggerEnterModules)
            {
                if (triggerEnterModule != null)
                {
                    triggerEnterModule.OnTriggerEnter(other);
                }
            }
        }

        public void ResetProgress()
        {
            _canRace = false;
            
            if (_carModules != null)
            {
                foreach (var module in _carModules)
                {
                    module.OnCarDestroy();
                }
            }
        }
    }
}