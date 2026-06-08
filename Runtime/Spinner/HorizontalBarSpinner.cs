using UnityEngine;

namespace GuiEngine
{
    public class HorizontalBarSpinner : BaseSpinner, IWindowModule
    {
        [System.Serializable]
        public class BallData
        {
            public float currentScale = 1f;
            public float velocity = 0f;
            public float delayTimer = 0f;
        }

        [Header("Grid/Bar Settings")]
        public float size = 200f;             // Tamaño del contenedor/canvas blanco
        public float ballRadius = 15f;        // Radio base de cada círculo
        public float spacing = 12f;           // Espacio entre círculos vecinos
        public int ballCount = 4;             // Cantidad de bolitas (dinámico)
        public Color[] palette = new Color[4] {
            new Color(0.87f, 0.33f, 0.35f),    // Coral/Rojo
            new Color(0.97f, 0.69f, 0.35f),    // Naranja/Amarillo
            new Color(0.61f, 0.74f, 0.49f),    // Verde apagado
            new Color(0.48f, 0.62f, 0.73f)     // Azul grisáceo
        };

        [Header("Spring Animation Settings")]
        public float springStiffness = 200f;  // Fuerza de recuperación del resorte
        public float springDamping = 14f;     // Amortiguación del rebote
        public float targetScale = 1.6f;      // Escala máxima del Zoom interno
        public float delayBetweenBalls = 0.12f; // Tiempo de desfase de Izquierda a Derecha
        public float cycleCooldown = 0.3f;    // Pausa antes de reiniciar la ola desde la izquierda

        public Texture2D ballTexture;         // Asigná un círculo PNG blanco con Alpha suave
        private Texture2D flatTexture;
        private BallData[] balls;
        private int currentAnimatingIndex = 0;
        private float cycleTimer = 0f;
        private bool isCycleActive = true;

        private void Start()
        {
            // Textura fallback de fondo
            flatTexture = new Texture2D(1, 1);
            flatTexture.SetPixel(0, 0, Color.white);
            flatTexture.Apply();

            // Si no hay textura de círculo suave asignada, usamos un cuadrado plano por defecto
            if (ballTexture == null)
            {
                ballTexture = flatTexture;
            }

            ResetSetup();
        }

        // Permitir reconfigurar la cantidad de bolitas en runtime de forma segura
        public void ResetSetup()
        {
            balls = new BallData[ballCount];
            for (int i = 0; i < ballCount; i++)
            {
                balls[i] = new BallData();
            }
            currentAnimatingIndex = 0;
            isCycleActive = true;
        }

        private void Update()
        {
            // Seguridad por si cambian el ballCount desde el inspector en runtime
            if (balls == null || balls.Length != ballCount) 
                ResetSetup();

            float dt = Time.deltaTime;

            // 1. Secuencia de la Ola (De izquierda a derecha)
            if (isCycleActive)
            {
                balls[currentAnimatingIndex].delayTimer += dt;
                if (balls[currentAnimatingIndex].delayTimer >= delayBetweenBalls)
                {
                    // Impacto violento de escala (Zoom-in)
                    balls[currentAnimatingIndex].currentScale = targetScale;
                    balls[currentAnimatingIndex].velocity = 0f;
                    balls[currentAnimatingIndex].delayTimer = 0f;

                    currentAnimatingIndex++;
                    if (currentAnimatingIndex >= ballCount)
                    {
                        isCycleActive = false; // Fin de la ola actual
                        cycleTimer = 0f;
                    }
                }
            }
            else
            {
                // Cooldown antes de la siguiente ráfaga
                cycleTimer += dt;
                if (cycleTimer >= cycleCooldown)
                {
                    currentAnimatingIndex = 0;
                    isCycleActive = true;
                }
            }

            // 2. Simulación física independiente para cada bolita
            for (int i = 0; i < ballCount; i++)
            {
                float displacement = balls[i].currentScale - 1f; // Target natural es 1.0
                float springForce = -springStiffness * displacement;
                float dampingForce = -springDamping * balls[i].velocity;

                float acceleration = springForce + dampingForce;
                balls[i].velocity += acceleration * dt;
                balls[i].currentScale += balls[i].velocity * dt;
            }
        }

        private void DrawSpinner(Rect canvasRect, Rect windowRect)
        {
            Color originalColor = GUI.color;
            Matrix4x4 screenMatrix = GUI.matrix; // Matriz global absoluta de pantalla

            // 2. Calcular la línea horizontal centrada
            float diameter = ballRadius * 2f;
            // Ancho total que ocupa la hilera completa de círculos
            float totalWidth = (ballCount * diameter) + ((ballCount - 1) * spacing);

            // Punto inicial X (Izquierda) para que toda la hilera quede centrada en el canvas
            float startX = canvasRect.center.x - (totalWidth / 2f);
            float centerY = canvasRect.center.y;

            // 3. Renderizar cada círculo con su pivote de matriz corregido
            for (int i = 0; i < ballCount; i++)
            {
                // Calcular el centro local de esta bolita específica
                float localCenterX = startX + (i * (diameter + spacing)) + ballRadius;
                Vector2 localCenter = new Vector2(localCenterX, centerY);

                float scale = balls[i].currentScale;

                // Rectángulo base de dibujo (sin escala)
                Rect ballRect = new Rect(localCenter.x - ballRadius, localCenter.y - ballRadius, diameter, diameter);

                // Sincronizar el pivote local al espacio de pantalla absoluto agregando windowRect.position
                Vector2 screenCenter = localCenter + windowRect.position;

                // Construcción de la matriz TRS enfocada en la pantalla
                Matrix4x4 scaleMatrix = Matrix4x4.TRS(screenCenter, Quaternion.identity, new Vector3(scale, scale, 1f))
                                      * Matrix4x4.TRS(-screenCenter, Quaternion.identity, Vector3.one);

                // Multiplicación correcta: Mantiene la ventana estable y escala el elemento local
                GUI.matrix = screenMatrix * scaleMatrix;

                // Pintar con color de paleta cíclico y dibujar textura circular
                GUI.color = palette[i % palette.Length];
                GUI.DrawTexture(ballRect, ballTexture);
            }

            // Restaurar estados nativos de la GUI de Unity
            GUI.matrix = screenMatrix;
            GUI.color = originalColor;
        }

        public void OnWindowGUI(int id)
        {
            if (!enabled) return;

            // Canvas cuadrado centrado en el layout de la ventana de tu GUI Engine
            Rect localRect = new Rect(
                (window.width - size) / 2f,
                (window.height - size) / 2f,
                size,
                size
            );

            // Inyectamos el canvas local y el Rect global de la Ventana (window)
            DrawSpinner(localRect, window.rect);
        }
    }
}
