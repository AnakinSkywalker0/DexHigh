using UnityEngine;
using TMPro;

namespace DexHigh.Combat
{
    public class DamagePopup : MonoBehaviour
    {
        [SerializeField] TMP_Text label;
        [SerializeField] float floatSpeed = 1.2f;
        [SerializeField] float lifetime = 0.8f;

        float timer;
        Color startColor;

        public void Init(float amount)
        {
            if (label == null) label = GetComponentInChildren<TMP_Text>();
            if (label == null) return;

            label.text = Mathf.RoundToInt(amount).ToString();
            startColor = label.color;
        }

        void Update()
        {
            transform.position += Vector3.up * floatSpeed * Time.deltaTime;

            if (Camera.main != null)
                transform.rotation = Camera.main.transform.rotation;

            timer += Time.deltaTime;

            if (label != null)
            {
                float alpha = Mathf.Lerp(1f, 0f, timer / lifetime);
                label.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            }

            if (timer >= lifetime)
                Destroy(gameObject);
        }
    }
}
