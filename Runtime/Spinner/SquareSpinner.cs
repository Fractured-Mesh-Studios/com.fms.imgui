using System;
using UnityEngine;

namespace GuiEngine
{
    public class SquareSpinner : BaseSpinner, IWindowModule
    {
        [Header("Configuración de la Grilla")]
        public int rows = 3;
        public int columns = 3;
        public float cellSize = 30f;
        public float spacing = 15f;

        [Header("Configuración de Animación")]
        public float speed = 3f;
        [Tooltip("Desfase de la animación entre celdas consecutivas")]
        public float waveOffset = 0.5f;
        public float minScale = 0.3f;
        public float maxScale = 1.0f;

        [Header("Colores de la Onda")]
        // Colores basados en tu imagen (de inicio a fin de la onda)
        public Color startColor = new Color(0.9f, 0.35f, 0.35f);  // Rojizo/Coral
        public Color midColor = new Color(0.95f, 0.65f, 0.4f);   // Naranja/Amarillento
        public Color endColor = new Color(0.5f, 0.6f, 0.5f);     // Verde Opaco

        private Texture2D _whiteTexture;

        private void DrawSpinner(Rect container)
        {
            if (!_whiteTexture)
            {
                // Creamos una textura blanca de 1x1 para poder pintarla con GUI.color
                _whiteTexture = new Texture2D(1, 1);
                _whiteTexture.SetPixel(0, 0, Color.white);
                _whiteTexture.Apply();
            }

            // Llamamos al método pasándole el espacio asignado
            DrawSpinner_Internal(container);
        }

        private void DrawSpinner_Internal(Rect container)
        {
            int rows = 3;
            int columns = 3;

            float spacingRatio = 0.15f;

            float availableWidth = container.width / (columns + (columns - 1) * spacingRatio);
            float availableHeight = container.height / (rows + (rows - 1) * spacingRatio);

            // Tamaño máximo que puede tener un cuadrado completo (escala 1.0)
            float maxCellSize = Mathf.Min(availableWidth, availableHeight);
            float spacing = maxCellSize * spacingRatio;

            float totalGridWidth = (columns * maxCellSize) + ((columns - 1) * spacing);
            float totalGridHeight = (rows * maxCellSize) + ((rows - 1) * spacing);

            // Esquina superior izquierda de la grilla (relativa al contenedor de la ventana)
            float startX = container.x + (container.width - totalGridWidth) / 2f;
            float startY = container.y + (container.height - totalGridHeight) / 2f;

            float time = Time.time * speed;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    float waveFactor = (r + c) * waveOffset;
                    float cycle = Mathf.Sin(time - waveFactor);
                    float t = (cycle + 1f) / 2f;

                    // 1. Calculamos el tamaño que debe tener el cuadrado en este frame
                    float currentScale = Mathf.Lerp(minScale, maxScale, t);
                    float currentSize = maxCellSize * currentScale;

                    // 2. Calculamos la posición del centro teórico de la celda
                    float cellCenterX = startX + (c * (maxCellSize + spacing)) + (maxCellSize / 2f);
                    float cellCenterY = startY + (r * (maxCellSize + spacing)) + (maxCellSize / 2f);

                    // 3. Modificamos el origen (X, Y) del Rect para que dibuje desde el centro
                    // Restamos la mitad del tamaño actual para centrarlo
                    float drawX = cellCenterX - (currentSize / 2f);
                    float drawY = cellCenterY - (currentSize / 2f);

                    // 4. Creamos el Rect final ya posicionado y escalado
                    Rect drawRect = new Rect(drawX, drawY, currentSize, currentSize);

                    // 5. Pintamos y dibujamos directo, sin alterar el contexto de la ventana
                    GUI.color = GetWaveColor(t);
                    GUI.DrawTexture(drawRect, _whiteTexture);
                }
            }

            // Al final solo reseteamos el color global
            GUI.color = Color.white;
        }

        // Devuelve el color correspondiente según el punto de la animación
        private Color GetWaveColor(float t)
        {
            // Dividimos la transición en dos partes (Start -> Mid -> End)
            if (t < 0.5f)
            {
                return Color.Lerp(endColor, midColor, t * 2f);
            }
            else
            {
                return Color.Lerp(midColor, startColor, (t - 0.5f) * 2f);
            }
        }


        public void OnWindowGUI(int id)
        {
            if (!enabled) return;

            Rect localRect = new Rect(0,0,window.width, window.height);

            localRect = ApplyPadding(localRect);

            DrawSpinner(localRect);
        }
    }
}
