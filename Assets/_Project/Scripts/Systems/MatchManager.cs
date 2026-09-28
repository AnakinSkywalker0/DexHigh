using UnityEngine;
using UnityEngine.SceneManagement;
using DexHigh.Core;

namespace DexHigh.Systems
{
    public class MatchManager : MonoBehaviour
    {
        [SerializeField] Health playerHealth;
        [SerializeField] Health aiHealth;
        [SerializeField] string playerDisplayName = "Player Dragon";
        [SerializeField] string aiDisplayName = "Enemy Dragon";

        public System.Action<string> OnMatchEnded; // winner display name

        bool matchOver;

        void OnEnable()
        {
            if (playerHealth != null) playerHealth.OnDeath.AddListener(HandlePlayerDeath);
            if (aiHealth != null) aiHealth.OnDeath.AddListener(HandleAiDeath);
        }

        void OnDisable()
        {
            if (playerHealth != null) playerHealth.OnDeath.RemoveListener(HandlePlayerDeath);
            if (aiHealth != null) aiHealth.OnDeath.RemoveListener(HandleAiDeath);
        }

        void HandlePlayerDeath() => EndMatch(aiDisplayName);
        void HandleAiDeath() => EndMatch(playerDisplayName);

        void EndMatch(string winnerName)
        {
            if (matchOver) return;
            matchOver = true;
            OnMatchEnded?.Invoke(winnerName);
        }

        public void Restart()
        {
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.buildIndex);
        }
    }
}
