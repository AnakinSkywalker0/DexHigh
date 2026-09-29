using UnityEngine;
using DexHigh.Core;

namespace DexHigh.Combat
{
    // Drives the model's locomotion / hit / death animations. Ability animations are triggered by AbilityController.
    public class DragonAnimator : MonoBehaviour
    {
        static readonly int SpeedHash = Animator.StringToHash("Speed");
        static readonly int HitHash = Animator.StringToHash("Hit");
        static readonly int DeathHash = Animator.StringToHash("Death");

        Animator animator;
        Health health;
        Vector3 lastPosition;

        void Awake()
        {
            animator = GetComponentInChildren<Animator>();
            health = GetComponent<Health>();
            lastPosition = transform.position;
        }

        void OnEnable()
        {
            if (health == null) return;
            health.OnDamaged.AddListener(HandleDamaged);
            health.OnDeath.AddListener(HandleDeath);
        }

        void OnDisable()
        {
            if (health == null) return;
            health.OnDamaged.RemoveListener(HandleDamaged);
            health.OnDeath.RemoveListener(HandleDeath);
        }

        void Update()
        {
            if (animator == null) return;

            // Works for both the CharacterController player and the NavMeshAgent enemy.
            Vector3 delta = transform.position - lastPosition;
            delta.y = 0f;
            float speed = Time.deltaTime > 0f ? delta.magnitude / Time.deltaTime : 0f;
            lastPosition = transform.position;

            animator.SetFloat(SpeedHash, speed, 0.1f, Time.deltaTime);
        }

        void HandleDamaged(DamageInfo info)
        {
            if (animator != null && !health.IsDead) animator.SetTrigger(HitHash);
        }

        void HandleDeath()
        {
            if (animator != null) animator.SetTrigger(DeathHash);
        }
    }
}
