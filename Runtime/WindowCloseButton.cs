using LoggerEngine;
using System;
using UnityEngine;

namespace GuiEngine
{
    public class WindowCloseButton : MonoBehaviour, IWindow
    {
        [Header("Appearance")]
        public Color color = Color.red;
        public string text = "x";
        public Vector2Int size = new Vector2Int(15, 15);
        public Vector2Int offset = new Vector2Int(0, 2);

        private Window m_window;

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
            GUI.color = color;
            if (GUI.Button(new Rect(window.width - size.x - offset.x, offset.y, size.x, size.y), text))
            {
                window.Close();
            }
            GUI.color = Color.white;
        }
    }
}
