using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DexHigh.Core;
using DexHigh.Systems;

namespace DexHigh.Combat
{
    public class AbilityController : MonoBehaviour
    {
        [SerializeField] AbilityDefinition[] abilities = new AbilityDefinition[3];
        [SerializeField] LayerMask targetLayers = ~0;
        [SerializeField, Min(0f)] float damageMultiplier = 1f; // difficulty knob: scales every ability this dragon uses

        Animator animator;
        float[] cooldownRemaining;

        public AbilityDefinition[] Abilities => abilities;
        public bool IsCasting { get; private set; }

        public UnityEvent<int, float, float> OnCooldownChanged; // slot, remaining, total
        public UnityEvent<int> OnAbilityUsed;

        void Awake()
        {
            animator = GetComponentInChildren<Animator>(); // lives on the model child
            cooldownRemaining = new float[abilities.Length];
        }

        void Update()
        {
            for (int i = 0; i < cooldownRemaining.Length; i++)
            {
                if (cooldownRemaining[i] <= 0f) continue;

                cooldownRemaining[i] = Mathf.Max(0f, cooldownRemaining[i] - Time.deltaTime);
                var def = abilities[i];
                OnCooldownChanged?.Invoke(i, cooldownRemaining[i], def != null ? def.cooldown : 1f);
            }
        }

        public bool IsOnCooldown(int slot) => slot >= 0 && slot < cooldownRemaining.Length && cooldownRemaining[slot] > 0f;

        public bool CanUse(int slot)
        {
            if (IsCasting) return false;
            if (slot < 0 || slot >= abilities.Length || abilities[slot] == null) return false;
            return !IsOnCooldown(slot);
        }

        public bool TryUse(int slot)
        {
            if (!CanUse(slot)) return false;

            var def = abilities[slot];
            cooldownRemaining[slot] = def.cooldown;
            OnCooldownChanged?.Invoke(slot, def.cooldown, def.cooldown);
            OnAbilityUsed?.Invoke(slot);

            if (animator != null && !string.IsNullOrEmpty(def.animatorTrigger))
                animator.SetTrigger(def.animatorTrigger);

            StartCoroutine(RunAbility(def));
            return true;
        }

        IEnumerator RunAbility(AbilityDefinition def)
        {
            IsCasting = true;

            if (def.shape == AbilityShape.LeapAOE)
            {
                yield return StartCoroutine(RunLeap(def));
            }
            else
            {
                if (def.castDelay > 0f)
                    yield return new WaitForSeconds(def.castDelay);
                ApplyHit(def, transform.position, transform.forward);
            }

            IsCasting = false;
        }

        IEnumerator RunLeap(AbilityDefinition def)
        {
            Vector3 start = transform.position;
            Vector3 forward = transform.forward;
            Vector3 landing = start + forward * def.range;
            Vector3 apex = Vector3.Lerp(start, landing, 0.5f) + Vector3.up * 2.5f;

            float half = Mathf.Max(0.01f, def.leapDuration * 0.5f);

            float t = 0f;
            while (t < half)
            {
                t += Time.deltaTime;
                transform.position = Vector3.Lerp(start, apex, t / half);
                yield return null;
            }

            t = 0f;
            while (t < half)
            {
                t += Time.deltaTime;
                transform.position = Vector3.Lerp(apex, landing, t / half);
                yield return null;
            }

            transform.position = landing;
            ApplyHit(def, landing, forward);
        }

        void ApplyHit(AbilityDefinition def, Vector3 origin, Vector3 forward)
        {
            if (def.vfxPrefab != null)
            {
                Quaternion rot = forward.sqrMagnitude > 0.0001f ? Quaternion.LookRotation(forward) : Quaternion.identity;
                var vfx = Instantiate(def.vfxPrefab, origin + Vector3.up, rot);
                Destroy(vfx, def.vfxLifetime);
            }

            if (def.sfxClip != null)
                Sfx.Play2D(def.sfxClip, 0.8f, 0.04f);

            float radius = def.shape switch
            {
                AbilityShape.Cone => def.range,
                AbilityShape.LeapAOE => def.range * 0.5f,
                _ => def.range, // Sphere (melee) — range doubles as the melee radius
            };

            Collider[] hits = Physics.OverlapSphere(origin, radius, targetLayers);

            // A dragon has several colliders (body + CharacterController); count each target once per hit.
            var alreadyHit = new HashSet<Transform>();

            foreach (var hit in hits)
            {
                Transform targetRoot = hit.transform.root;
                if (targetRoot == transform.root) continue;
                if (targetRoot.TryGetComponent<Health>(out var targetHealth) && targetHealth.IsInvulnerable) continue; // dashed through it
                if (!alreadyHit.Add(targetRoot)) continue;

                if (def.shape == AbilityShape.Cone)
                {
                    Vector3 toTarget = hit.transform.position - origin;
                    toTarget.y = 0f;
                    if (toTarget.sqrMagnitude > 0.001f && Vector3.Angle(forward, toTarget.normalized) > def.coneAngle * 0.5f)
                        continue;
                }

                Vector3 dir = (hit.transform.position - origin).normalized;

                if (hit.transform.root.TryGetComponent<IDamageable>(out var damageable))
                    damageable.TakeDamage(new DamageInfo(def.damage * damageMultiplier, hit.ClosestPoint(origin), dir, gameObject));

                if (def.knockbackForce > 0f && hit.transform.root.TryGetComponent<Knockback>(out var knockback))
                {
                    // Flatten to the horizontal plane — at close range the raw hit direction is
                    // dominated by the small vertical offset between the origin point and the
                    // target's collider surface, which launched targets almost straight up.
                    Vector3 knockDir = hit.transform.position - origin;
                    knockDir.y = 0f;
                    knockDir = knockDir.sqrMagnitude > 0.0001f ? knockDir.normalized : forward;
                    knockback.ApplyKnockback(knockDir * def.knockbackForce);
                }
            }
        }
    }
}
