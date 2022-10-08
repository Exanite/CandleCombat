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
    }
}