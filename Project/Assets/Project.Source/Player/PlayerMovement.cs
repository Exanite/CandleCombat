using Project.Source.Characters;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float movementSpeed;
    [SerializeField] private float movementSmoothTime = 0.05f;

    private Character character;
    private Vector2 moveDirection;

    // Not a typo
    private Vector3 smoothedVelocityVelocity;

    private void FixedUpdate()
    {
        var rb = character?.Rigidbody;
        if (rb == null)
        {
            return;
        }

        var targetVelocity = new Vector3(moveDirection.x, 0, moveDirection.y);
        targetVelocity *= movementSpeed;
        
        rb.velocity = Vector3.SmoothDamp(rb.velocity, targetVelocity, ref smoothedVelocityVelocity, movementSmoothTime);
    }

    public void SetCharacter(Character character)
    {
        this.character = character;
    }

    public void SetMoveDirection(Vector2 moveDir)
    {
        moveDirection = moveDir;
    }
}
