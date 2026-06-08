using UnityEngine;

namespace GuiEngine
{
    public class TextureSpinner : BaseSpinner, IWindowModule
    {
        [Header("Spinner")]
        [Tooltip("Arrastra aquí la textura del spinner desde el Inspector")]
        public Texture2D spinnerTexture;

        [Tooltip("Velocidad de rotación (valores más altos = más rápido)")]
        public float rotationSpeed = 180f;

        [Tooltip("Color y opacidad del Spinner")]
        public Color spinnerColor = Color.white;

        private float currentAngle = 0f;

        private void Update()
        {
            currentAngle += rotationSpeed * Time.deltaTime;

            currentAngle %= 360f;
        }

        private void DrawSpinner(Rect container)
        {
            Vector2 pivotPoint = new Vector2(container.width / 2f, container.height / 2f);

            Matrix4x4 backupMatrix = GUI.matrix;

            GUI.color = spinnerColor;
            GUIUtility.RotateAroundPivot(currentAngle, pivotPoint);

            GUI.DrawTexture(container, spinnerTexture, ScaleMode.ScaleToFit, true);

            GUI.matrix = backupMatrix;
        }

        public void OnWindowGUI(int id)
        {
            if(!enabled) return;

            // Solo dibujamos si está activo y si tenemos una textura asignada
            if (spinnerTexture == null) 
                return;

            // Solo dibujamos en el evento Repaint para evitar tirones visuales
            if (Event.current.type == EventType.Repaint)
            {
                DrawSpinner(new Rect(0, 0, window.width, window.height));
            }
        }
    }
}
