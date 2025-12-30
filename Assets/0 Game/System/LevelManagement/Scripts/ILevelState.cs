namespace Game.LevelManagement
{
    public interface ILevelState
    {
        void Enter(LevelController controller);
        void Update();
        void Exit();
    }
}