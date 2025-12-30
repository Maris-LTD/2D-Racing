namespace Game.UI
{
    using UnityEngine;
    using Game.GameFlow.Events;

    public class OutGameUI : MonoBehaviour
    {
        public void OnPlayButtonClicked()
        {
            Observer.Notify(new StartGameRequestedEvent());
        }
    }
}

