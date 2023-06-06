using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using UnityEditor;
using System.Reflection;
using System;
using System.Linq;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace PhysicsCastDebugger
{
    public class RaycastCallData
    {
        public static Action<PhysicsCastInfo> onPhysicsCastCall;

        private static MethodInfo method_StacktraceWithHyperlinks;

        [InitializeOnLoadMethod]
        public static void DoPatching()
        {
            var harmony = new Harmony("com.example.patch");

            var type_PhysicsScene = typeof(PhysicsScene);
            var type_Physics = typeof(Physics);
            var type_RaycastCallData = typeof(RaycastCallData);
            var type_ConsoleWindow = Type.GetType("UnityEditor.ConsoleWindow, UnityEditor");

            method_StacktraceWithHyperlinks = type_ConsoleWindow?.GetMethod("StacktraceWithHyperlinks", BindingFlags.NonPublic | BindingFlags.Static);

            if (method_StacktraceWithHyperlinks == null)
            {
                Debug.LogWarning($"Iternal Unity code used by {typeof(RaycastCallData).Name} has been changed or removed");
                return;
            }

            #region Raycast

            var method_Raycast = type_PhysicsScene?.GetMethod(
                "Raycast",
                new Type[] {
                typeof(Vector3),
                typeof(Vector3),
                typeof(RaycastHit).MakeByRefType(),
                typeof(float),
                typeof(int),
                typeof(QueryTriggerInteraction),
                });
            var method_RaycastNonAlloc = type_PhysicsScene?.GetMethod(
                "Raycast",
                new Type[] {
                typeof(Vector3),
                typeof(Vector3),
                typeof(RaycastHit[]),
                typeof(float),
                typeof(int),
                typeof(QueryTriggerInteraction),
                });
            var method_RaycastTest = type_PhysicsScene?.GetMethod(
                "Raycast",
                new Type[] {
                typeof(Vector3),
                typeof(Vector3),
                typeof(float),
                typeof(int),
                typeof(QueryTriggerInteraction),
                });
            var method_RaycastAll = type_Physics?.GetMethod(
               "RaycastAll",
               new Type[] {
                typeof(Vector3),
                typeof(Vector3),
                typeof(float),
                typeof(int),
                typeof(QueryTriggerInteraction),
               });

            var postfix_Raycast = type_RaycastCallData.GetMethod("PostfixRaycast", BindingFlags.NonPublic | BindingFlags.Static);
            var postfix_RaycastNonAlloc = type_RaycastCallData.GetMethod("PostfixRaycastNonAlloc", BindingFlags.NonPublic | BindingFlags.Static);
            var postfix_RaycastTest = type_RaycastCallData.GetMethod("PostfixRaycastTest", BindingFlags.NonPublic | BindingFlags.Static);
            var postfix_RaycastAll = type_RaycastCallData.GetMethod("PostfixRaycastAll", BindingFlags.NonPublic | BindingFlags.Static);

            try
            {
                harmony.Patch(method_Raycast, null, new HarmonyMethod(postfix_Raycast));
                harmony.Patch(method_RaycastNonAlloc, null, new HarmonyMethod(postfix_RaycastNonAlloc));
                harmony.Patch(method_RaycastTest, null, new HarmonyMethod(postfix_RaycastTest));
                harmony.Patch(method_RaycastAll, null, new HarmonyMethod(postfix_RaycastAll));
            }
            catch
            {
                Debug.LogWarning($"Iternal Unity code used by {typeof(RaycastCallData).Name} has been changed or removed");
            }

            #endregion

            #region SphereCast

            var method_SphereCast = type_PhysicsScene?.GetMethod(
                "SphereCast",
                new Type[] {
                typeof(Vector3),
                typeof(float),
                typeof(Vector3),
                typeof(RaycastHit).MakeByRefType(),
                typeof(float),
                typeof(int),
                typeof(QueryTriggerInteraction),
                });
            var method_SphereCastNonAlloc = type_PhysicsScene?.GetMethod(
                "SphereCast",
                new Type[] {
                typeof(Vector3),
                typeof(float),
                typeof(Vector3),
                typeof(RaycastHit[]),
                typeof(float),
                typeof(int),
                typeof(QueryTriggerInteraction),
                });
            var method_CheckSphere = type_Physics?.GetMethod(
                "CheckSphere",
                new Type[] {
                typeof(Vector3),
                typeof(float),
                typeof(int),
                typeof(QueryTriggerInteraction),
                });
            var method_SphereCastAll = type_Physics?.GetMethod(
               "SphereCastAll",
               new Type[] {
                typeof(Vector3),
                typeof(float),
                typeof(Vector3),
                typeof(float),
                typeof(int),
                typeof(QueryTriggerInteraction),
               });

            var postfix_SphereCast = type_RaycastCallData.GetMethod("PostfixSphereCast", BindingFlags.NonPublic | BindingFlags.Static);
            var postfix_SphereCastNonAlloc = type_RaycastCallData.GetMethod("PostfixSphereCastNonAlloc", BindingFlags.NonPublic | BindingFlags.Static);
            var postfix_CheckSphere = type_RaycastCallData.GetMethod("PostfixCheckSphere", BindingFlags.NonPublic | BindingFlags.Static);
            var postfix_SphereCastAll = type_RaycastCallData.GetMethod("PostfixSphereCastAll", BindingFlags.NonPublic | BindingFlags.Static);

            try
            {
                harmony.Patch(method_SphereCast, null, new HarmonyMethod(postfix_SphereCast));
                harmony.Patch(method_SphereCastNonAlloc, null, new HarmonyMethod(postfix_SphereCastNonAlloc));
                harmony.Patch(method_CheckSphere, null, new HarmonyMethod(postfix_CheckSphere));
                harmony.Patch(method_SphereCastAll, null, new HarmonyMethod(postfix_SphereCastAll));
            }
            catch
            {
                Debug.LogWarning($"Iternal Unity code used by {typeof(RaycastCallData).Name} has been changed or removed");
            }

            #endregion

            #region OverlapSphere

            var method_OverlapSphere = type_Physics?.GetMethod(
                "OverlapSphere",
                new Type[] {
                typeof(Vector3),
                typeof(float),
                typeof(int),
                typeof(QueryTriggerInteraction),
                });
            var method_OverlapSphereNonAlloc = type_PhysicsScene?.GetMethod(
                "OverlapSphere",
                new Type[] {
                typeof(Vector3),
                typeof(float),
                typeof(Collider[]),
                typeof(int),
                typeof(QueryTriggerInteraction),
                });

            var postfix_OverlapSphere = type_RaycastCallData.GetMethod("PostfixOverlapSphere", BindingFlags.NonPublic | BindingFlags.Static);
            var postfix_OverlapSphereNonAlloc = type_RaycastCallData.GetMethod("PostfixOverlapSphereNonAlloc", BindingFlags.NonPublic | BindingFlags.Static);

            try
            {
                harmony.Patch(method_OverlapSphere, null, new HarmonyMethod(postfix_OverlapSphere));
                harmony.Patch(method_OverlapSphereNonAlloc, null, new HarmonyMethod(postfix_OverlapSphereNonAlloc));
            }
            catch
            {
                Debug.LogWarning($"Iternal Unity code used by {typeof(RaycastCallData).Name} has been changed or removed");
            }

            #endregion

            #region BoxCast

            var method_BoxCast = type_PhysicsScene?.GetMethod(
                "BoxCast",
                new Type[] {
                typeof(Vector3),
                typeof(Vector3),
                typeof(Vector3),
                typeof(RaycastHit).MakeByRefType(),
                typeof(Quaternion),
                typeof(float),
                typeof(int),
                typeof(QueryTriggerInteraction),
                });
            var method_BoxCastNonAlloc = type_PhysicsScene?.GetMethod(
                "BoxCast",
                new Type[] {
                typeof(Vector3),
                typeof(Vector3),
                typeof(Vector3),
                typeof(RaycastHit[]),
                typeof(Quaternion),
                typeof(float),
                typeof(int),
                typeof(QueryTriggerInteraction),
                });
            var method_CheckBox = type_Physics?.GetMethod(
                "CheckBox",
                new Type[] {
                typeof(Vector3),
                typeof(Vector3),
                typeof(Quaternion),
                typeof(int),
                typeof(QueryTriggerInteraction),
                });
            var method_BoxCastAll = type_Physics?.GetMethod(
               "BoxCastAll",
               new Type[] {
                typeof(Vector3),
                typeof(Vector3),
                typeof(Vector3),
                typeof(Quaternion),
                typeof(float),
                typeof(int),
                typeof(QueryTriggerInteraction),
               });

            var postfix_BoxCast = type_RaycastCallData.GetMethod("PostfixBoxCast", BindingFlags.NonPublic | BindingFlags.Static);
            var postfix_BoxCastNonAlloc = type_RaycastCallData.GetMethod("PostfixBoxCastNonAlloc", BindingFlags.NonPublic | BindingFlags.Static);
            var postfix_CheckBox = type_RaycastCallData.GetMethod("PostfixCheckBox", BindingFlags.NonPublic | BindingFlags.Static);
            var postfix_BoxCastAll = type_RaycastCallData.GetMethod("PostfixBoxCastAll", BindingFlags.NonPublic | BindingFlags.Static);

            try
            {
                harmony.Patch(method_BoxCast, null, new HarmonyMethod(postfix_BoxCast));
                harmony.Patch(method_BoxCastNonAlloc, null, new HarmonyMethod(postfix_BoxCastNonAlloc));
                harmony.Patch(method_CheckBox, null, new HarmonyMethod(postfix_CheckBox));
                harmony.Patch(method_BoxCastAll, null, new HarmonyMethod(postfix_BoxCastAll));
            }
            catch
            {
                Debug.LogWarning($"Iternal Unity code used by {typeof(RaycastCallData).Name} has been changed or removed");
            }

            #endregion

            #region OverlapBox

            var method_OverlapBox = type_Physics?.GetMethod(
                "OverlapBox",
                new Type[] {
                typeof(Vector3),
                typeof(Vector3),
                typeof(Quaternion),
                typeof(int),
                typeof(QueryTriggerInteraction),
                });
            var method_OverlapBoxNonAlloc = type_PhysicsScene?.GetMethod(
                "OverlapBox",
                new Type[] {
                typeof(Vector3),
                typeof(Vector3),
                typeof(Collider[]),
                typeof(Quaternion),
                typeof(int),
                typeof(QueryTriggerInteraction),
                });

            var postfix_OverlapBox = type_RaycastCallData.GetMethod("PostfixOverlapBox", BindingFlags.NonPublic | BindingFlags.Static);
            var postfix_OverlapBoxNonAlloc = type_RaycastCallData.GetMethod("PostfixOverlapBoxNonAlloc", BindingFlags.NonPublic | BindingFlags.Static);

            try
            {
                harmony.Patch(method_OverlapBox, null, new HarmonyMethod(postfix_OverlapBox));
                harmony.Patch(method_OverlapBoxNonAlloc, null, new HarmonyMethod(postfix_OverlapBoxNonAlloc));
            }
            catch
            {
                Debug.LogWarning($"Iternal Unity code used by {typeof(RaycastCallData).Name} has been changed or removed");
            }

            #endregion

            #region CapsuleCast

            var method_CapsuleCast = type_PhysicsScene?.GetMethod(
                "CapsuleCast",
                new Type[] {
                typeof(Vector3),
                typeof(Vector3),
                typeof(float),
                typeof(Vector3),
                typeof(RaycastHit).MakeByRefType(),
                typeof(float),
                typeof(int),
                typeof(QueryTriggerInteraction),
                });
            var method_CapsuleCastNonAlloc = type_PhysicsScene?.GetMethod(
                "CapsuleCast",
                new Type[] {
                typeof(Vector3),
                typeof(Vector3),
                typeof(float),
                typeof(Vector3),
                typeof(RaycastHit[]),
                typeof(float),
                typeof(int),
                typeof(QueryTriggerInteraction),
                });
            var method_CheckCapsule = type_Physics?.GetMethod(
                "CheckCapsule",
                new Type[] {
                typeof(Vector3),
                typeof(Vector3),
                typeof(float),
                typeof(int),
                typeof(QueryTriggerInteraction),
                });
            var method_CapsuleCastAll = type_Physics?.GetMethod(
               "CapsuleCastAll",
               new Type[] {
                typeof(Vector3),
                typeof(Vector3),
                typeof(float),
                typeof(Vector3),
                typeof(float),
                typeof(int),
                typeof(QueryTriggerInteraction),
               });

            var postfix_CapsuleCast = type_RaycastCallData.GetMethod("PostfixCapsuleCast", BindingFlags.NonPublic | BindingFlags.Static);
            var postfix_CapsuleCastNonAlloc = type_RaycastCallData.GetMethod("PostfixCapsuleCastNonAlloc", BindingFlags.NonPublic | BindingFlags.Static);
            var postfix_CheckCapsule = type_RaycastCallData.GetMethod("PostfixCheckCapsule", BindingFlags.NonPublic | BindingFlags.Static);
            var postfix_CapsuleCastAll = type_RaycastCallData.GetMethod("PostfixCapsuleCastAll", BindingFlags.NonPublic | BindingFlags.Static);

            try
            {
                harmony.Patch(method_CapsuleCast, null, new HarmonyMethod(postfix_CapsuleCast));
                harmony.Patch(method_CapsuleCastNonAlloc, null, new HarmonyMethod(postfix_CapsuleCastNonAlloc));
                harmony.Patch(method_CheckCapsule, null, new HarmonyMethod(postfix_CheckCapsule));
                harmony.Patch(method_CapsuleCastAll, null, new HarmonyMethod(postfix_CapsuleCastAll));
            }
            catch
            {
                Debug.LogWarning($"Iternal Unity code used by {typeof(RaycastCallData).Name} has been changed or removed");
            }

            #endregion

            #region OverlapCapsule

            var method_OverlapCapsule = type_Physics?.GetMethod(
                "OverlapCapsule",
                new Type[] {
                typeof(Vector3),
                typeof(Vector3),
                typeof(float),
                typeof(int),
                typeof(QueryTriggerInteraction),
                });
            var method_OverlapCapsuleNonAlloc = type_PhysicsScene?.GetMethod(
                "OverlapCapsule",
                new Type[] {
                typeof(Vector3),
                typeof(Vector3),
                typeof(float),
                typeof(Collider[]),
                typeof(int),
                typeof(QueryTriggerInteraction),
                });

            var postfix_OverlapCapsule = type_RaycastCallData.GetMethod("PostfixOverlapCapsule", BindingFlags.NonPublic | BindingFlags.Static);
            var postfix_OverlapCapsuleNonAlloc = type_RaycastCallData.GetMethod("PostfixOverlapCapsuleNonAlloc", BindingFlags.NonPublic | BindingFlags.Static);

            try
            {
                harmony.Patch(method_OverlapCapsule, null, new HarmonyMethod(postfix_OverlapCapsule));
                harmony.Patch(method_OverlapCapsuleNonAlloc, null, new HarmonyMethod(postfix_OverlapCapsuleNonAlloc));
            }
            catch
            {
                Debug.LogWarning($"Iternal Unity code used by {typeof(RaycastCallData).Name} has been changed or removed");
            }

            #endregion
        }

        #region Raycast

        private static void PostfixRaycastTest(ref bool __result, ref Vector3 origin, ref Vector3 direction, ref float maxDistance, ref int layerMask, ref QueryTriggerInteraction queryTriggerInteraction)
        {
            if (onPhysicsCastCall is null) return;

            var info = new RaycastInfo(origin, direction, maxDistance, layerMask, queryTriggerInteraction, GetRaycastStackTrace(), __result, new RaycastHit[0]);
            onPhysicsCastCall.Invoke(info);
        }

        private static void PostfixRaycast(ref bool __result, ref RaycastHit hitInfo, ref Vector3 origin, ref Vector3 direction, ref float maxDistance, ref int layerMask, ref QueryTriggerInteraction queryTriggerInteraction)
        {
            if (onPhysicsCastCall is null) return;

            var hitInfos = __result ? new RaycastHit[] { hitInfo } : new RaycastHit[0];
            var info = new RaycastInfo(origin, direction, maxDistance, layerMask, queryTriggerInteraction, GetRaycastStackTrace(), __result, hitInfos);
            onPhysicsCastCall.Invoke(info);
        }

        private static void PostfixRaycastNonAlloc(ref int __result, ref RaycastHit[] raycastHits, ref Vector3 origin, ref Vector3 direction, ref float maxDistance, ref int layerMask, ref QueryTriggerInteraction queryTriggerInteraction)
        {
            if (onPhysicsCastCall is null) return;

            var hitInfos = raycastHits.Take(__result).ToArray();
            var info = new RaycastInfo(origin, direction, maxDistance, layerMask, queryTriggerInteraction, GetRaycastStackTrace(), __result > 0, hitInfos);
            onPhysicsCastCall.Invoke(info);
        }

        private static void PostfixRaycastAll(ref RaycastHit[] __result, ref Vector3 origin, ref Vector3 direction, ref float maxDistance, ref int layerMask, ref QueryTriggerInteraction queryTriggerInteraction)
        {
            if (onPhysicsCastCall is null) return;

            var info = new RaycastInfo(origin, direction, maxDistance, layerMask, queryTriggerInteraction, GetRaycastStackTrace(), __result.Length > 0, __result);
            onPhysicsCastCall.Invoke(info);
        }

        #endregion

        #region SphereCast

        private static void PostfixCheckSphere(ref bool __result, ref Vector3 position, ref float radius, ref int layerMask, ref QueryTriggerInteraction queryTriggerInteraction)
        {
            if (onPhysicsCastCall is null) return;

            var info = new SphereCastInfo(position, radius, Vector3.zero, 0, layerMask, queryTriggerInteraction, GetRaycastStackTrace(), __result, new RaycastHit[0]);
            onPhysicsCastCall.Invoke(info);
        }

        private static void PostfixSphereCast(ref bool __result, ref RaycastHit hitInfo, ref Vector3 origin, ref float radius, ref Vector3 direction, ref float maxDistance, ref int layerMask, ref QueryTriggerInteraction queryTriggerInteraction)
        {
            if (onPhysicsCastCall is null) return;

            var hitInfos = __result ? new RaycastHit[] { hitInfo } : new RaycastHit[0];
            var info = new SphereCastInfo(origin, radius, direction, maxDistance, layerMask, queryTriggerInteraction, GetRaycastStackTrace(), __result, hitInfos);
            onPhysicsCastCall.Invoke(info);
        }

        private static void PostfixSphereCastNonAlloc(ref int __result, ref RaycastHit[] results, ref Vector3 origin, ref float radius, ref Vector3 direction, ref float maxDistance, ref int layerMask, ref QueryTriggerInteraction queryTriggerInteraction)
        {
            if (onPhysicsCastCall is null) return;

            var hitInfos = results.Take(__result).ToArray();
            var info = new SphereCastInfo(origin, radius, direction, maxDistance, layerMask, queryTriggerInteraction, GetRaycastStackTrace(), __result > 0, hitInfos);
            onPhysicsCastCall.Invoke(info);
        }

        private static void PostfixSphereCastAll(ref RaycastHit[] __result, ref Vector3 origin, ref float radius, ref Vector3 direction, ref float maxDistance, ref int layerMask, ref QueryTriggerInteraction queryTriggerInteraction)
        {
            if (onPhysicsCastCall is null) return;

            var info = new SphereCastInfo(origin, radius, direction, maxDistance, layerMask, queryTriggerInteraction, GetRaycastStackTrace(), __result.Length > 0, __result);
            onPhysicsCastCall.Invoke(info);
        }

        #endregion

        #region OverlapSphere

        private static void PostfixOverlapSphere(ref Collider[] __result, ref Vector3 position, ref float radius, ref int layerMask, ref QueryTriggerInteraction queryTriggerInteraction)
        {
            if (onPhysicsCastCall is null) return;

            var info = new SphereCastInfo(position, radius, Vector3.zero, 0, layerMask, queryTriggerInteraction, GetRaycastStackTrace(), __result.Length > 0, __result);
            onPhysicsCastCall.Invoke(info);
        }

        private static void PostfixOverlapSphereNonAlloc(ref int __result, ref Collider[] results, ref Vector3 position, ref float radius, ref int layerMask, ref QueryTriggerInteraction queryTriggerInteraction)
        {
            if (onPhysicsCastCall is null) return;

            var hitInfos = results.Take(__result).ToArray();
            var info = new SphereCastInfo(position, radius, Vector3.zero, 0, layerMask, queryTriggerInteraction, GetRaycastStackTrace(), __result > 0, hitInfos);
            onPhysicsCastCall.Invoke(info);
        }

        #endregion

        #region BoxCast

        //If reflection couse error on this method, it's probably "layermask" renamed to "layerMask"
        private static void PostfixCheckBox(ref bool __result, ref Vector3 center, ref Vector3 halfExtents, ref Quaternion orientation, ref int layermask, ref QueryTriggerInteraction queryTriggerInteraction)
        {
            if (onPhysicsCastCall is null) return;

            var info = new BoxCastInfo(center, halfExtents, Vector3.zero, orientation, 0, layermask, queryTriggerInteraction, GetRaycastStackTrace(), __result, new RaycastHit[0]);
            onPhysicsCastCall.Invoke(info);
        }

        private static void PostfixBoxCast(ref bool __result, ref RaycastHit hitInfo, ref Vector3 center, ref Vector3 halfExtents, ref Vector3 direction, ref Quaternion orientation, ref float maxDistance, ref int layerMask, ref QueryTriggerInteraction queryTriggerInteraction)
        {
            if (onPhysicsCastCall is null) return;

            var hitInfos = __result ? new RaycastHit[] { hitInfo } : new RaycastHit[0];
            var info = new BoxCastInfo(center, halfExtents, direction, orientation, maxDistance, layerMask, queryTriggerInteraction, GetRaycastStackTrace(), __result, hitInfos);
            onPhysicsCastCall.Invoke(info);
        }

        private static void PostfixBoxCastNonAlloc(ref int __result, ref RaycastHit[] results, ref Vector3 center, ref Vector3 halfExtents, ref Vector3 direction, ref Quaternion orientation, ref float maxDistance, ref int layerMask, ref QueryTriggerInteraction queryTriggerInteraction)
        {
            if (onPhysicsCastCall is null) return;

            var hitInfos = results.Take(__result).ToArray();
            var info = new BoxCastInfo(center, halfExtents, direction, orientation, maxDistance, layerMask, queryTriggerInteraction, GetRaycastStackTrace(), __result > 0, hitInfos);
            onPhysicsCastCall.Invoke(info);
        }

        private static void PostfixBoxCastAll(ref RaycastHit[] __result, ref Vector3 center, ref Vector3 halfExtents, ref Vector3 direction, ref Quaternion orientation, ref float maxDistance, ref int layerMask, ref QueryTriggerInteraction queryTriggerInteraction)
        {
            if (onPhysicsCastCall is null) return;

            var info = new BoxCastInfo(center, halfExtents, direction, orientation, maxDistance, layerMask, queryTriggerInteraction, GetRaycastStackTrace(), __result.Length > 0, __result);
            onPhysicsCastCall.Invoke(info);
        }

        #endregion

        #region OverlapBox

        private static void PostfixOverlapBox(ref Collider[] __result, ref Vector3 center, ref Vector3 halfExtents, ref Quaternion orientation, ref int layerMask, ref QueryTriggerInteraction queryTriggerInteraction)
        {
            if (onPhysicsCastCall is null) return;

            var info = new BoxCastInfo(center, halfExtents, Vector3.zero, orientation, 0, layerMask, queryTriggerInteraction, GetRaycastStackTrace(), __result.Length > 0, __result);
            onPhysicsCastCall.Invoke(info);
        }

        private static void PostfixOverlapBoxNonAlloc(ref int __result, ref Collider[] results, ref Vector3 center, ref Vector3 halfExtents, ref Quaternion orientation, ref int layerMask, ref QueryTriggerInteraction queryTriggerInteraction)
        {
            if (onPhysicsCastCall is null) return;

            var hitInfos = results.Take(__result).ToArray();
            var info = new BoxCastInfo(center, halfExtents, Vector3.zero, orientation, 0, layerMask, queryTriggerInteraction, GetRaycastStackTrace(), __result > 0, hitInfos);
            onPhysicsCastCall.Invoke(info);
        }

        #endregion

        #region CapsuleCast

        private static void PostfixCheckCapsule(ref bool __result, ref Vector3 start, ref Vector3 end, ref float radius, ref int layerMask, ref QueryTriggerInteraction queryTriggerInteraction)
        {
            if (onPhysicsCastCall is null) return;

            var info = new CapsuleCastInfo(start, end, radius, Vector3.zero, 0, layerMask, queryTriggerInteraction, GetRaycastStackTrace(), __result, new RaycastHit[0]);
            onPhysicsCastCall.Invoke(info);
        }

        private static void PostfixCapsuleCast(ref bool __result, ref RaycastHit hitInfo, ref Vector3 point1, ref Vector3 point2, ref float radius, ref Vector3 direction, ref float maxDistance, ref int layerMask, ref QueryTriggerInteraction queryTriggerInteraction)
        {
            if (onPhysicsCastCall is null) return;

            var hitInfos = __result ? new RaycastHit[] { hitInfo } : new RaycastHit[0];
            var info = new CapsuleCastInfo(point1, point2, radius, direction, maxDistance, layerMask, queryTriggerInteraction, GetRaycastStackTrace(), __result, hitInfos);
            onPhysicsCastCall.Invoke(info);
        }

        private static void PostfixCapsuleCastNonAlloc(ref int __result, ref RaycastHit[] results, ref Vector3 point1, ref Vector3 point2, ref float radius, ref Vector3 direction, ref float maxDistance, ref int layerMask, ref QueryTriggerInteraction queryTriggerInteraction)
        {
            if (onPhysicsCastCall is null) return;

            var hitInfos = results.Take(__result).ToArray();
            var info = new CapsuleCastInfo(point1, point2, radius, direction, maxDistance, layerMask, queryTriggerInteraction, GetRaycastStackTrace(), __result > 0, hitInfos);
            onPhysicsCastCall.Invoke(info);
        }

        private static void PostfixCapsuleCastAll(ref RaycastHit[] __result, ref Vector3 point1, ref Vector3 point2, ref float radius, ref Vector3 direction, ref float maxDistance, ref int layerMask, ref QueryTriggerInteraction queryTriggerInteraction)
        {
            if (onPhysicsCastCall is null) return;

            var info = new CapsuleCastInfo(point1, point2, radius, direction, maxDistance, layerMask, queryTriggerInteraction, GetRaycastStackTrace(), __result.Length > 0, __result);
            onPhysicsCastCall.Invoke(info);
        }

        #endregion

        #region CapsuleCast

        private static void PostfixOverlapCapsule(ref Collider[] __result, ref Vector3 point0, ref Vector3 point1, ref float radius, ref int layerMask, ref QueryTriggerInteraction queryTriggerInteraction)
        {
            if (onPhysicsCastCall is null) return;

            var info = new CapsuleCastInfo(point0, point1, radius, Vector3.zero, 0, layerMask, queryTriggerInteraction, GetRaycastStackTrace(), __result.Length > 0, __result);
            onPhysicsCastCall.Invoke(info);
        }

        private static void PostfixOverlapCapsuleNonAlloc(ref int __result, ref Collider[] results, ref Vector3 point0, ref Vector3 point1, ref float radius, ref int layerMask, ref QueryTriggerInteraction queryTriggerInteraction)
        {
            if (onPhysicsCastCall is null) return;

            var hitInfos = results.Take(__result).ToArray();
            var info = new CapsuleCastInfo(point0, point1, radius, Vector3.zero, 0, layerMask, queryTriggerInteraction, GetRaycastStackTrace(), __result > 0, hitInfos);
            onPhysicsCastCall.Invoke(info);
        }

        #endregion

        private static string GetRaycastStackTrace()
        {
            var stackTrace = StackTraceUtility.ExtractStackTrace();
            var stacktraceWithHyperlinks = method_StacktraceWithHyperlinks.Invoke(null, new object[] { stackTrace, 0 }) as string;

            stacktraceWithHyperlinks = new string(stacktraceWithHyperlinks
                .ToArray()
                .SkipWhile(t => t != '\n')
                .Skip(1)
                .SkipWhile(t => t != '\n')
                .Skip(1)
                .SkipWhile(t => t != '\n')
                .Skip(1)
                .SkipWhile(t => t != '\n')
                .Skip(1)
                .ToArray());

            stacktraceWithHyperlinks = new string(stacktraceWithHyperlinks
                .Take(stacktraceWithHyperlinks.Length - 1)
                .ToArray());

            return stacktraceWithHyperlinks;
        }
    }
}
