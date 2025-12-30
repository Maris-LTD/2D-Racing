namespace Game.LevelManagement.States
{
    using UnityEngine;
    using Game.GameFlow.Events;

    public class CountdownState : LevelStateBase
    {
        private float _timer;
        private int _lastCountdownValue;

        public override void Enter(LevelController controller)
        {
            base.Enter(controller);
            _timer = controller.CountdownTime;
            _lastCountdownValue = Mathf.CeilToInt(_timer);
            Observer.Notify(new CountdownUpdateEvent { CountdownValue = _lastCountdownValue });
        }

        public override void Update()
        {
            _timer -= Time.deltaTime;
            
            int currentCountdownValue = Mathf.CeilToInt(_timer);
            
            if (currentCountdownValue != _lastCountdownValue && currentCountdownValue >= 0)
            {
                _lastCountdownValue = currentCountdownValue;
                Observer.Notify(new CountdownUpdateEvent { CountdownValue = currentCountdownValue });
            }
            
            if (_timer <= 0)
            {
                _controller.ChangeState(new RacingState());
            }
        }
    }
}