namespace Game.Car
{
    using System.Collections.Generic;
    using LevelManagement;
    using Map;
    using UnityEngine;

    public class CheckpointTracker : CarModule, ITriggerEnter
    {
        private HashSet<int> _passedCheckpoints;
        private int _totalCheckpoints;
        private int _currentLap;
        private bool _hasStarted;
        private bool _isInitialized;

        public int CurrentLap => _currentLap;

        public override void OnCarInit()
        {
            _passedCheckpoints = new HashSet<int>();
            _currentLap = 0;
            _hasStarted = false;
            _isInitialized = false;
        }

        private void EnsureInitialized()
        {
            if (!_isInitialized && _controller != null && _controller.MapProvider != null)
            {
                _totalCheckpoints = _controller.MapProvider.GetTotalCheckpoints();
                _isInitialized = true;
            }
        }

        public override void OnCarDestroy()
        {
            ResetProgress();
            base.OnCarDestroy();
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<Checkpoint>(out var checkpoint))
            {
                OnCheckpointEnter(checkpoint);
            }
        }

        public void OnCheckpointEnter(Checkpoint checkpoint)
        {
            EnsureInitialized();

            if (checkpoint.IsStartFinish)
            {
                HandleStartFinishCheckpoint(checkpoint);
            }
            else
            {
                HandleRegularCheckpoint(checkpoint);
            }
        }

        private void HandleStartFinishCheckpoint(Checkpoint checkpoint)
        {
            if (!_hasStarted)
            {
                _hasStarted = true;
                _passedCheckpoints.Clear();
                return;
            }

            if (HasCompletedAllCheckpoints())
            {
                _currentLap++;
                
                Observer.Notify(new LapCompletionEvent
                {
                    Racer = _controller,
                    CompletedLap = _currentLap
                });

                _passedCheckpoints.Clear();
            }
        }

        private void HandleRegularCheckpoint(Checkpoint checkpoint)
        {
            if (!_hasStarted)
            {
                return;
            }

            int expectedIndex = _passedCheckpoints.Count;
            
            if (checkpoint.CheckpointIndex == expectedIndex)
            {
                _passedCheckpoints.Add(checkpoint.CheckpointIndex);
            }
        }

        private bool HasCompletedAllCheckpoints()
        {
            return _passedCheckpoints.Count == _totalCheckpoints;
        }

        public void ResetProgress()
        {
            _passedCheckpoints?.Clear();
            _currentLap = 0;
            _hasStarted = false;
        }
    }
}

