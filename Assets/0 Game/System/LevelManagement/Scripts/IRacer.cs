namespace Game.LevelManagement
{
    using Game.Map;

    public interface IRacer
    {
        void Initialize(IMapProvider mapProvider);
        void OnRaceStart();
        void OnRaceFinished();
        void ResetProgress();
    }
}