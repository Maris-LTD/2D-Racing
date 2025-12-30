namespace Game.UI
{
    using UnityEngine;
    using Game.GameFlow.Events;

    public class UIManager : MonoBehaviour
    {
        [Header("UI Panels")]
        [SerializeField] private GameObject _outGameUI;
        [SerializeField] private GameObject _inGameUI;

        private void OnEnable()
        {
            Observer.AddObserver<ShowOutGameUIEvent>(OnShowOutGameUI);
            Observer.AddObserver<ShowInGameUIEvent>(OnShowInGameUI);
        }

        private void OnDisable()
        {
            Observer.RemoveObserver<ShowOutGameUIEvent>(OnShowOutGameUI);
            Observer.RemoveObserver<ShowInGameUIEvent>(OnShowInGameUI);
        }

        private void OnShowOutGameUI(ShowOutGameUIEvent evt)
        {
            _outGameUI.SetActive(true);
            _inGameUI.SetActive(false);
        }

        private void OnShowInGameUI(ShowInGameUIEvent evt)
        {
            _outGameUI.SetActive(false);
            _inGameUI.SetActive(true);
        }
    }
}

