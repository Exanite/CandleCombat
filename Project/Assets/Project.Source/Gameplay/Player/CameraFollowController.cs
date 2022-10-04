using Cinemachine;
using Project.Source.Gameplay.Characters;
using UnityEngine;

namespace Project.Source.Gameplay.Player
{
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
}
