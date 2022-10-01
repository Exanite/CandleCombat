using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.InputSystem;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class PlayerMovement : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Rigidbody rbRef;
    
    [Header("Settings")]
    [SerializeField] private int movementSpeed;

    private Vector2 moveDirection;
    
    private void Update()
    {
        Vector2 xyVelocity = moveDirection * movementSpeed;
        Rigidbody rb = GetRigidBody();
        rb.velocity = new Vector3(xyVelocity.x, 0, xyVelocity.y);
    }

    public void SetMoveDirection(Vector2 moveDir)
    {
        moveDirection = moveDir;
    }

    private Rigidbody GetRigidBody()
    {
        return rbRef;
    }
}
