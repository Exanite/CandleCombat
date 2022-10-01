using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Project.Source;
using Project.Source.Characters;
using UnityEngine;
using UnityEngine.InputSystem;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class PlayerMovement : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int movementSpeed;

    private Character character;
    private Vector2 moveDirection;
    
    private void FixedUpdate()
    {
        if (character == null) return;

        Vector2 xyVelocity = moveDirection * movementSpeed;
        Rigidbody rb = character.Rigidbody;
        rb.velocity = new Vector3(xyVelocity.x, 0, xyVelocity.y);
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
