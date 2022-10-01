using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Project.Source;
using UnityEngine;

public class CameraFollowController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private PlayerLook playerLook;

    private void Update()
    {
        Character character = GameContext.Instance.CurrentPlayer;

        if (character == null || playerLook.LookAt == null) return;

        virtualCamera.Follow = playerLook.LookAt;
        virtualCamera.LookAt = character.transform;
    }
}
