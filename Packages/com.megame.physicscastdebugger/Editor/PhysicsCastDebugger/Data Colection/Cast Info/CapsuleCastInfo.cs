using UnityEngine;

namespace PhysicsCastDebugger
{
    public class CapsuleCastInfo : PhysicsCastInfo
    {
        public readonly Vector3 point1;
        public readonly Vector3 point2;
        public readonly float radius;

        public CapsuleCastInfo(Vector3 point1, Vector3 point2, float radius, Vector3 direction, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction, string stackTrace, bool hasCollide, RaycastHit[] raycastHits)
            : base((point1 + point2) / 2, direction, maxDistance, layerMask, queryTriggerInteraction, stackTrace, hasCollide, raycastHits)
        {
            this.point1 = point1;
            this.point2 = point2;
            this.radius = radius;
        }

        public CapsuleCastInfo(Vector3 point1, Vector3 point2, float radius, Vector3 direction, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction, string stackTrace, bool hasCollide, Collider[] colliders)
            : base((point1 + point2) / 2, direction, maxDistance, layerMask, queryTriggerInteraction, stackTrace, hasCollide, colliders)
        {
            this.point1 = point1;
            this.point2 = point2;
            this.radius = radius;
        }
    }
}
