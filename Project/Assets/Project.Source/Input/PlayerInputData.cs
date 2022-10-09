using UnityEngine;

namespace Project.Source.Input
{
    public class PlayerInputData
    {
        public Vector2 MovementDirection;
        public Vector3 TargetPosition;

        public bool IsBurningShotPressed;
        public bool IsSoulTransferPressed;
        public bool IsDodgePressed;

        public bool IsShootPressed;
        public bool IsReloadPressed;

        public void Clean()
        {
            MovementDirection = Vector2.ClampMagnitude(MovementDirection, 1);
        }

        public void CopyTo(PlayerInputData input)
        {
            input.MovementDirection = MovementDirection;
            input.TargetPosition = TargetPosition;
            input.IsBurningShotPressed = IsBurningShotPressed;
            input.IsSoulTransferPressed = IsSoulTransferPressed;
            input.IsDodgePressed = IsDodgePressed;
            input.IsShootPressed = IsShootPressed;
            input.IsReloadPressed = IsReloadPressed;
        }
    }
}