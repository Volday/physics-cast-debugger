using UnityEngine;

namespace PhysicsCastDebugger
{
    public class BoxCastInfo : PhysicsCastInfo
    {
        public readonly Vector3 halfExtents;
        public readonly Quaternion orientation;

        public BoxCastInfo(Vector3 origin, Vector3 halfExtents, Vector3 direction, Quaternion orientation, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction, string stackTrace, bool hasCollide, RaycastHit[] raycastHits)
            : base(origin, direction, maxDistance, layerMask, queryTriggerInteraction, stackTrace, hasCollide, raycastHits)
        {
            this.halfExtents = halfExtents;
            this.orientation = orientation;
        }

        public BoxCastInfo(Vector3 origin, Vector3 halfExtents, Vector3 direction, Quaternion orientation, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction, string stackTrace, bool hasCollide, Collider[] colliders)
           : base(origin, direction, maxDistance, layerMask, queryTriggerInteraction, stackTrace, hasCollide, colliders)
        {
            this.halfExtents = halfExtents;
            this.orientation = orientation;
        }
    }
}
