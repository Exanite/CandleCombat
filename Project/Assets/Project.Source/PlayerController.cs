using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMovement), typeof(PlayerLook))]
public class PlayerController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private InputActionReference movementReference;
    [SerializeField] private InputActionReference pointerReference;

    private PlayerMovement playerMovement;
    private PlayerLook playerLook;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerLook = GetComponent<PlayerLook>();
    }

    private void Update()
    {
        playerMovement.SetMoveDirection(movementReference.action.ReadValue<Vector2>());
        playerLook.SetPointerPosition(pointerReference.action.ReadValue<Vector2>());
    }
}
