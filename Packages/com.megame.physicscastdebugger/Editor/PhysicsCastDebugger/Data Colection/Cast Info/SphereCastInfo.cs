using UnityEngine;

namespace PhysicsCastDebugger
{ 
    public class SphereCastInfo : PhysicsCastInfo
    {
        public readonly float radius;

        public SphereCastInfo(Vector3 origin, float radius, Vector3 direction, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction, string stackTrace, bool hasCollide, RaycastHit[] raycastHits)
            : base(origin, direction, maxDistance, layerMask, queryTriggerInteraction, stackTrace, hasCollide, raycastHits)
        {
            this.radius = radius;
        }

        public SphereCastInfo(Vector3 origin, float radius, Vector3 direction, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction, string stackTrace, bool hasCollide, Collider[] colliders)
            : base(origin, direction, maxDistance, layerMask, queryTriggerInteraction, stackTrace, hasCollide, colliders)
        {
            this.radius = radius;
        }
    }
}
