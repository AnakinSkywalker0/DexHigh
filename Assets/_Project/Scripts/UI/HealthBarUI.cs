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
            if (health == null) return;
            health.OnHealthChanged.AddListener(HandleHealthChanged);
            HandleHealthChanged(health.Current, health.MaxHealth);
        }

        void OnDisable()
        {
            if (health != null) health.OnHealthChanged.RemoveListener(HandleHealthChanged);
        }

        void HandleHealthChanged(float current, float max)
        {
            if (fillImage != null) fillImage.fillAmount = max > 0f ? current / max : 0f;
        }
    }
}
