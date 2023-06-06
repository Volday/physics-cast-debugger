using UnityEngine;
using System.Linq;
using System;

namespace PhysicsCastDebugger
{
    public class PhysicsCastInfo
    {
        public readonly Vector3 origin;
        public readonly Vector3 direction;
        public readonly float maxDistance;
        public readonly int layerMask;
        public readonly QueryTriggerInteraction queryTriggerInteraction;
        public readonly string stackTrace;
        public readonly bool isCollided;
        public readonly RaycastHit[] raycastHits;
        public readonly Collider[] colliders;

        public readonly string callSourceName;
        public readonly DateTime castTime;

        public PhysicsCastInfo(Vector3 origin, Vector3 direction, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction, string stackTrace, bool hasCollided, RaycastHit[] raycastHits)
            :this(origin, direction, maxDistance, layerMask, queryTriggerInteraction, stackTrace, hasCollided)
        {
            this.raycastHits = raycastHits;
        }

        public PhysicsCastInfo(Vector3 origin, Vector3 direction, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction, string stackTrace, bool hasCollided, Collider[] colliders)
            : this(origin, direction, maxDistance, layerMask, queryTriggerInteraction, stackTrace, hasCollided)
        {
            this.colliders = colliders;
        }

        public PhysicsCastInfo(Vector3 origin, Vector3 direction, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction, string stackTrace, bool hasCollided)
        {
            const string INGNORE_SOURCE = "UnityEngine.Physics";

            this.origin = origin;
            this.direction = direction;
            this.maxDistance = maxDistance;
            this.layerMask = layerMask;
            this.queryTriggerInteraction = queryTriggerInteraction;
            this.stackTrace = stackTrace;
            this.isCollided = hasCollided;

            if (this.raycastHits is null)
                this.raycastHits = new RaycastHit[0];

            var callSourceLine = "Unknown";
            var stackTraceLines = stackTrace.Split('\n');
            for (int i = 0; i < stackTraceLines.Length; i++)
            {
                if (stackTraceLines[i].Length < INGNORE_SOURCE.Length || stackTraceLines[i].Substring(0, INGNORE_SOURCE.Length) != INGNORE_SOURCE)
                {
                    callSourceLine = stackTraceLines[i];
                    break;
                }
            }

            callSourceName = new string(callSourceLine
                .TakeWhile(t => t != '\n' && t != '/' && t != ':' && t != ' ' && t != '.')
                .ToArray());

            castTime = DateTime.UtcNow;
        }
    }
}
