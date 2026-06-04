using UnityEngine;
using UnityEngine.InputSystem;

namespace GuiEngine
{
    [RequireComponent(typeof(Window))]
    public class ToggleWindow : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputAction OnToggle;

        private Window m_window;
        protected Window window
        {
            get
            {
                if (m_window == null)
                    m_window = GetComponent<Window>();
                return m_window;
            }
        }

        private void OnEnable()
        {
            OnToggle.performed += OnToggleWindow;
            OnToggle.Enable();
        }

        private void OnDisable()
        {
            OnToggle.performed -= OnToggleWindow;
            OnToggle.Disable();
        }

        private void OnValidate()
        {
            if (OnToggle == null)
            {
                OnToggle = new InputAction("Toggle");
            }

            if (OnToggle.bindings.Count == 0 || string.IsNullOrEmpty(OnToggle.bindings[0].path))
            {
                while (OnToggle.bindings.Count > 0)
                {
                    OnToggle.ChangeBinding(0).Erase();
                }

                OnToggle.AddBinding("<Keyboard>/space");

                Debug.Log($"[Input] Se restableció la tecla por defecto en {gameObject.name}");
            }
        }

        private void OnToggleWindow(InputAction.CallbackContext context)
        {
            if(window.isOpen)
                window.Close();
            else
                window.Open();
        }
    }
}
