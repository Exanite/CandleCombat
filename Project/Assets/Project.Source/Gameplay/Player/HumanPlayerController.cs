using Project.Source.Input;
using UniDi;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Source.Gameplay.Player
{
    public class HumanPlayerController : PlayerController
    {
        [Header("Dependencies")]
        [SerializeField] private InputActionReference movementInput;
        [SerializeField] private InputActionReference pointerInput;

        [SerializeField] private InputActionReference burningShotInput;
        [SerializeField] private InputActionReference soulTransferInput;
        [SerializeField] private InputActionReference dodgeInput;
        
        [SerializeField] private InputActionReference shootInput;
        [SerializeField] private InputActionReference reloadInput;
        
        [SerializeField] private InputActionReference quitInput;

        private Plane Plane => new Plane(Vector3.up, Vector3.up);

        [Inject]
        private Camera mainCamera;

        protected override void GatherInput(PlayerInputData input)
        {
            Vector3 pointerPosition = pointerInput.action.ReadValue<Vector2>();
            input.TargetPosition = PointerToWorldPosition(pointerPosition);
            input.MovementDirection = movementInput.action.ReadValue<Vector2>();
            
            input.IsBurningShotPressed = burningShotInput.action.IsPressed();
            input.IsSoulTransferPressed = soulTransferInput.action.IsPressed();
            input.IsDodgePressed = dodgeInput.action.IsPressed();
            
            input.IsShootPressed = shootInput.action.IsPressed();
            input.IsReloadPressed = reloadInput.action.IsPressed();

            if (quitInput.action.WasPressedThisFrame())
            {
                Application.Quit();
            }
        }

        private Vector3 PointerToWorldPosition(Vector3 pointerPosition)
        {
            var ray = mainCamera.ScreenPointToRay(pointerPosition);
            if (Plane.Raycast(ray, out var distance))
            {
                return ray.GetPoint(distance);
            }

            return transform.position;
        }
    }
}