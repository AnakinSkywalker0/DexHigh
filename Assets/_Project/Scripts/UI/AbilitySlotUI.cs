using UnityEngine;
using UnityEngine.UI;
using DexHigh.Combat;

namespace DexHigh.UI
{
    public class AbilitySlotUI : MonoBehaviour
    {
        [SerializeField] AbilityController abilities;
        [SerializeField] int slot;
        [SerializeField] Image icon;
        [SerializeField] Image cooldownOverlay; // Image Type = Filled, used as the radial cooldown sweep

        void OnEnable()
        {
            if (abilities == null) return;

            abilities.OnCooldownChanged.AddListener(HandleCooldownChanged);

            var defs = abilities.Abilities;
            if (slot >= 0 && slot < defs.Length && defs[slot] != null && icon != null)
                icon.sprite = defs[slot].icon;
        }

        void OnDisable()
        {
            if (abilities != null) abilities.OnCooldownChanged.RemoveListener(HandleCooldownChanged);
        }

        void HandleCooldownChanged(int changedSlot, float remaining, float total)
        {
            if (changedSlot != slot || cooldownOverlay == null) return;
            cooldownOverlay.fillAmount = total > 0f ? remaining / total : 0f;
        }
    }
}
