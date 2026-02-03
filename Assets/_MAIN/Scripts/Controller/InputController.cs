using Gameplay.Core.Controllers;
using Gameplay.Views;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay.Core.Controllers
{
    public class InputController : MonoBehaviour
    {
        [Required, SerializeField]
        private Camera MainCamera;

        [Required, SerializeField]
        private LevelController LevelController;

        [SerializeField] private LayerMask InteractableLayer;

        [SerializeField, BoxGroup("Settings")]
        private float interactionCooldown = 0.15f;

        private float lastInteractionTime;

        private InputAction PressAction;
        private InputAction PositionAction;

        private void Awake()
        {
            PressAction = new InputAction(type: InputActionType.Button);
            PressAction.AddBinding("<Mouse>/leftButton");
            PressAction.AddBinding("<Touchscreen>/primaryTouch/press");

            // Configura a leitura de posição
            // PassThrough permite ler o valor a qualquer momento
            PositionAction = new InputAction(type: InputActionType.Value, expectedControlType: "Vector2");
            PositionAction.AddBinding("<Mouse>/position");
            PositionAction.AddBinding("<Touchscreen>/primaryTouch/position");
        }

        private void OnEnable()
        {
            PressAction.performed += OnInputPerformed;
            PressAction.Enable();
            PositionAction.Enable();
        }

        private void OnDisable()
        {
            PressAction.performed -= OnInputPerformed;
            PressAction.Disable();
            PositionAction.Disable();
        }


        private void OnInputPerformed(InputAction.CallbackContext _)
        {
            if (Time.time < lastInteractionTime + interactionCooldown) return;
            Vector2 ScreenPosition = MouseUtil.GetMousePosition();
            PerformRaycast(ScreenPosition);
        }

        private void PerformRaycast(Vector2 screenPosition)
        {
            Ray Ray = MainCamera.ScreenPointToRay(screenPosition);


            if (Physics.Raycast(Ray, out RaycastHit Hit, Mathf.Infinity, InteractableLayer))
            {
                if (Hit.collider.TryGetComponent(out NodeView Node))
                {
                    lastInteractionTime = Time.time;
                    LevelController.OnNodeInteraction(Node.XPosition, Node.YPosition);
                }
            }
        }
    }
}
