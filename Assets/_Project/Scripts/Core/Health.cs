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

        public UnityEvent<float, float> OnHealthChanged; // current, max
        public UnityEvent<DamageInfo> OnDamaged;
        public UnityEvent OnDeath;

        void Awake()
        {
            Current = maxHealth;
        }

        public void TakeDamage(DamageInfo damage)
        {
            if (IsDead) return;

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
