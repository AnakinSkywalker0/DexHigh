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

        NavMeshAgent agent;
        AbilityController abilities;
        Knockback knockback;
        Health health;
        State state = State.Idle;
        float decisionTimer;

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
            state = State.Chase;
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

            if (knockback != null && knockback.IsStaggered)
            {
                agent.isStopped = true;
                return;
            }

            decisionTimer -= Time.deltaTime;

            switch (state)
            {
                case State.Idle:
                    state = State.Chase;
                    break;

                case State.Chase:
                    agent.isStopped = false;
                    agent.SetDestination(target.position);
                    FaceTarget();

                    if (decisionTimer <= 0f)
                    {
                        decisionTimer = decisionInterval;
                        if (PickAbility() >= 0)
                            state = State.Attack;
                    }
                    break;

                case State.Attack:
                    agent.isStopped = true;
                    FaceTarget();

                    if (!abilities.IsCasting)
                    {
                        int slot = PickAbility();
                        if (slot < 0 || !abilities.TryUse(slot))
                            state = State.Chase;
                    }
                    break;
            }
        }

        void FaceTarget()
        {
            Vector3 dir = target.position - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f) return;

            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeedDegrees * Time.deltaTime);
        }

        int PickAbility()
        {
            float distance = Vector3.Distance(transform.position, target.position);
            var defs = abilities.Abilities;

            int best = -1;
            float bestRange = float.MaxValue;

            for (int i = 0; i < defs.Length; i++)
            {
                var def = defs[i];
                if (def == null || abilities.IsOnCooldown(i)) continue;

                float usableRange = def.range + attackRangeBuffer;
                if (distance <= usableRange && def.range < bestRange)
                {
                    best = i;
                    bestRange = def.range;
                }
            }

            return best;
        }
    }
}
