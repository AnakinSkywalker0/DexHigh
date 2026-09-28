using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace DexHigh.Combat
{
    [DisallowMultipleComponent]
    public class Knockback : MonoBehaviour
    {
        [SerializeField] float duration = 0.18f;

        CharacterController controller;
        NavMeshAgent agent;
        Coroutine running;

        public bool IsStaggered { get; private set; }

        void Awake()
        {
            controller = GetComponent<CharacterController>();
            agent = GetComponent<NavMeshAgent>();
        }

        public void ApplyKnockback(Vector3 velocity)
        {
            if (running != null) StopCoroutine(running);
            running = StartCoroutine(Run(velocity));
        }

        IEnumerator Run(Vector3 velocity)
        {
            IsStaggered = true;
            bool hadAgentControl = agent != null && agent.enabled;
            if (hadAgentControl) agent.updatePosition = false;

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                Vector3 step = velocity * (1f - t / duration) * Time.deltaTime;

                if (controller != null && controller.enabled)
                    controller.Move(step);
                else
                    transform.position += step;

                if (hadAgentControl)
                    agent.nextPosition = transform.position;

                yield return null;
            }

            if (hadAgentControl)
            {
                agent.updatePosition = true;
                agent.nextPosition = transform.position;
            }

            IsStaggered = false;
            running = null;
        }
    }
}
