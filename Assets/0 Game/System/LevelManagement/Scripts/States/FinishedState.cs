namespace Game.LevelManagement.States
{
    using UnityEngine;
    using System.Linq;
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

            var winner = _controller.Racers.FirstOrDefault();
            
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