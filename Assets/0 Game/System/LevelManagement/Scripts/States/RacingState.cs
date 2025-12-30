namespace Game.LevelManagement.States
{
    using UnityEngine;
    using Game; // For Observer

    public class RacingState : LevelStateBase
    {
        public override void Enter(LevelController controller)
        {
            base.Enter(controller);
            
            Observer.AddObserver<LapCompletionEvent>(OnLapCompleted);

            foreach (var racer in _controller.Racers)
            {
                racer.OnRaceStart();
            }
        }

        public override void Exit()
        {
            base.Exit();
            Observer.RemoveObserver<LapCompletionEvent>(OnLapCompleted);
        }

        public override void Update()
        {
        }

        private void OnLapCompleted(LapCompletionEvent evt)
        {
            if (evt.CompletedLap >= _controller.TotalLaps)
            {
                _controller.ChangeState(new FinishedState());
            }
        }
    }
}