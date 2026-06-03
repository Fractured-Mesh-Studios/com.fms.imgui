using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using GuiEngine;

namespace GuiEditor
{

    [CustomEditor(typeof(Window), true)]
    public class WindowEditor : Editor
    {
        protected Window m_target;
        protected bool m_transformTab;
        protected bool m_headerTab;
        protected bool m_renderTab;

        protected virtual void OnEnable()
        {
            m_target = (Window)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            DrawWindow();
        }

        protected virtual void DrawWindow()
        {
            //m_debugEnabled = EditorGUILayout.ToggleLeft("Enable Debug", m_debugEnabled);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_skin"));

            Tab(ref m_headerTab, "Header", () => { 
                EditorGUILayout.PropertyField(serializedObject.FindProperty("title"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("tooltip"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("icon"));
            });

            Tab(ref m_transformTab, "Transform", () =>
            {
                m_target.rect = DrawRect(m_target.rect, "Window Rect");

                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("drag"));
                if (m_target.drag)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("clamp"));
                    m_target.dragSize = EditorGUILayout.Vector2Field(string.Empty, m_target.dragSize);
                    m_target.dragSize.x = EditorGUILayout.Slider(m_target.dragSize.x, 0, m_target.rect.width);
                    m_target.dragSize.y = EditorGUILayout.Slider(m_target.dragSize.y, 0, m_target.rect.height);
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("resize"));
                if (m_target.resize)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("resizeIcon"));
                    
                }
                EditorGUILayout.EndVertical();
            });

            Tab(ref m_renderTab, "Render", () => {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("color"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("opacity"));
            });

            serializedObject.ApplyModifiedProperties();
        }

        protected void Tab(ref bool tab, string name, System.Action onOpen)
        {
            EditorGUILayout.BeginVertical("GroupBox");
            EditorGUI.indentLevel = 1;
            tab = EditorGUILayout.Foldout(tab, name, true);
            if(tab && onOpen != null)
            {
                var rect = EditorGUILayout.GetControlRect();
                rect.height = 1;
                EditorGUI.DrawRect(rect, Color.gray);
                onOpen.Invoke();
            }
            EditorGUI.indentLevel = 0;
            EditorGUILayout.EndVertical();
        }

        protected void DrawButtons()
        {
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Open")) { m_target.Open(); }
            if (GUILayout.Button("Close")) { m_target.Close(); }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.HelpBox("SendMessage To GameObject: OnToggle(InputValue)", MessageType.Info);
        }

        protected Rect DrawRect(Rect rect, string name)
        {
            Rect selfRect;

            GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel);
            labelStyle.alignment = TextAnchor.MiddleCenter;
            labelStyle.normal.textColor = Color.cyan;

            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField(name, labelStyle);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(5);
            selfRect = EditorGUILayout.RectField(rect);
            GUILayout.Space(5);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5);
            EditorGUILayout.EndVertical();

            return selfRect;
        }
    }
}