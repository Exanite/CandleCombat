using System;
using System.Collections;
using System.Collections.Generic;
using Project.Source;
using Project.Source.Characters;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMovement), typeof(PlayerLook))]
public class PlayerController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private InputActionReference shootReference;
    [SerializeField] private InputActionReference movementReference;
    [SerializeField] private InputActionReference pointerReference;

    private PlayerMovement playerMovement;
    private PlayerLook playerLook;
    private PlayerGunController playerGunController;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerLook = GetComponent<PlayerLook>();
        playerGunController = GetComponent<PlayerGunController>();
        
        playerLook.SetCamera(GameContext.Instance.MainCamera);
    }

    private void Update()
    {
        Character character = GameContext.Instance.CurrentPlayer;

        if (character == null) return;
        
        playerMovement.SetCharacter(character);
        playerLook.SetCharacter(character);
        playerGunController.SetCharacter(character);

        playerMovement.SetMoveDirection(movementReference.action.ReadValue<Vector2>());
        Vector3 pointerPosition = pointerReference.action.ReadValue<Vector2>();
        playerLook.SetPointerPosition(pointerPosition);

        if (shootReference.action.IsPressed())
        {
            playerGunController.Fire();
        }
    }
}
