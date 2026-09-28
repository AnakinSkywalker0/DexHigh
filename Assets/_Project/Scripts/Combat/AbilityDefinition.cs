using UnityEngine;

namespace DexHigh.Combat
{
    public enum AbilityShape
    {
        Cone,    // ranged, filtered to a forward arc — the fire breath
        Sphere,  // short-range overlap centered on the caster — the tail attack
        LeapAOE, // caster leaps to a point in front of it, then AOE on landing — the fly attack
    }

    [CreateAssetMenu(menuName = "DexHigh/Ability Definition", fileName = "NewAbility")]
    public class AbilityDefinition : ScriptableObject
    {
        public string abilityName = "Ability";
        public Sprite icon;

        public float damage = 10f;
        public float cooldown = 3f;
        public float range = 3f;
        [Range(0f, 180f)] public float coneAngle = 60f; // Cone only

        public AbilityShape shape = AbilityShape.Sphere;
        public string animatorTrigger = "Attack1";

        [Tooltip("Delay from button press to the hit landing — lines the damage up with the animation's impact frame.")]
        public float castDelay = 0.3f;
        [Tooltip("LeapAOE only: total time from take-off to landing.")]
        public float leapDuration = 0.6f;

        public float knockbackForce = 4f;
        public GameObject vfxPrefab;
        public AudioClip sfxClip;
    }
}
