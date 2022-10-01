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
    [SerializeField] private InputActionReference movementAction;
    [SerializeField] private Rigidbody rbRef;
    
    [Header("Settings")]
    [SerializeField] private int movementSpeed;
    
    private void Update()
    {
        Vector2 xyVelocity = GetDirection() * movementSpeed;
        Rigidbody rb = GetRigidBody();
        rb.velocity = new Vector3(xyVelocity.x, 0, xyVelocity.y);
    }

    private Vector2 GetDirection()
    {
        return movementAction.action.ReadValue<Vector2>();
    }

    private Rigidbody GetRigidBody()
    {
        return rbRef;
    }
}
