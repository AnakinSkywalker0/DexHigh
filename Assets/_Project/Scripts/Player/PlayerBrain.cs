using UnityEngine;
using UnityEngine.InputSystem;
using DexHigh.Combat;
using DexHigh.Core;

namespace DexHigh.Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(AbilityController))]
    public class PlayerBrain : MonoBehaviour
    {
        [SerializeField] float moveSpeed = 5f;
        [SerializeField] float rotationSpeed = 720f;
        [SerializeField] float gravity = -20f;

        CharacterController controller;
        AbilityController abilities;
        Knockback knockback;
        Health health;
        float verticalVelocity;

        void Awake()
        {
            controller = GetComponent<CharacterController>();
            abilities = GetComponent<AbilityController>();
            knockback = GetComponent<Knockback>();
            health = GetComponent<Health>();
        }

        void Update()
        {
            if (health != null && health.IsDead) return;
            if (knockback != null && knockback.IsStaggered) return;

            HandleMovement();
            HandleAbilities();
        }

        void HandleMovement()
        {
            var kb = Keyboard.current;
            float h = (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f);
            float v = (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f);

            Vector3 input = new Vector3(h, 0f, v);
            if (input.sqrMagnitude > 1f) input.Normalize();

            bool casting = abilities != null && abilities.IsCasting;

            if (input.sqrMagnitude > 0.0001f && !casting)
            {
                Quaternion targetRot = Quaternion.LookRotation(input, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }

            Vector3 motion = casting ? Vector3.zero : input * moveSpeed;

            if (controller.isGrounded && verticalVelocity < 0f)
                verticalVelocity = -1f;
            verticalVelocity += gravity * Time.deltaTime;
            motion.y = verticalVelocity;

            controller.Move(motion * Time.deltaTime);
        }

        void HandleAbilities()
        {
            if (abilities == null) return;

            var kb = Keyboard.current;
            if (kb.digit1Key.wasPressedThisFrame) abilities.TryUse(0);
            else if (kb.digit2Key.wasPressedThisFrame) abilities.TryUse(1);
            else if (kb.digit3Key.wasPressedThisFrame) abilities.TryUse(2);
        }
    }
}
