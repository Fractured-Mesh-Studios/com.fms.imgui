using LoggerEngine;
using System;
using UnityEngine;

namespace GuiEngine
{
    public class WindowCloseButton : MonoBehaviour, IWindowModule
    {
        [Header("Appearance")]
        public Color color = Color.red;
        public string text = "x";
        public string tooltip = "Click to close the window";
        public Vector2Int size = new Vector2Int(15, 15);
        public Vector2Int offset = new Vector2Int(0, 2);

        private Window m_window;
        private GUIContent m_content;

        protected Window window
        {
            get
            {
                if (m_window == null)
                    m_window = GetComponentInParent<Window>();
                return m_window;
            }
        }

        public void OnWindowGUI(int id)
        {
            Rect closeButtonRect = new Rect(window.width - size.x - offset.x, offset.y, size.x, size.y);

            m_content = new GUIContent(text, tooltip);
            GUI.color = color;
            if (GUI.Button(closeButtonRect, m_content))
            {
                window.Close();
            }
            GUI.color = Color.white;
        }
    }
}
