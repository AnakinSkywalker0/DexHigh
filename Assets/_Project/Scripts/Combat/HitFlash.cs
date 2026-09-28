using System.Collections;
using UnityEngine;

namespace DexHigh.Combat
{
    public class HitFlash : MonoBehaviour
    {
        [SerializeField] Renderer[] renderers;
        [SerializeField] Color flashColor = Color.white;
        [SerializeField] float flashDuration = 0.12f;

        MaterialPropertyBlock block;
        Coroutine running;

        void Awake()
        {
            block = new MaterialPropertyBlock();
            if (renderers == null || renderers.Length == 0)
                renderers = GetComponentsInChildren<Renderer>();
        }

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
