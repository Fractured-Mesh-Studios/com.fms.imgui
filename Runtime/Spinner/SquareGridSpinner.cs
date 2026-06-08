using UnityEngine;

namespace GuiEngine
{
    public class SquareGridSpinner : BaseSpinner, IWindowModule
    {
        [System.Serializable]
        public class SquareData
        {
            public float currentScale = 1f;
            public float velocity = 0f;
            public float delayTimer = 0f;
        }

        [Header("Grid Settings")]
        public float size = 200f;             // Tamaño del contenedor/canvas blanco
        public float squareSize = 70f;        // Tamaño base de cada cuadrado
        public float spacing = 15f;           // Espacio de separación entre cuadrados
        public Color[] palette = new Color[4] {
            new Color(0.44f, 0.0f, 0.49f),    // Violeta Oscuro (Arriba-Izquierda)
            new Color(0.65f, 0.23f, 0.74f),   // Violeta Medio (Arriba-Derecha)
            new Color(0.9f, 0.55f, 0.95f),    // Rosa Claro (Abajo-Derecha)
            new Color(0.84f, 0.42f, 0.88f)    // Violeta Claro (Abajo-Izquierda)
        };

        [Header("Spring Animation Settings")]
        public float springStiffness = 180f;  // Fuerza del resorte (fuerza del "spring violentamente")
        public float springDamping = 12f;     // Amortiguación (qué tan rápido frena y vuelve al original)
        public float targetScale = 1.4f;      // Escala máxima a la que salta el spring
        public float delayBetweenSquares = 0.15f; // Desfase en segundos entre cada cuadrado
        public float cycleCooldown = 0.4f;    // Pausa en segundos antes de reiniciar el ciclo completo

        private SquareData[] squares;
        private int currentAnimatingIndex = 0;
        private float cycleTimer = 0f;
        private bool isCycleActive = true;
        private Texture2D flatTexture;

        private void Start()
        {
            // Creamos una textura plana de 1x1 para pintar los colores mediante GUI.color
            flatTexture = new Texture2D(1, 1);
            flatTexture.SetPixel(0, 0, Color.white);
            flatTexture.Apply();

            // Inicializamos los 4 cuadrados
            squares = new SquareData[4];
            for (int i = 0; i < 4; i++)
            {
                squares[i] = new SquareData();
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            // 1. Gestionar la secuencia de disparo en sentido horario
            // Los cuadrados permanecen fijos; el "foco" de la animación secuencial es lo que se mueve.
            if (isCycleActive)
            {
                squares[currentAnimatingIndex].delayTimer += dt;
                if (squares[currentAnimatingIndex].delayTimer >= delayBetweenSquares)
                {
                    // Disparamos el "impacto" de escala en el cuadrado actual
                    // Esto es lo que "rota": el índice del cuadrado al que le toca dispararse.
                    squares[currentAnimatingIndex].currentScale = targetScale;
                    squares[currentAnimatingIndex].velocity = 0f; // Reset de velocidad para el impacto limpio
                    squares[currentAnimatingIndex].delayTimer = 0f;

                    currentAnimatingIndex++;
                    if (currentAnimatingIndex >= 4)
                    {
                        isCycleActive = false; // Terminó la ronda de los 4
                        cycleTimer = 0f;
                    }
                }
            }
            else
            {
                // Espera para iniciar la siguiente vuelta de aguja de reloj
                cycleTimer += dt;
                if (cycleTimer >= cycleCooldown)
                {
                    currentAnimatingIndex = 0;
                    isCycleActive = true;
                }
            }

            // 2. Simulación física del Spring (Hooke's Law + Damping) para todos los cuadrados
            for (int i = 0; i < 4; i++)
            {
                // Fuerza de restitución hacia el valor original (escala = 1.0)
                float displacement = squares[i].currentScale - 1f;
                float springForce = -springStiffness * displacement;
                float dampingForce = -springDamping * squares[i].velocity;

                float acceleration = springForce + dampingForce;
                squares[i].velocity += acceleration * dt;
                squares[i].currentScale += squares[i].velocity * dt;
            }
        }

        private void DrawSpinner(Rect canvasRect, Rect windowRect)
        {
            Color originalColor = GUI.color;
            Matrix4x4 screenMatrix = GUI.matrix; // Matriz global de la pantalla

            Vector2 center = canvasRect.center;
            float offset = (squareSize / 2f) + (spacing / 2f);

            // Centros locales de los cuadrados
            Vector2[] squareCenters = new Vector2[4] {
                new Vector2(center.x - offset, center.y - offset),
                new Vector2(center.x + offset, center.y - offset),
                new Vector2(center.x + offset, center.y + offset),
                new Vector2(center.x - offset, center.y + offset)
            };

            for (int i = 0; i < 4; i++)
            {
                Vector2 localCenter = squareCenters[i];
                float scale = squares[i].currentScale;

                // Rectángulo local para dibujar el cuadrado
                Rect sqRect = new Rect(localCenter.x - squareSize / 2f, localCenter.y - squareSize / 2f, squareSize, squareSize);

                // EXPLICACIÓN DE LA MATRIZ:
                // Como GUI.matrix opera a nivel PANTALLA, el pivote tiene que ser en coordenadas de pantalla.
                // Convertimos el centro local a centro de pantalla sumándole el offset X e Y de la ventana (windowRect.position)
                Vector2 screenCenter = localCenter + windowRect.position;

                // Construimos la matriz de escalado usando el punto exacto de la pantalla
                Matrix4x4 scaleMatrix = Matrix4x4.TRS(screenCenter, Quaternion.identity, new Vector3(scale, scale, 1f))
                                      * Matrix4x4.TRS(-screenCenter, Quaternion.identity, Vector3.one);

                // Aplicamos la matriz sobre la de la pantalla
                GUI.matrix = screenMatrix * scaleMatrix;

                // Pintar y dibujar el cuadrado
                GUI.color = palette[i % palette.Length];
                GUI.DrawTexture(sqRect, flatTexture);
            }

            // Restauramos la matriz global de la pantalla
            GUI.matrix = screenMatrix;
            GUI.color = originalColor;
        }

        public void OnWindowGUI(int id)
        {
            if(!enabled) return;

            Rect localRect = new Rect(0, 0, window.width, window.height);

            DrawSpinner(localRect, window.rect);
        }
    }
}
