namespace Game.GameFlow.Events
{
    using UnityEngine;
    using Game.LevelManagement;
    
    public struct StartGameRequestedEvent { }
    
    public struct ShowOutGameUIEvent { }
    
    public struct ShowInGameUIEvent { }
    
    public struct StartLevelEvent { }
    
    public struct TrackLoadedEvent 
    {
        public GameObject TrackInstance;
    }
    
    public struct CarsSpawnedEvent { }
    
    public struct CountdownUpdateEvent 
    {
        public int CountdownValue;
    }
    
    public struct LevelCompleteEvent 
    {
        public IRacer Winner;
        public int TotalRacers;
        public int TotalLaps;
    }
    
    public struct ReturnToHomeEvent { }
    
    public struct CleanupLevelEvent { }
}

