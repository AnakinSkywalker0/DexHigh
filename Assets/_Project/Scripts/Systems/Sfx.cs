using UnityEngine;

namespace DexHigh.Systems
{
    // Fire-and-forget 2D one-shots. The camera sits ~18 units above the arena, so 3D-positioned
    // sounds (PlayClipAtPoint) would be heard very quietly; this plays them at full volume instead.
    public static class Sfx
    {
        public static void Play2D(AudioClip clip, float volume = 1f, float pitchVariance = 0.06f)
        {
            if (clip == null) return;

            var go = new GameObject("Sfx_" + clip.name);
            var src = go.AddComponent<AudioSource>();
            src.clip = clip;
            src.volume = volume;
            src.spatialBlend = 0f;
            src.pitch = 1f + Random.Range(-pitchVariance, pitchVariance);
            src.Play();
            Object.Destroy(go, clip.length / Mathf.Max(0.1f, src.pitch) + 0.1f);
        }
    }
}
