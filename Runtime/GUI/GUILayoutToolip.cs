using UnityEngine;

namespace GuiEngine
{
    public class GUILayoutToolip : MonoBehaviour
    {
        private GUIStyle tooltipStyle;

        [RuntimeInitializeOnLoadMethod]
        private static void OnLoad()
        {
            var obj = new GameObject("GUILayoutTooltip", typeof(GUILayoutToolip));
        }

        private void OnGUI()
        {
            GUI.Window(int.MinValue, new Rect(0, 0, Screen.width, Screen.height), OnWindowGUI, string.Empty, GUIStyle.none);
            GUI.BringWindowToFront(int.MinValue);
        }

        private void OnWindowGUI(int id)
        {
            if (Event.current.type == EventType.Repaint)
            {
                if (!string.IsNullOrEmpty(GUI.tooltip))
                {
                    DrawTooltip(GUI.tooltip);
                }
            }
        }

        private void DrawTooltip(string text)
        {
            GUI.depth = int.MinValue;

            if (tooltipStyle == null)
            {
                // Inicializa el estilo clásico del Box de Unity
                tooltipStyle = new GUIStyle(GUI.skin.box);
                tooltipStyle.alignment = TextAnchor.MiddleLeft;
                tooltipStyle.normal.textColor = Color.white;
                tooltipStyle.fontSize = 12;
                tooltipStyle.padding = new RectOffset(8, 8, 5, 5);
            }

            Vector2 mousePos = Event.current.mousePosition;
            GUIContent content = new GUIContent(text);

            // Calculamos el tamaño exacto del cuadro según el texto
            Vector2 size = tooltipStyle.CalcSize(content);

            // Desplazamiento para que el box no tape la punta del cursor
            float offsetX = 15f;
            float offsetY = 15f;

            // Evita que el tooltip se salga de la pantalla por la derecha o por abajo
            float xPos = mousePos.x + offsetX;
            float yPos = mousePos.y + offsetY;

            if (xPos + size.x > Screen.width) xPos = mousePos.x - size.x - 5f;
            if (yPos + size.y > Screen.height) yPos = mousePos.y - size.y - 5f;

            Rect rect = new Rect(xPos, yPos, size.x, size.y);

            // Forzamos a que se dibuje por encima de Absolutamente TODO
            GUI.depth = int.MaxValue;
            GUI.Box(rect, content, tooltipStyle);

            GUI.tooltip = string.Empty;
        }
    }
}

