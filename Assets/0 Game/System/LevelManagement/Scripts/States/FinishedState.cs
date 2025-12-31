namespace Game.LevelManagement.States
{
    using UnityEngine;
    using Game.GameFlow.Events;

    public class FinishedState : LevelStateBase
    {
        public override void Enter(LevelController controller)
        {
            base.Enter(controller);

            foreach (var racer in _controller.Racers)
            {
                racer.OnRaceFinished();
            }

            IRacer winner = null;
            if (_controller.Racers.Count > 0)
            {
                winner = _controller.Racers[0];
            }
            
            Observer.Notify(new LevelCompleteEvent
            {
                Winner = winner,
                TotalRacers = _controller.Racers.Count,
                TotalLaps = _controller.TotalLaps
            });
        }

        public override void Update()
        {
        }
    }
}