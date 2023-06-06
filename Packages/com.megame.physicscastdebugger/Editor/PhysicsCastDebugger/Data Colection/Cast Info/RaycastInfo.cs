using UnityEngine;

namespace PhysicsCastDebugger
{
    public class RaycastInfo : PhysicsCastInfo
    {
        public RaycastInfo(Vector3 origin, Vector3 direction, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction, string stackTrace, bool hasCollide, RaycastHit[] raycastHits)
            : base(origin, direction, maxDistance, layerMask, queryTriggerInteraction, stackTrace, hasCollide, raycastHits)
        {
        }
    }
}
