using UnityEngine;

namespace DexHigh.Cameras
{
    public class BattleCameraRig : MonoBehaviour
    {
        [SerializeField] Transform targetA;
        [SerializeField] Transform targetB;
        [SerializeField] float pitchAngle = 55f;
        [SerializeField] float baseDistance = 10f;
        [SerializeField] float distancePerSeparation = 1.1f;
        [SerializeField] float minDistance = 8f;
        [SerializeField] float maxDistance = 22f;
        [SerializeField] float followLerp = 6f;

        void LateUpdate()
        {
            if (targetA == null || targetB == null) return;

            Vector3 midpoint = (targetA.position + targetB.position) * 0.5f;
            float separation = Vector3.Distance(targetA.position, targetB.position);
            float distance = Mathf.Clamp(baseDistance + separation * distancePerSeparation, minDistance, maxDistance);

            Quaternion rot = Quaternion.Euler(pitchAngle, 0f, 0f);
            Vector3 desiredPosition = midpoint - rot * Vector3.forward * distance;

            transform.position = Vector3.Lerp(transform.position, desiredPosition, followLerp * Time.deltaTime);
            transform.rotation = rot;
        }
    }
}
