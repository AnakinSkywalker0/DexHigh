using UnityEngine;
using DexHigh.Core;

namespace DexHigh.Combat
{
    public class DamagePopupSpawner : MonoBehaviour
    {
        [SerializeField] Health health;
        [SerializeField] GameObject popupPrefab;

        void Reset()
        {
            health = GetComponent<Health>();
        }

        void OnEnable()
        {
            if (health == null) health = GetComponent<Health>();
            if (health != null) health.OnDamaged.AddListener(HandleDamaged);
        }

        void OnDisable()
        {
            if (health != null) health.OnDamaged.RemoveListener(HandleDamaged);
        }

        void HandleDamaged(DamageInfo info)
        {
            if (popupPrefab == null) return;

            var popup = Instantiate(popupPrefab, info.HitPoint + Vector3.up * 0.25f, Quaternion.identity);
            if (popup.TryGetComponent<DamagePopup>(out var damagePopup))
                damagePopup.Init(info.Amount);
        }
    }
}
