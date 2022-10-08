using Cinemachine;
using UniDi;
using UnityEngine;

namespace Project.Source.Gameplay.Player
{
    public class CameraFollow : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField]
        private CinemachineVirtualCamera virtualCamera;
        [SerializeField]
        private PlayerLook playerLook;

        [Inject]
        private GameContext gameContext;

        private void Update()
        {
            var character = gameContext.CurrentPlayer;
            if (character == null || playerLook.LookAt == null)
            {
                return;
            }

            virtualCamera.Follow = playerLook.LookAt;
            virtualCamera.LookAt = character.transform;
        }
    }
}