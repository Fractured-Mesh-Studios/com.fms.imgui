using UnityEngine;
using UnityEditor;
using GuiEngine;

namespace GuiEditor
{
    [CustomEditor(typeof(WindowTextOverlay))]
    public class WindowOverlayEditor : Editor
    {
        private WindowTextOverlay m_target;

        private bool m_isActive;

        private void OnEnable()
        {
            m_target = (WindowTextOverlay)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space(10);
            if (GUILayout.Button("Toggle Busy Mode"))
            {
                m_isActive = !m_isActive;

                if (m_isActive)
                    m_target.Begin("[ Loading ]");
                else
                    m_target.End();
            }
        }
    }
}
