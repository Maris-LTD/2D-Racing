namespace Game.GameFlow
{
    using UnityEngine;
    using Game.GameFlow.Events;

    public class GameFlowController : MonoBehaviour
    {
        private void OnEnable()
        {
            Observer.AddObserver<StartGameRequestedEvent>(OnStartGameRequested);
            Observer.AddObserver<ReturnToHomeEvent>(OnReturnToHome);
            Observer.AddObserver<TrackLoadedEvent>(OnTrackLoaded);
        }

        private void OnDisable()
        {
            Observer.RemoveObserver<StartGameRequestedEvent>(OnStartGameRequested);
            Observer.RemoveObserver<ReturnToHomeEvent>(OnReturnToHome);
            Observer.RemoveObserver<TrackLoadedEvent>(OnTrackLoaded);
        }

        private void Start()
        {
            Observer.Notify(new ShowOutGameUIEvent());
        }

        private void OnStartGameRequested(StartGameRequestedEvent evt)
        {
            Observer.Notify(new StartLevelEvent());
        }

        private void OnTrackLoaded(TrackLoadedEvent evt)
        {
            Observer.Notify(new ShowInGameUIEvent());
        }

        private void OnReturnToHome(ReturnToHomeEvent evt)
        {
            Observer.Notify(new CleanupLevelEvent());
            Observer.Notify(new ShowOutGameUIEvent());
        }
    }
}

