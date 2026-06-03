using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GuiEngine
{
    public class WindowEvent : MonoBehaviour
    {
        private Event m_event;

        [Header("Events")]
        public UnityEvent onMouseDown = new UnityEvent();
        public UnityEvent onMouseUp = new UnityEvent();

        public UnityEvent onMouseEnter = new UnityEvent();
        public UnityEvent onMouseExit = new UnityEvent();

        public UnityEvent onMouseDrag = new UnityEvent();

        private Window m_window;
        private bool m_lastContain = false;

        private void Awake()
        {
            m_window = GetComponent<Window>();
        }

        private void OnGUI()
        {
            m_event = Event.current;

            if(!m_window)
            {
                m_window = GetComponent<Window>();
            }

            if (m_event.type == EventType.MouseDown && m_window.isOpen)
            {
                onMouseDown.Invoke();
                OnMouseDownEvent();
            }

            if (m_event.type == EventType.MouseUp && m_window.isOpen)
            {
                onMouseUp.Invoke();
                OnMouseUpEvent();
            }

            if (m_event.type == EventType.MouseDrag && m_window.isOpen)
            {
                onMouseDrag.Invoke();
                OnMouseDragEvent();
            }

            if (m_window.rect.Contains(m_event.mousePosition) && !m_lastContain && m_window.isOpen)
            {
                m_lastContain = true;
                onMouseEnter.Invoke();
                OnMouseEnterEvent();
            }

            if (!m_window.rect.Contains(m_event.mousePosition) && m_lastContain && m_window.isOpen)
            {
                m_lastContain = false;
                onMouseExit.Invoke();
                OnMouseExitEvent();
            }
        }

        protected virtual void OnMouseDownEvent() { }

        protected virtual void OnMouseDragEvent() { }

        protected virtual void OnMouseUpEvent() { }

        protected virtual void OnMouseEnterEvent() { } 
        
        protected virtual void OnMouseExitEvent() { }
    }
}
