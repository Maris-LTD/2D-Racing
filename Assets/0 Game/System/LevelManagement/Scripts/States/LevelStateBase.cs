namespace Game.LevelManagement.States
{
    public abstract class LevelStateBase : ILevelState
    {
        protected LevelController _controller;

        public virtual void Enter(LevelController controller)
        {
            _controller = controller;
        }

        public abstract void Update();

        public virtual void Exit() { }
    }
}