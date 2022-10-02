using Project.Source.Characters;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float movementSpeed;
    [SerializeField] private float movementSmoothTime = 0.05f;
    // [SerializeField] private float moveTiltDegrees = 30f;
    [SerializeField] private float dodgeSpeed = 50f;
    [SerializeField] private float dodgeTime = 0.2f;
    [SerializeField] private float timeBetweenDodge = 2f;
    
    private Character character;
    private Vector2 moveDirection;

    private bool isDodging;
    private float elapsedDodgeTime;
    private float elapsedTimeSinceDodge;
    private float startingMoveSpeed;

    // Not a typo
    private Vector3 smoothedVelocityVelocity;

    private void FixedUpdate()
    {
        if (isDodging)
        {
            if (elapsedDodgeTime > dodgeTime)
                StopDodging();
            else
                elapsedDodgeTime += Time.fixedDeltaTime;
        }
        else
            elapsedTimeSinceDodge += Time.fixedDeltaTime;

        if (!character || character.IsDead || !character.Rigidbody)
        {
            return;
        }

        var rb = character.Rigidbody;

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

    public void Dodge()
    {
        if (elapsedTimeSinceDodge < timeBetweenDodge) return;
        
        startingMoveSpeed = movementSpeed;
        movementSpeed = dodgeSpeed;
        isDodging = true;
        character.IsInvulnerable = true;
        elapsedDodgeTime = 0;
        elapsedTimeSinceDodge = 0;
        
    }

    private void StopDodging()
    {
        isDodging = false;
        character.IsInvulnerable = false;
        movementSpeed = startingMoveSpeed;
    }
}
