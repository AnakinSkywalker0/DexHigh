using UnityEngine;
using UnityEngine.UI;
using DexHigh.Core;

namespace DexHigh.UI
{
    public class HealthBarUI : MonoBehaviour
    {
        [SerializeField] Health health;
        [SerializeField] Image fillImage;

        void OnEnable()
        {
            if (health != null) health.OnHealthChanged.AddListener(HandleHealthChanged);
        }

        void OnDisable()
        {
            if (health != null) health.OnHealthChanged.RemoveListener(HandleHealthChanged);
        }

        void Start()
        {
            // Health.Awake() isn't guaranteed to run before this object's OnEnable() since
            // they're on different GameObjects — Start() is guaranteed to run after every
            // object's Awake(), so the initial sync belongs here instead.
            if (health != null) HandleHealthChanged(health.Current, health.MaxHealth);
        }

        void HandleHealthChanged(float current, float max)
        {
            if (fillImage != null) fillImage.fillAmount = max > 0f ? current / max : 0f;
        }
    }
}
