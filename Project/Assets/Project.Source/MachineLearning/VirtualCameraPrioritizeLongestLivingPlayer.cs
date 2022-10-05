using Cinemachine;
using UniDi;
using UnityEngine;

namespace Project.Source.MachineLearning
{
    public class VirtualCameraPrioritizeLongestLivingPlayer : MonoBehaviour
    {
        public CinemachineVirtualCamera VirtualCamera;

        private float timer;

        [Inject]
        private GameContext gameContext;

        private void Update()
        {
            timer += Time.deltaTime;
            VirtualCamera.Priority = (int)timer;
        }
    }
}