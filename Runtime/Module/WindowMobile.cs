using UnityEngine;

namespace GuiEngine
{
    public class WindowMobile : MonoBehaviour
    {
        private enum ScaleMode
        {
            FitScreen,
            FitWidth,
            FitHeight,
            Uniform
        }

        [Header("Scaling")]
        [SerializeField] private ScaleMode scaleMode = ScaleMode.Uniform;
        [SerializeField, Range(0.5f, 3f)] private float scaleMultiplier = 1f;
        [SerializeField] private float minScale = 0.5f;
        [SerializeField] private float maxScale = 3f;

        [Header("DPI Scaling")]
        [SerializeField] private bool useDpiScaling = false;
        [SerializeField] private float referenceDpi = 160f;

        [Header("Base Resolution")]
        [SerializeField] private float baseWidth = 1080f;
        [SerializeField] private float baseHeight = 1920f;

        private Matrix4x4 svMat;

        public Vector2 CurrentScale { get; private set; } = Vector2.one;

        public Rect GuiViewportRect
        {
            get
            {
                var safeWidth = Mathf.Max(1f, Screen.width / Mathf.Max(CurrentScale.x, 0.0001f));
                var safeHeight = Mathf.Max(1f, Screen.height / Mathf.Max(CurrentScale.y, 0.0001f));
                return new Rect(0f, 0f, safeWidth, safeHeight);
            }
        }

        public void BeginMobile()
        {
            svMat = GUI.matrix;

            var scaleX = Screen.width / Mathf.Max(1f, baseWidth);
            var scaleY = Screen.height / Mathf.Max(1f, baseHeight);
            var dpiFactor = useDpiScaling && Screen.dpi > 0f && referenceDpi > 0f ? Screen.dpi / referenceDpi : 1f;

            if (scaleMode == ScaleMode.FitScreen)
            {
                var finalScaleX = Mathf.Clamp(scaleX * scaleMultiplier * dpiFactor, minScale, maxScale);
                var finalScaleY = Mathf.Clamp(scaleY * scaleMultiplier * dpiFactor, minScale, maxScale);
                CurrentScale = new Vector2(finalScaleX, finalScaleY);
                GUI.matrix = Matrix4x4.Scale(new Vector3(finalScaleX, finalScaleY, 1f));
                return;
            }

            var scale = scaleMode switch
            {
                ScaleMode.FitWidth => scaleX,
                ScaleMode.FitHeight => scaleY,
                _ => Mathf.Min(scaleX, scaleY)
            };

            scale = Mathf.Clamp(scale * scaleMultiplier * dpiFactor, minScale, maxScale);
            CurrentScale = new Vector2(scale, scale);
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
        }

        public void EndMobile()
        {
            GUI.matrix = svMat;
        }
    }
}
