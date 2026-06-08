using UnityEngine;

namespace GuiEngine
{
    public class BaseSpinner : MonoBehaviour
    {
        public RectOffset padding;

        private GUIStyle spinerStyle;

        private BaseWindow m_window;
        protected BaseWindow window
        {
            get
            {
                if (!m_window)
                {
                    m_window = GetComponent<BaseWindow>();
                }
                return m_window;
            }
        }

        protected Rect ApplyPadding(Rect rect)
        {
            if (spinerStyle == null)
            {
                spinerStyle = new GUIStyle();
            }
            spinerStyle.padding = padding;

            return spinerStyle.padding.Remove(rect);
        }

    }
}
