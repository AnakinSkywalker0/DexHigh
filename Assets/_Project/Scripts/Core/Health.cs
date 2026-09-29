using UnityEngine;
using UnityEngine.Events;

namespace DexHigh.Core
{
    public class Health : MonoBehaviour, IDamageable
    {
        [SerializeField] float maxHealth = 100f;

        public float MaxHealth => maxHealth;
        public float Current { get; private set; }
        public bool IsDead { get; private set; }
        public bool IsInvulnerable => Time.time < invulnerableUntil;

        float invulnerableUntil;

        public UnityEvent<float, float> OnHealthChanged; // current, max
        public UnityEvent<DamageInfo> OnDamaged;
        public UnityEvent OnDeath;

        void Awake()
        {
            Current = maxHealth;
        }

        public void GrantInvulnerability(float seconds) => invulnerableUntil = Mathf.Max(invulnerableUntil, Time.time + seconds);

        public void TakeDamage(DamageInfo damage)
        {
            if (IsDead || IsInvulnerable) return;

            Current = Mathf.Max(0f, Current - damage.Amount);
            OnHealthChanged?.Invoke(Current, maxHealth);
            OnDamaged?.Invoke(damage);

            if (Current <= 0f)
            {
                IsDead = true;
                OnDeath?.Invoke();
            }
        }
    }
}
