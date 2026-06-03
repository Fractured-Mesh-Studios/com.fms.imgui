using UnityEngine;

namespace GuiEngine
{
    public class WindowMobile : MonoBehaviour
    {
        // Definimos la resolución en la que diseñamos la interfaz (ej. Full HD vertical)
        private readonly float baseWidth = 1080f;
        private readonly float baseHeight = 1920f;

        private Matrix4x4 svMat;

        public void BeginMobile()
        {
            svMat = GUI.matrix;

            float scaleX = Screen.width / baseWidth;
            float scaleY = Screen.height / baseHeight;

            GUI.matrix = Matrix4x4.Scale(new Vector3(scaleX, scaleY, 1f));
        }

        public void EndMobile()
        {
            GUI.matrix = svMat;
        }
    }
}
