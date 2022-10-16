using Cinemachine;
using UniDi;
using UnityEngine;

namespace Project.Source.MachineLearning
{
    public class VirtualCameraPrioritizeLongestLivingPlayer : MonoBehaviour
    {
        public CinemachineVirtualCamera VirtualCamera;

        [Inject]
        private GameContext gameContext;

        private void Update()
        {
            VirtualCamera.Priority = (int)(gameContext.TimeAlive * 100);
        }
    }
}