using UnityEngine;

namespace GuiEngine
{
    public class StripeSpinner : BaseSpinner, IWindowModule
    {
        [Header("Stripe Bar Settings")]
        public float width = 160f;            // Ancho de la barra de progreso
        public float height = 34f;            // Alto de la barra de progreso
        public float stroke = 5f;             // Grosor del borde/borde exterior
        public int stripeWidthPixels = 20;    // Ancho en píxeles de cada franja diagonal

        [Header("Style & Colors")]
        public Color color1 = new Color(0.62f, 0.60f, 0.78f); // Franja Clara (#9e9ac8)
        public Color color2 = new Color(0.46f, 0.42f, 0.69f); // Franja Oscura (#756bb1)
        public Color strokeColor = new Color(0.33f, 0.28f, 0.56f); // Color del Borde (#54278f)

        [Header("Animation Settings")]
        public float speed = 1.5f;            // Velocidad del desplazamiento de las franjas

        private Texture2D stripeTexture;      // Textura repetitiva generada dinámicamente
        private Texture2D flatTexture;        // Textura base de 1x1 para bordes y fondos
        private float textureOffset = 0f;

        private void Start()
        {
            // 1. Crear la textura plana para fondos y bordes sólidos
            flatTexture = new Texture2D(1, 1);
            flatTexture.SetPixel(0, 0, Color.white);
            flatTexture.Apply();

            // 2. Generar la textura procedimental de franjas diagonales a 45 grados
            GenerateStripeTexture();
        }

        private void GenerateStripeTexture()
        {
            // El tamaño de la textura patrón debe ser el doble del ancho de la franja 
            // para poder ciclar color1 y color2 perfectamente de forma simétrica.
            int texSize = stripeWidthPixels * 2;
            stripeTexture = new Texture2D(texSize, texSize);
            stripeTexture.wrapMode = TextureWrapMode.Repeat;
            stripeTexture.filterMode = FilterMode.Bilinear;

            for (int y = 0; y < texSize; y++)
            {
                for (int x = 0; x < texSize; x++)
                {
                    // La magia matemática para los 45°: (x + y) determinan la diagonal.
                    // Al usar el operador residuo (%), dividimos el patrón en dos mitades perfectas.
                    if ((x + y) % texSize < stripeWidthPixels)
                    {
                        stripeTexture.SetPixel(x, y, color1);
                    }
                    else
                    {
                        stripeTexture.SetPixel(x, y, color2);
                    }
                }
            }
            stripeTexture.Apply();
        }

        private void Update()
        {
            // Desplazamiento infinito basado en el tiempo.
            // Restamos para que el movimiento de las líneas vaya de izquierda a derecha de forma natural.
            textureOffset -= speed * Time.deltaTime;

            // Mantener el offset dentro del rango de 0 a 1 para evitar floats gigantes con las horas
            if (textureOffset < -1f) textureOffset += 1f;
        }

        private void DrawSpinner(Rect canvasRect, Rect windowRect)
        {
            Color originalColor = GUI.color;
            Matrix4x4 screenMatrix = GUI.matrix; // Matriz global absoluta de pantalla

            // --- 1. Calcular Rectángulos de la barra centrada ---
            // Área de la barra exterior (Borde)
            Rect outerBarRect = new Rect(
                canvasRect.center.x - (width / 2f),
                canvasRect.center.y - (height / 2f),
                width,
                height
            );

            // Área de las franjas internas (restando el grosor del borde en todos los lados)
            Rect innerBarRect = new Rect(
                outerBarRect.x + stroke,
                outerBarRect.y + stroke,
                outerBarRect.width - (stroke * 2f),
                outerBarRect.height - (stroke * 2f)
            );

            // --- 2. Sincronización de Matrices para GUI.Window ---
            // Obtenemos la posición absoluta de la barra en la pantalla
            Vector2 screenPivot = outerBarRect.center + windowRect.position;

            // En este spinner no escalamos, pero mantenemos la transformación TRS para asegurar 
            // que la posición se fije sin glitches en la pantalla sin importar el movimiento de la ventana
            Matrix4x4 positionMatrix = Matrix4x4.TRS(screenPivot, Quaternion.identity, Vector3.one)
                                        * Matrix4x4.TRS(-screenPivot, Quaternion.identity, Vector3.one);

            GUI.matrix = screenMatrix * positionMatrix;

            // --- 3. Renderizado --

            // B. Dibujar el Rectángulo del Borde (Borde exterior cuadrado)
            GUI.color = strokeColor;
            GUI.DrawTexture(outerBarRect, flatTexture);

            // C. Dibujar las Franjas Animadas en base a coordenadas UV móviles
            GUI.color = Color.white; // Resetear tinte para no alterar la textura nativa generada

            // Calculamos cuántas veces se debe repetir el patrón de textura a lo largo del ancho de la barra
            float numRepeatsX = innerBarRect.width / stripeTexture.width;
            float numRepeatsY = innerBarRect.height / stripeTexture.height;

            // Definimos las coordenadas UV. Pasando textureOffset al componente X generamos el movimiento continuo.
            Rect uvCoordinates = new Rect(textureOffset, 0f, numRepeatsX, numRepeatsY);

            // Dibujar la textura interna recortada perfectamente por las coordenadas asignadas
            GUI.DrawTextureWithTexCoords(innerBarRect, stripeTexture, uvCoordinates);

            // Restaurar estados nativos de la GUI de Unity
            GUI.matrix = screenMatrix;
            GUI.color = originalColor;
        }

        public void OnWindowGUI(int id)
        {
            if (!enabled) return;

            // Canvas base que contiene el componente dentro de tu ventana
            // Usamos un tamaño cuadrado superior al width configurado para evitar recortes
            float canvasSize = Mathf.Max(width, height) + 40f;

            Rect localRect = new Rect(
                (window.width - canvasSize) / 2f,
                (window.height - canvasSize) / 2f,
                canvasSize,
                canvasSize
            );

            // Ejecutamos pasándole el layout local y el Rect real de la Window (window)
            DrawSpinner(localRect, window.rect);
        }

        // Método utilitario por si querés cambiar colores en runtime y regenerar la textura
        public void RefreshColors(Color c1, Color c2, Color strokeCol)
        {
            color1 = c1;
            color2 = c2;
            strokeColor = strokeCol;
            GenerateStripeTexture();
        }
    }
}

