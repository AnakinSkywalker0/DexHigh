using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DexHigh.Combat;
using DexHigh.Core;

namespace DexHigh.AI
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(AbilityController))]
    public class AIBrain : MonoBehaviour
    {
        enum State { Idle, Chase, Attack }

        [SerializeField] Transform target;
        [SerializeField] float attackRangeBuffer = 0.3f;
        [SerializeField] float decisionInterval = 0.2f;
        [SerializeField] float turnSpeedDegrees = 480f;
        [SerializeField] float startDelay = 3f;   // grace period so the player can get oriented before the AI engages

        [Header("Pacing")]
        [SerializeField] float minAttackGap = 1.0f;   // pause after an attack is randomised in this range,
        [SerializeField] float maxAttackGap = 2.4f;   // so the rhythm can't be memorised
        [SerializeField] float strafeRadius = 4.5f;   // circles the player at this distance while waiting to attack
        [SerializeField] float strafeRange = 10f;     // only strafes when the player is this close

        NavMeshAgent agent;
        AbilityController abilities;
        Knockback knockback;
        Health health;
        Health targetHealth;
        State state = State.Idle;
        float decisionTimer;
        float idleTimer;
        float nextAttackTime;
        int lastAbility = -1;
        bool castStarted;
        float strafeDirection = 1f;
        float strafeFlipTime;
        readonly List<int> candidates = new List<int>();

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            abilities = GetComponent<AbilityController>();
            knockback = GetComponent<Knockback>();
            health = GetComponent<Health>();
            agent.updateRotation = false; // AIBrain drives facing, so turns read cleanly against the ability range checks
        }

        void Start()
        {
            idleTimer = startDelay;
            strafeDirection = Random.value < 0.5f ? -1f : 1f;
        }

        public void SetTarget(Transform newTarget) => target = newTarget;

        void Update()
        {
            if (health != null && health.IsDead)
            {
                agent.isStopped = true;
                return;
            }

            if (target == null) return;
            if (targetHealth == null) target.TryGetComponent(out targetHealth);
            if (targetHealth != null && targetHealth.IsDead)   // match is over — stand down
            {
                agent.isStopped = true;
                return;
            }

            if (knockback != null && knockback.IsStaggered)
            {
                agent.isStopped = true;
                return;
            }

            decisionTimer -= Time.deltaTime;

            switch (state)
            {
                case State.Idle:
                    FaceTarget();
                    idleTimer -= Time.deltaTime;
                    if (idleTimer <= 0f)
                        state = State.Chase;
                    break;

                case State.Chase:
                    agent.isStopped = false;
                    bool waiting = Time.time < nextAttackTime;

                    // Between attacks, circle the player instead of standing still; otherwise close in.
                    if (waiting && DistanceToTarget() < strafeRange)
                        Strafe();
                    else
                        agent.SetDestination(target.position);
                    FaceTarget();

                    if (!waiting && decisionTimer <= 0f)
                    {
                        decisionTimer = decisionInterval;
                        if (PickAbility() >= 0)
                            state = State.Attack;
                    }
                    break;

                case State.Attack:
                    agent.isStopped = true;
                    FaceTarget();

                    if (abilities.IsCasting) break;

                    if (castStarted)
                    {
                        // Cast finished: back to circling / chasing until the next randomised gap ends.
                        castStarted = false;
                        state = State.Chase;
                        break;
                    }

                    int slot = PickAbility();
                    if (slot >= 0 && abilities.TryUse(slot))
                    {
                        castStarted = true;
                        lastAbility = slot;
                        nextAttackTime = Time.time + Random.Range(minAttackGap, maxAttackGap);
                    }
                    else
                    {
                        state = State.Chase;
                    }
                    break;
            }
        }

        float DistanceToTarget() => Vector3.Distance(transform.position, target.position);

        void Strafe()
        {
            if (Time.time >= strafeFlipTime)
            {
                if (Random.value < 0.4f) strafeDirection = -strafeDirection;
                strafeFlipTime = Time.time + Random.Range(1.2f, 2.6f);
            }

            Vector3 away = transform.position - target.position;
            away.y = 0f;
            away = away.sqrMagnitude < 0.01f ? -transform.forward : away.normalized;

            Vector3 around = Quaternion.Euler(0f, strafeDirection * 55f, 0f) * away;
            agent.SetDestination(target.position + around * strafeRadius);
        }

        void FaceTarget()
        {
            Vector3 dir = target.position - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f) return;

            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeedDegrees * Time.deltaTime);
        }

        // Picks at random among abilities that are off cooldown and in range, avoiding an immediate repeat when there's a choice.
        int PickAbility()
        {
            float distance = DistanceToTarget();
            var defs = abilities.Abilities;

            candidates.Clear();
            for (int i = 0; i < defs.Length; i++)
            {
                var def = defs[i];
                if (def == null || abilities.IsOnCooldown(i)) continue;
                if (distance <= def.range + attackRangeBuffer) candidates.Add(i);
            }

            if (candidates.Count == 0) return -1;
            if (candidates.Count > 1) candidates.Remove(lastAbility);

            return candidates[Random.Range(0, candidates.Count)];
        }
    }
}
