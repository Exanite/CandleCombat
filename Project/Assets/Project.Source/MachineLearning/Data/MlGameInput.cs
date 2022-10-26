using Project.Source.Input;
using UnityEngine;

namespace Project.Source.MachineLearning.Data
{
    public class MlGameInput
    {
        public Vector2 MovementDirection;
        public Vector2 TargetDirection;
        
        public bool IsBurningShotPressed;
        public bool IsSoulTransferPressed;
        public bool IsDodgePressed;
        public bool IsShootPressed;
        public bool IsReloadPressed;
        
        public void CopyTo(PlayerInputData input, Vector3 playerPosition)
        {
            input.MovementDirection = MovementDirection;
            input.TargetPosition = playerPosition + new Vector3(TargetDirection.x, 0, TargetDirection.y);
            input.IsBurningShotPressed = IsBurningShotPressed;
            input.IsSoulTransferPressed = IsSoulTransferPressed;
            input.IsDodgePressed = IsDodgePressed;
            input.IsShootPressed = IsShootPressed;
            input.IsReloadPressed = IsReloadPressed;
        }
    }
}