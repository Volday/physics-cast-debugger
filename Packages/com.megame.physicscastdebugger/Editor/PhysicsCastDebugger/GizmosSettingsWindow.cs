using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PhysicsCastDebugger
{
    public class GizmosSettingsWindow : EditorWindow
    {
        private static GizmosSettingsWindow gizmosSettingsWindow = null;

        private GizmosSettings settings;

        public static void ShowAtPosition(Rect buttonRect, GizmosSettings settings)
        {
            Event.current.Use();

            if (settings is null) return;

            if (gizmosSettingsWindow == null)
            {
                gizmosSettingsWindow = ScriptableObject.CreateInstance<GizmosSettingsWindow>();
            }
            else
            {
                gizmosSettingsWindow.Cancel();
                return;
            }

            gizmosSettingsWindow.settings = settings;
            gizmosSettingsWindow.Init(buttonRect);
        }

        public void Init(Rect buttonRect)
        {
            buttonRect = GUIUtility.GUIToScreenRect(buttonRect);

            var windowWidth = 250;
            var windowHeight = 200;
            Vector2 windowSize = new Vector2(windowWidth, windowHeight);

            ShowAsDropDown(buttonRect, windowSize);
        }

        private void Cancel()
        {
            Close();
            GUI.changed = true;
            GUIUtility.ExitGUI();
        }

        private void OnGUI()
        {
            settings.drawSelected = GUILayout.Toggle(settings.drawSelected, "Draw selected");
            settings.drawOrigin = GUILayout.Toggle(settings.drawOrigin, "Draw origins");
            settings.drawHitPoint = GUILayout.Toggle(settings.drawHitPoint, "Draw hit points");
            settings.drawNewCast = GUILayout.Toggle(settings.drawNewCast, "Draw new casts");
            settings.usefadeEffect = GUILayout.Toggle(settings.usefadeEffect, "Use fade effect");

            GUILayout.Space(5);

            settings.gizmosLifeTime = Mathf.Max(EditorGUILayout.FloatField("Gizmos life time", settings.gizmosLifeTime), 0.2f);
            settings.pointSize = Mathf.Max(EditorGUILayout.FloatField("Point size", settings.pointSize), 0.01f);

            GUILayout.Space(5);

            settings.castColor = EditorGUILayout.ColorField("Physics cast color", settings.castColor);
            settings.originPointColor = EditorGUILayout.ColorField("Origin point color", settings.originPointColor);
            settings.hitPointColor = EditorGUILayout.ColorField("Hit point color", settings.hitPointColor);

            if (Event.current.type == EventType.Repaint)
            {
                GUIStyle background = "grey_border";
                background.Draw(new Rect(0, 0, position.width, position.height), GUIContent.none, false, false, false, false);
            }
        }
    }
}