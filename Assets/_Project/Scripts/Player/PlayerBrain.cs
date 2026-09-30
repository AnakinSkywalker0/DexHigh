using UnityEngine;
using UnityEngine.InputSystem;
using DexHigh.Combat;
using DexHigh.Core;
using DexHigh.Systems;

namespace DexHigh.Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(AbilityController))]
    public class PlayerBrain : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] float moveSpeed = 6f;
        [SerializeField] float acceleration = 45f;   // units/s² — speeds up and slows down instead of snapping
        [SerializeField] float rotationSpeed = 720f;
        [SerializeField] float gravity = -20f;

        [Header("Dash (Space / Shift)")]
        [SerializeField] float dashSpeed = 11.7f;
        [SerializeField] float dashDuration = 0.215f;
        [SerializeField] float dashCooldown = 1.1f;
        [SerializeField] float dashInvulnerability = 0.3f;   // dodge window covers the whole dash
        [SerializeField] AudioClip dashSound;

        [Header("Lock-on")]
        [SerializeField] Transform lockTarget;      // auto-found if left empty
        [SerializeField] GameObject lockMarkerPrefab;
        [SerializeField] bool lockedOn = true;

        CharacterController controller;
        AbilityController abilities;
        Knockback knockback;
        Health health;
        float verticalVelocity;
        Vector3 planarVelocity;
        Vector3 dashDirection;
        float dashEndTime;
        float nextDashTime;
        GameObject lockMarker;
        Health targetHealth;
        GUIStyle hintStyle;

        void Awake()
        {
            controller = GetComponent<CharacterController>();
            abilities = GetComponent<AbilityController>();
            knockback = GetComponent<Knockback>();
            health = GetComponent<Health>();
        }

        void Start()
        {
            if (lockTarget == null)
            {
                var ai = FindFirstObjectByType<DexHigh.AI.AIBrain>();
                if (ai != null) lockTarget = ai.transform;
            }

            if (lockTarget != null)
            {
                targetHealth = lockTarget.GetComponent<Health>();
                if (lockMarkerPrefab != null)
                {
                    lockMarker = Instantiate(lockMarkerPrefab, lockTarget);
                    lockMarker.transform.localPosition = new Vector3(0f, 0.05f, 0f);
                }
            }
            RefreshMarker();
        }

        bool HasLock => lockedOn && lockTarget != null && (targetHealth == null || !targetHealth.IsDead);
        bool IsDashing => Time.time < dashEndTime;

        void RefreshMarker()
        {
            if (lockMarker != null) lockMarker.SetActive(HasLock);
        }

        void Update()
        {
            var kb = Keyboard.current;

            if (kb.tabKey.wasPressedThisFrame)
            {
                lockedOn = !lockedOn;
                RefreshMarker();
            }
            if (lockMarker != null && lockMarker.activeSelf != HasLock) RefreshMarker();

            if (health != null && health.IsDead) return;
            if (knockback != null && knockback.IsStaggered) { planarVelocity = Vector3.zero; return; }

            if (kb.spaceKey.wasPressedThisFrame || kb.leftShiftKey.wasPressedThisFrame) TryDash();

            HandleMovement();
            HandleAbilities();
        }

        Vector3 ReadMoveInput()
        {
            var kb = Keyboard.current;
            float h = (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f);
            float v = (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f);
            Vector3 input = new Vector3(h, 0f, v);
            return input.sqrMagnitude > 1f ? input.normalized : input;
        }

        void TryDash()
        {
            if (Time.time < nextDashTime || (abilities != null && abilities.IsCasting)) return;

            Vector3 dir = ReadMoveInput();
            if (dir.sqrMagnitude < 0.01f)
            {
                // No direction held: back away from the enemy (or from where we're facing).
                dir = HasLock ? transform.position - lockTarget.position : -transform.forward;
                dir.y = 0f;
            }

            dashDirection = dir.normalized;
            dashEndTime = Time.time + dashDuration;
            nextDashTime = Time.time + dashCooldown;
            if (health != null) health.GrantInvulnerability(dashInvulnerability);
            Sfx.Play2D(dashSound, 0.7f);
        }

        void HandleMovement()
        {
            Vector3 input = ReadMoveInput();
            bool casting = abilities != null && abilities.IsCasting;

            if (!casting)
            {
                // Locked on: always face the enemy so abilities land; free: face the move direction.
                Vector3 facing = input;
                if (HasLock)
                {
                    facing = lockTarget.position - transform.position;
                    facing.y = 0f;
                }

                if (facing.sqrMagnitude > 0.0001f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(facing, Vector3.up);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
                }
            }

            if (IsDashing)
                planarVelocity = dashDirection * dashSpeed;
            else
            {
                // Dash just ended: drop back to walking pace so it doesn't slide on afterwards.
                if (planarVelocity.magnitude > moveSpeed)
                    planarVelocity = planarVelocity.normalized * moveSpeed;

                Vector3 desired = casting ? Vector3.zero : input * moveSpeed;
                planarVelocity = Vector3.MoveTowards(planarVelocity, desired, acceleration * Time.deltaTime);
            }

            if (controller.isGrounded && verticalVelocity < 0f)
                verticalVelocity = -1f;
            verticalVelocity += gravity * Time.deltaTime;

            Vector3 motion = planarVelocity;
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

        void OnGUI()
        {
            if (hintStyle == null)
                hintStyle = new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Bold, normal = { textColor = Color.white } };

            float dashLeft = Mathf.Max(0f, nextDashTime - Time.time);
            string dash = dashLeft > 0f ? $"Dash {dashLeft:0.0}s" : "Dash READY";
            GUI.Label(new Rect(16f, Screen.height - 40f, 900f, 30f),
                $"WASD move   1/2/3 abilities   Space/Shift: {dash}   Tab: Lock-on [{(lockedOn ? "ON" : "OFF")}]", hintStyle);
        }
    }
}
