using System;
using System.Collections;
using System.Collections.Generic;
using Project.Source;
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
        
        playerLook.SetCamera(GameContext.Instance.MainCamera);
    }

    private void Update()
    {
        Character character = GameContext.Instance.CurrentPlayer;

        if (character == null) return;
        
        playerMovement.SetCharacter(character);
        playerLook.SetCharacter(character);

        playerMovement.SetMoveDirection(movementReference.action.ReadValue<Vector2>());
        playerLook.SetPointerPosition(pointerReference.action.ReadValue<Vector2>());
    }
}
