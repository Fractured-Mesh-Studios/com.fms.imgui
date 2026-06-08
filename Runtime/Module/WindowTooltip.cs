using UnityEngine;

namespace GuiEngine
{
    public class WindowTooltip : MonoBehaviour
    {
        public static string tooltip = "";

        [SerializeField] private string laTooltip;

        // Estilo personalizado para el Box del Tooltip (opcional)
        private GUIStyle tooltipStyle;

        private void OnGUI()
        {
            // 1. Dibujamos los elementos de la interfaz normalmente
            if (string.IsNullOrEmpty(tooltip))
            {
                return;
            }

            laTooltip = tooltip;

            // 2. RENDER: Solo dibujamos en el evento 'Repaint' para evitar parpadeos
            // y asegurarnos de que ya pasó por la lectura de todos los demás scripts.
            if (Event.current.type == EventType.Repaint)
            {
                if (!string.IsNullOrEmpty(tooltip))
                {
                    DrawTooltip(tooltip);

                    // Limpiamos para el próximo frame. Si el mouse sigue ahí, 
                    // el otro script lo volverá a llenar en el siguiente ciclo.
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
            GUI.depth = -1000;
            GUI.Box(rect, content, tooltipStyle);
        }
    }
}
