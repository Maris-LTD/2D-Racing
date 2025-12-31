namespace Game.UI
{
    using UnityEngine;
    using TMPro;
    using GameFlow.Events;

    public class InGameUI : MonoBehaviour
    {
        [Header("Countdown")]
        [SerializeField] private TextMeshProUGUI _countdownText;

        [Header("Level Result")]
        [SerializeField] private GameObject _resultPanel;
        [SerializeField] private TextMeshProUGUI _resultText;

        private void OnEnable()
        {
            Observer.AddObserver<ShowInGameUIEvent>(OnShowInGameUI);
            Observer.AddObserver<CountdownUpdateEvent>(OnCountdownUpdate);
            Observer.AddObserver<LevelCompleteEvent>(OnLevelComplete);
        }

        private void OnDisable()
        {
            Observer.RemoveObserver<ShowInGameUIEvent>(OnShowInGameUI);
            Observer.RemoveObserver<CountdownUpdateEvent>(OnCountdownUpdate);
            Observer.RemoveObserver<LevelCompleteEvent>(OnLevelComplete);
        }

        private void Start()
        {
            ResetUI();
        }

        private void ResetUI()
        {
            if (_resultPanel != null)
            {
                _resultPanel.SetActive(false);
            }

            if (_countdownText != null)
            {
                _countdownText.transform.parent.gameObject.SetActive(false);
            }
        }

        private void OnShowInGameUI(ShowInGameUIEvent evt)
        {
            ResetUI();
        }

        private void OnCountdownUpdate(CountdownUpdateEvent evt)
        {
            if (_countdownText != null)
            {
                _countdownText.transform.parent.gameObject.SetActive(true);

                if (evt.CountdownValue > 0)
                {
                    _countdownText.text = evt.CountdownValue.ToString();
                }
                else
                {
                    _countdownText.text = "GO!";
                    StartCoroutine(HideCountdownAfterDelay(0.5f));
                }
            }
        }

        private System.Collections.IEnumerator HideCountdownAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (_countdownText != null)
            {
                _countdownText.transform.parent.gameObject.SetActive(false);
            }
        }

        private void OnLevelComplete(LevelCompleteEvent evt)
        {
            ShowLevelResult(evt);
        }

        private void ShowLevelResult(LevelCompleteEvent evt)
        {
            if (_resultPanel != null)
            {
                _resultPanel.SetActive(true);
            }

            if (_resultText != null)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder(128);
                sb.Append(" LEVEL COMPLETE! \n\n");
                sb.Append("Total Laps: ").Append(evt.TotalLaps).Append("\n");
                sb.Append("Total Racers: ").Append(evt.TotalRacers).Append("\n");
                
                if (evt.Winner != null)
                {
                    sb.Append("\n Winner: Racer Completed!");
                }

                _resultText.text = sb.ToString();
            }
        }

        public void OnHomeButtonClicked()
        {
            Observer.Notify(new ReturnToHomeEvent());
            ResetUI();
        }
    }
}

