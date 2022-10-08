using Project.Source.Input;
using UniDi;
using UnityEngine;

namespace Project.Source.Gameplay.Player
{
    public abstract class PlayerController : MonoBehaviour
    {
        private PlayerMovement playerMovement;
        private PlayerLook playerLook;
        private GunController gunController;

        [Inject]
        private GameContext gameContext;

        private PlayerInputData currentInput = new PlayerInputData();
        private PlayerInputData previousInput = new PlayerInputData();

        protected virtual void Awake()
        {
            playerMovement = GetComponent<PlayerMovement>();
            playerLook = GetComponent<PlayerLook>();
            gunController = GetComponent<GunController>();
        }

        private void Update()
        {
            var character = gameContext.CurrentPlayer;
            if (character == null)
            {
                return;
            }

            // Gather
            (currentInput, previousInput) = (previousInput, currentInput);
            GatherInput(currentInput);
            currentInput.Clean();

            // Apply
            playerMovement.SetCharacter(character);
            playerLook.SetCharacter(character);
            gunController.SetCharacter(character);

            playerMovement.SetMovementDirection(currentInput.MovementDirection);
            playerLook.SetTargetPosition(currentInput.TargetPosition);

            if (currentInput.IsShootPressed)
            {
                gunController.Fire();
            }

            if (currentInput.IsDodgePressed && !previousInput.IsDodgePressed)
            {
                playerMovement.Dodge();
            }

            if (currentInput.IsReloadPressed && !previousInput.IsReloadPressed)
            {
                gunController.ReloadEquippedGun();
            }
        }

        protected abstract void GatherInput(PlayerInputData input);
    }
}