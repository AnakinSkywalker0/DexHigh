using System.Collections;
using UnityEngine;
using DexHigh.Core;

namespace DexHigh.Combat
{
    public class HitFlash : MonoBehaviour
    {
        [SerializeField] Renderer[] renderers;
        [SerializeField] Color flashColor = new Color(1f, 0.15f, 0.15f);
        [SerializeField] float flashDuration = 0.2f;

        MaterialPropertyBlock block;
        Coroutine running;
        Health health;

        void Awake()
        {
            block = new MaterialPropertyBlock();
            if (renderers == null || renderers.Length == 0)
                renderers = GetComponentsInChildren<Renderer>();
            health = GetComponent<Health>();
        }

        void OnEnable()
        {
            if (health != null) health.OnDamaged.AddListener(HandleDamaged);
        }

        void OnDisable()
        {
            if (health != null) health.OnDamaged.RemoveListener(HandleDamaged);
        }

        void HandleDamaged(DamageInfo info) => Flash();

        public void Flash()
        {
            if (running != null) StopCoroutine(running);
            running = StartCoroutine(FlashRoutine());
        }

        IEnumerator FlashRoutine()
        {
            SetFlash(true);
            yield return new WaitForSeconds(flashDuration);
            SetFlash(false);
            running = null;
        }

        void SetFlash(bool on)
        {
            foreach (var r in renderers)
            {
                if (r == null) continue;

                if (on)
                {
                    block.SetColor("_BaseColor", flashColor);
                    block.SetColor("_EmissionColor", flashColor * 2f); // pops even on dark textures
                    r.SetPropertyBlock(block);
                }
                else
                {
                    r.SetPropertyBlock(null);
                }
            }
        }
    }
}
