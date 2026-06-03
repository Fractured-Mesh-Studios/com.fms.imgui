using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GuiEngine
{
    public interface IWindow
    {
        void OnWindowGUI(int id);
    }

    [ExecuteInEditMode]
    public class Window : MonoBehaviour
    {
        private const float RESIZE_WIDTH = 20;
        private const float RESIZE_HEIGHT = 20;
        private const string PREFS_KEY_SUFFIX_X = ".x";
        private const string PREFS_KEY_SUFFIX_Y = ".y";
        private const string PREFS_KEY_SUFFIX_WIDTH = ".width";
        private const string PREFS_KEY_SUFFIX_HEIGHT = ".height";

        [HideInInspector] public Rect rect = new Rect(50, 50, 400, 200);
        [HideInInspector] public Vector2 dragSize = new Vector2(400, 20);

        [HideInInspector] public string title = "Window";
        [HideInInspector] public string tooltip = "Ingame Window";
        [HideInInspector] public Texture icon = null;
        [HideInInspector] public Texture resizeIcon = null;
        [HideInInspector] public bool drag, resize, clamp;
        [HideInInspector] public Color color = Color.white;
        [HideInInspector][Range(0,1)] public float opacity = 1f;
        
        [SerializeField, HideInInspector] private GUISkin m_skin;
        [SerializeField, HideInInspector] private string m_playerPrefsKey = string.Empty;
        [SerializeField, HideInInspector] private bool m_persistLayout = true;

        public Action<bool> onOpen;
        public Action<bool> onClose;

        public bool isOpen => m_isOpen;

        public int width => (int)rect.width;

        public int height => (int)rect.height;

        private GUIContent m_windowContent;
        private int m_windowId;
        private bool m_isOpen;
        private Event m_event;

        private Vector2 m_mousePosition, m_mouseDragPosition;
        private Rect m_dragRect;
        private Rect m_resizeRect;
        private bool m_canDrag = false;
        private bool m_canResize = false;
        private Rect m_lastSavedRect;

        private IWindow[] m_components;

        private WindowMobile m_mobile;

        #region UNITY
        protected virtual void OnEnable()
        {
            m_windowContent = new GUIContent(title, icon, tooltip);

            m_components = GetComponentsInChildren<IWindow>(true);

            m_mobile = GetComponent<WindowMobile>();

            LoadWindowState();
            m_lastSavedRect = rect;
        }

        protected virtual void OnDisable()
        {
            SaveWindowState();

            m_windowContent = new GUIContent();
            m_isOpen = false;

            m_components = null;
        }
        #endregion

        #region INPUT
        private void OnToggle(InputValue value)
        {
            if (value.isPressed)
            {
                Toggle();
            }
        }
        #endregion

        #region GUI
        private void OnGUI()
        {
            GUI.skin = m_skin;
            m_event = Event.current;

            if (m_mobile != null) { 
                m_mobile.BeginMobile();
            }

            Vector2 resizeOffset = new Vector2(
                rect.width - RESIZE_WIDTH,
                rect.height - RESIZE_HEIGHT
            );

            if(m_isOpen)
            {
                m_mousePosition = Event.current.mousePosition;
                m_dragRect = new Rect(rect.position, dragSize);
                m_resizeRect = new Rect(rect.position + resizeOffset, new Vector2(RESIZE_WIDTH, RESIZE_HEIGHT));

                if(m_event.type == EventType.MouseDown && m_event.button == 0)
                {
                    //Resize
                    m_canResize = m_resizeRect.Contains(m_mousePosition);

                    //Drag
                    if (m_dragRect.Contains(m_mousePosition))
                    {
                        m_canDrag = true;
                        m_mouseDragPosition = m_mousePosition - rect.position;
                    }
                    else
                    {
                        m_canDrag = false;
                    }

                    m_event.Use();
                }

                if (m_event.type == EventType.MouseUp && m_event.button == 0)
                {
                    if (m_canDrag || m_canResize)
                    {
                        SaveWindowState();
                    }

                    m_canDrag = false;
                    m_canResize = false;
                }

                m_canDrag = drag ? m_canDrag : false;
                m_canResize = resize ? m_canResize : false;

                if(resize) GUI.Label(m_resizeRect, resizeIcon);

                if(m_event.type == EventType.MouseDrag && m_event.button == 0 && m_canDrag)
                {
                    rect = new Rect(m_mousePosition - m_mouseDragPosition, rect.size);

                    if (clamp)
                    {
                        rect.x = Mathf.Clamp(rect.x, 0, Screen.width - rect.width);
                        rect.y = Mathf.Clamp(rect.y, 0, Screen.height - rect.height);
                    }
                }

                if(m_event.type == EventType.MouseDrag && m_event.button == 0 && m_canResize)
                {
                    var w = m_mousePosition.x - rect.x;
                    var h = m_mousePosition.y - rect.y;

                    rect.width = Mathf.Clamp(w, Screen.width * 0.1f, Screen.width);
                    rect.height = Mathf.Clamp(h, Screen.height * 0.1f, Screen.height);

                    m_dragRect.width = rect.width;
                    m_dragRect.height = rect.height;
                }

                /*m_closeRect = new Rect(rect.x + rect.width - 40, rect.y, 40, 20);
                if (GUI.Button(m_closeRect, "X"))
                {
                    Close();
                }*/

                //Window
                GUI.color = new Color(color.r,color.g, color.b, opacity);
                rect = GUI.Window(m_windowId, rect, OnWindow, m_windowContent);
                GUI.color = Color.white;
            }

            if (m_mobile != null)
            {
                m_mobile.EndMobile();
            }
        }
        #endregion

        #region WINDOW
        protected virtual void OnWindow(int id)
        {
            foreach(var component in m_components)
            {
                component.OnWindowGUI(id);
            }
        }

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
                SaveWindowState();
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
                SaveWindowState();

                if (onClose != null)
                    onClose.Invoke(m_isOpen);
            }
        }
        #endregion

        #region LOAD_SAVE
        private void LoadWindowState()
        {
            if (!Application.isPlaying || !m_persistLayout)
            {
                return;
            }

            string key = GetPrefsKey();

            if (!PlayerPrefs.HasKey(key + PREFS_KEY_SUFFIX_X))
            {
                return;
            }

            rect = new Rect(
                PlayerPrefs.GetFloat(key + PREFS_KEY_SUFFIX_X, rect.x),
                PlayerPrefs.GetFloat(key + PREFS_KEY_SUFFIX_Y, rect.y),
                PlayerPrefs.GetFloat(key + PREFS_KEY_SUFFIX_WIDTH, rect.width),
                PlayerPrefs.GetFloat(key + PREFS_KEY_SUFFIX_HEIGHT, rect.height)
            );
        }

        private void SaveWindowState()
        {
            if (!Application.isPlaying || !m_persistLayout)
            {
                return;
            }

            if (rect == m_lastSavedRect)
            {
                return;
            }

            string key = GetPrefsKey();

            PlayerPrefs.SetFloat(key + PREFS_KEY_SUFFIX_X, rect.x);
            PlayerPrefs.SetFloat(key + PREFS_KEY_SUFFIX_Y, rect.y);
            PlayerPrefs.SetFloat(key + PREFS_KEY_SUFFIX_WIDTH, rect.width);
            PlayerPrefs.SetFloat(key + PREFS_KEY_SUFFIX_HEIGHT, rect.height);
            PlayerPrefs.Save();

            m_lastSavedRect = rect;
        }

        private string GetPrefsKey()
        {
            if (!string.IsNullOrWhiteSpace(m_playerPrefsKey))
            {
                return m_playerPrefsKey;
            }

            string windowName = SanitizeKeySegment(string.IsNullOrWhiteSpace(title) ? gameObject.name : title);
            string sceneName = SanitizeKeySegment(gameObject.scene.IsValid() ? gameObject.scene.name : "NoScene");

            return $"GuiEngine.{sceneName}.{windowName}";
        }

        private static string SanitizeKeySegment(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "Unnamed";
            }

            StringBuilder builder = new StringBuilder(value.Length);

            foreach (char character in value)
            {
                builder.Append(char.IsLetterOrDigit(character) ? character : '_');
            }

            return builder.ToString();
        }
        #endregion
    }
}