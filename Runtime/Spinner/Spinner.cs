using System;
using UnityEngine;

namespace GuiEngine
{
    [Serializable]
    public class Spinner : BaseSpinner, IWindowModule
    {
        [Header("Spinner")]
        public int totalPetals = 12;                        // Cantidad de rectángulos
        public float radius = 50f;                          // Radio del círculo
        public Vector2 petalSize = new Vector2(10f, 25f);   // Ancho y alto de cada pétalo
        public float speed = 5f;                            // Velocidad de giro

        [Header("Spinner Color")]
        public Color baseColor = Color.bisque;              // El rosa de tu imagen

        private void DrawSpinner(Rect container)
        {
            float currentTime = Time.realtimeSinceStartup * speed;

            for (int i = 0; i < totalPetals; i++)
            {
                // 1. Ángulo de distribución del pétalo
                float angle = i * (2f * Mathf.PI / totalPetals);

                // 2. Posición del pétalo usando el radio (Píxeles puros y directos)
                float x = container.x + Mathf.Cos(angle) * radius;
                float y = container.y + Mathf.Sin(angle) * radius;

                // Creamos el Rect del pétalo centrado en su propia posición calculada
                Rect petalRect = new Rect(x - (petalSize.x / 2f), y - (petalSize.y / 2f), petalSize.x, petalSize.y);

                // 3. Animación del color (Alpha)
                float petalProgress = (angle / (2f * Mathf.PI));
                float timeOffset = currentTime - (petalProgress * speed);
                float alpha = Mathf.Repeat(timeOffset, 1f);
                alpha = Mathf.Lerp(0.1f, 1f, alpha);

                // 4. Ángulo de rotación en grados (apuntando al centro)
                float angleDegrees = angle * Mathf.Rad2Deg + 90f;

                // 5. DIBUJO SEGURO: Guardamos el color, rotamos alrededor del pétalo y dibujamos
                Color oldColor = GUI.color;
                GUI.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);

                // Guardamos el estado de la GUI antes de rotar
                Matrix4x4 matrixBackup = GUI.matrix;

                // Rotamos la interfaz usando el centro exacto de ESTE pétalo como pivote
                GUIUtility.RotateAroundPivot(angleDegrees, new Vector2(x, y));

                // Dibujamos la textura (un rectángulo plano)
                GUI.DrawTexture(petalRect, Texture2D.whiteTexture);

                // Restauramos la matriz y el color inmediatamente para el siguiente pétalo
                GUI.matrix = matrixBackup;
                GUI.color = oldColor;
            }
        }

        public void OnWindowGUI(int id)
        {
            if (!enabled) return;

            int x = window.width / 2;
            int y = window.height / 2;

            Rect rect = new Rect(x, y, window.rect.width, window.rect.height);

            DrawSpinner(rect);
        }
    }
}
