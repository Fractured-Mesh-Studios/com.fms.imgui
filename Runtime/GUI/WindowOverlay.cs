using UnityEditor;
using UnityEngine;

namespace GuiEngine
{
    /// <summary>
    /// Spinner/overlay para IMGUI de Unity nativo.
    /// Uso:
    ///   1. Instanciar una vez: private readonly GUIBusyOverlay m_busy = new GUIBusyOverlay();
    ///   2. Llamar m_busy.Begin() antes de await, m_busy.End() en el finally.
    ///   3. Llamar m_busy.OnGUI() dentro de OnGUI() del MonoBehaviour.
    ///   4. Envolver los controles interactivos con: if (!m_busy.IsBusy) { ... }
    /// </summary>
    public class WindowOverlay : MonoBehaviour, IWindow
    {
        private static readonly string[] Frames = { "◐", "◓", "◑", "◒" };
        private const float FrameRate = 0.15f;

        public bool IsBusy { get{ return m_isBusy; } private set { m_isBusy = value; } }
        public string Label { get; private set; } = string.Empty;

        [SerializeField] private bool m_isBusy;

        [SerializeField][Range(0,1)]private float opacity = 0.4f;

        [Header("Label")]
        [SerializeField] private Color m_textColor = Color.white;
        [SerializeField][Range(8, 100)] private int fontSize = 18;

        [Header("Overlay Texture")]
        [SerializeField] private Color m_backgroundColor = Color.black;

        [Header("Border")]
        [SerializeField] private Color m_borderColor = Color.black;
        [SerializeField] private float m_borderWidth = 2f;
        [SerializeField] private float m_borderRadius = 8f;

        private float m_elapsed;
        private int m_frame;

        private GUIStyle m_spinnerStyle;
        private GUIStyle m_labelStyle;
        private Texture2D m_borderTexture;

        private Window m_window;
        protected Window window
        {
            get
            {
                if (m_window == null)
                {
                    m_window = GetComponent<Window>();
                }
                return m_window;
            }
        }

        public void Begin(string label = "Loading...")
        {
            IsBusy = true;
            Label = label;
            m_elapsed = 0f;
            m_frame = 0;
            window.Disable();
        }

        public void End()
        {
            IsBusy = false;
            window.Enable();
        }

        private void EnsureStyles()
        {
            if(!m_borderTexture)
            {
                m_borderTexture = new Texture2D(1, 1);
                m_borderTexture.SetPixel(0, 0, Color.white);
                m_borderTexture.Apply();
            }

            if (m_spinnerStyle == null)
            {
                m_spinnerStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 22,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = Color.white }
                };
            }

            if (m_labelStyle == null)
            {
                m_labelStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = fontSize,
                    alignment = TextAnchor.MiddleCenter,
                    wordWrap = true,
                    normal = { textColor = Color.white }
                };
            }
        }

        public void OnWindowGUI(int id)
        {
            if (!IsBusy)
                return;

            EnsureStyles();

            // Avanzar frame del spinner
            m_elapsed += Time.deltaTime;
            if (m_elapsed >= FrameRate)
            {
                m_elapsed -= FrameRate;
                m_frame = (m_frame + 1) % Frames.Length;
            }

            m_borderColor = new Color(m_borderColor.r, m_borderColor.g, m_borderColor.b, opacity);
            m_backgroundColor = new Color(m_backgroundColor.r, m_backgroundColor.g, m_backgroundColor.b, opacity);
            var rect = new Rect();

            //Fill
            var oldColor = GUI.color;
            rect = new Rect(0, 0, window.width, window.height);
            GUI.DrawTexture(rect, m_borderTexture, ScaleMode.StretchToFill, true, 0f, m_backgroundColor, Mathf.Infinity, m_borderRadius);

            //Border
            GUI.DrawTexture(rect, m_borderTexture, ScaleMode.StretchToFill, true, 0f, m_borderColor, m_borderWidth, m_borderRadius);
            GUI.color = oldColor;

            m_labelStyle.fontSize = fontSize;
            m_labelStyle.normal.textColor = m_textColor;

            GUILayout.BeginArea(new Rect(window.width/4, window.height/4, window.width * 0.5f, window.height * 0.5f), GUI.skin.box);
            
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            GUILayout.Label(Frames[m_frame], m_spinnerStyle);
            GUILayout.Space(10);
            GUILayout.Label(Label, m_labelStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();

            GUILayout.EndArea();

        }
    }
}
