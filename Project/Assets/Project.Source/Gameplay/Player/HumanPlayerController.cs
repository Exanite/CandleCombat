using Project.Source.Input;
using UniDi;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Project.Source.Gameplay.Player
{
    public class HumanPlayerController : PlayerController
    {
        [Header("Dependencies")]
        [FormerlySerializedAs("shootReference")]
        [SerializeField] private InputActionReference shootInput;
        [FormerlySerializedAs("movementReference")]
        [SerializeField] private InputActionReference movementInput;
        [FormerlySerializedAs("pointerReference")]
        [SerializeField] private InputActionReference pointerInput;
        [FormerlySerializedAs("dodgeReference")]
        [SerializeField] private InputActionReference dodgeInput;
        [FormerlySerializedAs("reloadReference")]
        [SerializeField] private InputActionReference reloadInput;
        [FormerlySerializedAs("quitReference")]
        [SerializeField] private InputActionReference quitInput;

        private Plane Plane => new Plane(Vector3.up, Vector3.up);

        [Inject]
        private GameContext gameContext;

        [Inject]
        private Camera mainCamera;

        protected override void GatherInput(PlayerInputData input)
        {
            Vector3 pointerPosition = pointerInput.action.ReadValue<Vector2>();
            input.TargetPosition = PointerToWorldPosition(pointerPosition);
            input.MovementDirection = movementInput.action.ReadValue<Vector2>();

            input.IsShootPressed = shootInput.action.IsPressed();
            input.IsDodgePressed = dodgeInput.action.IsPressed();
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