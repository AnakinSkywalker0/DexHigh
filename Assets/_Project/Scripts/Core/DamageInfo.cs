using UnityEngine;

namespace DexHigh.Core
{
    public readonly struct DamageInfo
    {
        public readonly float Amount;
        public readonly Vector3 HitPoint;
        public readonly Vector3 HitDirection;
        public readonly GameObject Source;

        public DamageInfo(float amount, Vector3 hitPoint, Vector3 hitDirection, GameObject source)
        {
            Amount = amount;
            HitPoint = hitPoint;
            HitDirection = hitDirection;
            Source = source;
        }
    }
}
