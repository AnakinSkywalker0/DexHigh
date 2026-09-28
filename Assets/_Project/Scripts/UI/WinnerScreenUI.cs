using UnityEngine;
using TMPro;
using DexHigh.Systems;

namespace DexHigh.UI
{
    public class WinnerScreenUI : MonoBehaviour
    {
        [SerializeField] MatchManager matchManager;
        [SerializeField] GameObject panelRoot;
        [SerializeField] TMP_Text winnerText;

        void OnEnable()
        {
            if (matchManager != null) matchManager.OnMatchEnded += HandleMatchEnded;
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        void OnDisable()
        {
            if (matchManager != null) matchManager.OnMatchEnded -= HandleMatchEnded;
        }

        void HandleMatchEnded(string winnerName)
        {
            if (panelRoot != null) panelRoot.SetActive(true);
            if (winnerText != null) winnerText.text = $"{winnerName} Wins!";
        }

        public void OnRestartPressed()
        {
            matchManager?.Restart();
        }
    }
}
