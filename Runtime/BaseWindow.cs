using System;
using UnityEngine;

namespace GuiEngine
{
    public interface IWindowModule
    {
        void OnWindowGUI(int id);
    }

    public abstract class BaseWindow : MonoBehaviour
    {
        [HideInInspector] public Rect rect = new Rect(50, 50, 400, 200);
        [HideInInspector] public string title = "Window";
        [HideInInspector] public string tooltip = "Ingame Window";
        [HideInInspector] public Texture icon = null;

        public Action<bool> onOpen;
        public Action<bool> onClose;

        protected int m_windowId = 1;
        private bool m_isOpen;
        public bool isOpen => m_isOpen;

        public int width => (int)rect.width;

        public int height => (int)rect.height;

        public void Open()
        {
            if (!m_isOpen)
            {
                m_isOpen = true;
                if (onOpen != null)
                    onOpen.Invoke(m_isOpen);
            }
        }

        public void Close()
        {
            if (m_isOpen)
            {
                m_isOpen = false;
                //SaveWindowState();
                if (onClose != null)
                    onClose.Invoke(m_isOpen);
            }
        }

        public void Toggle()
        {
            m_isOpen = !m_isOpen;
            if (m_isOpen)
            {
                if (onOpen != null)
                    onOpen.Invoke(m_isOpen);
            }
            else
            {
                //SaveWindowState();

                if (onClose != null)
                    onClose.Invoke(m_isOpen);
            }
        }

        protected GUIContent WindowContent()
        {
            return (icon != null)
                ? new GUIContent(title, icon, tooltip)
                : new GUIContent(title, null, tooltip);
        }
    }
}
