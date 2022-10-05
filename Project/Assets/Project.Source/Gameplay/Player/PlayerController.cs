using Project.Source.Gameplay.Characters;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Source.Gameplay.Player
{
    [RequireComponent(typeof(PlayerMovement), typeof(PlayerLook))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Dependencies")]
        public Camera MainCamera;
        [SerializeField] private InputActionReference shootReference;
        [SerializeField] private InputActionReference movementReference;
        [SerializeField] private InputActionReference pointerReference;
        [SerializeField] private InputActionReference dodgeReference;
        [SerializeField] private InputActionReference reloadReference;
        [SerializeField] private InputActionReference quitReference;

        private PlayerMovement playerMovement;
        private PlayerLook playerLook;
        private GunController gunController;
        
        private Plane Plane => new Plane(Vector3.up, Vector3.zero);

        private void Awake()
        {
            playerMovement = GetComponent<PlayerMovement>();
            playerLook = GetComponent<PlayerLook>();
            gunController = GetComponent<GunController>();
        }

        private void Update()
        {
            Character character = GameContext.Instance.CurrentPlayer;

            if (character == null) return;
        
            playerMovement.SetCharacter(character);
            playerLook.SetCharacter(character);
            gunController.SetCharacter(character);

            playerMovement.SetMoveDirection(movementReference.action.ReadValue<Vector2>());
            
            Vector3 pointerPosition = pointerReference.action.ReadValue<Vector2>();
            playerLook.SetTargetPosition(PointerToWorldPosition(pointerPosition));

            if (shootReference.action.IsPressed())
            {
                gunController.Fire();
            }

            if (dodgeReference.action.WasPressedThisFrame())
            {
                playerMovement.Dodge();
            }

            if (reloadReference.action.WasPressedThisFrame())
            {
                gunController.ReloadEquippedGun();
            }

            if (quitReference.action.WasPressedThisFrame())
            {
                Application.Quit();
            }
        }
        
        private Vector3 PointerToWorldPosition(Vector3 pointerPosition)
        {
            var ray = MainCamera.ScreenPointToRay(pointerPosition);
            if (Plane.Raycast(ray, out var distance))
            {
                return ray.GetPoint(distance);
            }
            return transform.position;
        }
    }
}
