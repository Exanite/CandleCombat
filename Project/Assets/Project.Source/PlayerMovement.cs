using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private InputActionReference movementAction;
    [SerializeField] private int movementSpeed;

    private void Update()
    {
        Debug.Log($"Direction: {GetDirection()}");
    }

    private Vector2 GetDirection()
    {
        return movementAction.action.ReadValue<Vector2>();
    }
}
