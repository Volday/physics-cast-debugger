using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PhysicsCastSimulation : MonoBehaviour
{
    public float angleDeviation = 15;

    public Ray GetRandomRay()
    {
        var rotation = Quaternion.Euler(Random.Range(-angleDeviation, angleDeviation), Random.Range(-angleDeviation, angleDeviation), Random.Range(-angleDeviation, angleDeviation));
        var direction = rotation * transform.forward;
        return new Ray(transform.position, direction);
    }

    public Quaternion GetRandomRaotation()
    {
        return Quaternion.Euler(Random.Range(-180f, 180f), Random.Range(-180f, 180f), Random.Range(-180f, 180f));
    }

    public Vector3 GetRandomHalfExtends()
    {
        return new Vector3(Random.Range(0.25f, 1f), Random.Range(0.25f, 1f), Random.Range(0.25f, 1f));
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(PhysicsCastSimulation))]
    class PhysicsCastSimulationEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var pcs = target as PhysicsCastSimulation;

            if (GUILayout.Button("RayCast"))
            {
                Physics.Raycast(pcs.GetRandomRay(), out RaycastHit hit);
            }

            if (GUILayout.Button("RayCastAll"))
            {
                Physics.RaycastAll(pcs.GetRandomRay());
            }

            if (GUILayout.Button("RayCastAll"))
            {
                RaycastHit[] raycastHits = new RaycastHit[10];
                Physics.RaycastNonAlloc(pcs.GetRandomRay(), raycastHits);
            }

            EditorGUILayout.Separator();

            if (GUILayout.Button("SphereCast"))
            {
                Physics.SphereCast(pcs.GetRandomRay(), Random.Range(0.5f, 1f), out RaycastHit hit);
            }

            if (GUILayout.Button("SphereCastAll"))
            {
                Physics.SphereCastAll(pcs.GetRandomRay(), Random.Range(0.5f, 1f));
            }

            if (GUILayout.Button("SphereCastNonAlloc"))
            {
                RaycastHit[] raycastHits = new RaycastHit[10];
                Physics.SphereCastNonAlloc(pcs.GetRandomRay(), Random.Range(0.5f, 1f), raycastHits);
            }

            EditorGUILayout.Separator();

            if (GUILayout.Button("BoxCast"))
            {
                var ray = pcs.GetRandomRay();
                Physics.BoxCast(ray.origin, pcs.GetRandomHalfExtends(), ray.direction, pcs.GetRandomRaotation());
            }

            if (GUILayout.Button("BoxCastAll"))
            {
                var ray = pcs.GetRandomRay();
                Physics.BoxCastAll(ray.origin, pcs.GetRandomHalfExtends(), ray.direction, pcs.GetRandomRaotation());
            }

            if (GUILayout.Button("BoxCastNonAlloc"))
            {
                var ray = pcs.GetRandomRay();
                RaycastHit[] raycastHits = new RaycastHit[10];
                Physics.BoxCastNonAlloc(ray.origin, pcs.GetRandomHalfExtends(), ray.direction, raycastHits, pcs.GetRandomRaotation());
            }
        }
    }
#endif
}
