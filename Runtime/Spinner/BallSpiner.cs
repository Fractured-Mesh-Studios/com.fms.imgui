using GuiEngine;
using System;
using System.ComponentModel;
using UnityEngine;

[Serializable]
public class BallSpinner : BaseSpinner, IWindowModule
{
    // --- Configuración del Spinner (Mapeado a la imagen) ---
    [Header("Spinner Settings")]
    public float width = 30f;        // Radio del polígono/órbita
    public float ballRadius = 5f;    // Radio de cada bolita
    [Range(1, 12)]
    public int ballCount = 4;        // Cantidad de bolitas
    public float speed = 1f;         // Velocidad de giro

    [Header("Style & Colors")]
    public Color[] palette = new Color[] {
            new Color(0.96f, 0.41f, 0.42f), // Coral/Rojo
            new Color(0.98f, 0.74f, 0.02f), // Amarillo/Naranja
            new Color(0.24f, 0.84f, 0.65f), // Verde/Cian
            new Color(0.18f, 0.71f, 0.82f)  // Azul
        };

    // --- Recursos Visuales ---
    // Asigná una textura blanca de un círculo con borde suave (Soft Circle) en el inspector.
    // Si la dejás vacía, el script generará texturas cuadradas planas por defecto.
    public Texture2D ballTexture;
    private Texture2D bgTexture;
    private float currentAngle = 0f;

    private void Start()
    {
        // Crear textura de fondo sólida de 1x1 si no es transparente
        bgTexture = new Texture2D(1, 1);
        bgTexture.SetPixel(0, 0, Color.white);
        bgTexture.Apply();

        // Si no se asignó textura para las bolitas, creamos una por defecto (cuadrada)
        if (ballTexture == null)
        {
            ballTexture = new Texture2D(1, 1);
            ballTexture.SetPixel(0, 0, Color.white);
            ballTexture.Apply();
        }
    }

    private void Update()
    {
        // Actualizamos la rotación global del spinner en base al tiempo y velocidad
        // Multiplicamos por 100 para que el valor "1" de velocidad se sienta natural
        currentAngle += speed * 100f * Time.deltaTime;
        if (currentAngle >= 360f) currentAngle -= 360f;
    }

    private void DrawSpinner(Rect container)
    {
        // Guardar el estado original de la GUI para no romper otros layouts
        Color originalColor = GUI.color;
        Matrix4x4 originalMatrix = GUI.matrix;

        // 1. Dibujar el contenedor (Canvas) en el centro de la pantalla
        //Rect canvasRect = new Rect((Screen.width - size) / 2f, (Screen.height - size) / 2f, size, size);

        // Centro del canvas para pivotar los cálculos de las bolitas
        Vector2 center = new Vector2(container.x + (container.width / 2f), container.y + (container.height / 2f));

        // 2. Dibujar las Bolitas en base a la configuración
        for (int i = 0; i < ballCount; i++)
        {
            // Calculamos el ángulo base distribuido simétricamente para cada bolita
            // El ángulo final suma el 'currentAngle' para generar la animación de rotación
            float angleOffset = (360f / ballCount) * i;
            float radians = (currentAngle + angleOffset) * Mathf.Deg2Rad;

            // Posición X e Y usando coordenadas polares con respecto al centro
            float posX = center.x + Mathf.Cos(radians) * width;
            float posY = center.y + Mathf.Sin(radians) * width;

            // Definir el rectángulo que ocupa la bolita (centrado en su eje)
            float diameter = ballRadius * 2f;
            Rect ballRect = new Rect(posX - ballRadius, posY - ballRadius, diameter, diameter);

            // Elegir el color de la paleta cíclicamente
            Color ballColor = palette[i % palette.Length];

            // Tintar y dibujar la bolita
            GUI.color = ballColor;
            GUI.DrawTexture(ballRect, ballTexture);
        }

        // Restaurar la configuración original de la GUI
        GUI.color = originalColor;
    }

    public void OnWindowGUI(int id)
    {
        if (!enabled) return;

        Rect localRect = new Rect(0, 0, window.width, window.height);

        localRect = ApplyPadding(localRect);

        DrawSpinner(localRect);
    }
}