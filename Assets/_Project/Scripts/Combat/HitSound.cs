using UnityEngine;
using DexHigh.Core;
using DexHigh.Systems;

namespace DexHigh.Combat
{
    // Plays an impact sound whenever this dragon takes damage.
    public class HitSound : MonoBehaviour
    {
        [SerializeField] AudioClip[] clips;
        [SerializeField, Range(0f, 1f)] float volume = 0.9f;

        Health health;

        void Awake() => health = GetComponent<Health>();

        void OnEnable()
        {
            if (health != null) health.OnDamaged.AddListener(HandleDamaged);
        }

        void OnDisable()
        {
            if (health != null) health.OnDamaged.RemoveListener(HandleDamaged);
        }

        void HandleDamaged(DamageInfo info)
        {
            if (clips == null || clips.Length == 0) return;
            Sfx.Play2D(clips[Random.Range(0, clips.Length)], volume);
        }
    }
}
