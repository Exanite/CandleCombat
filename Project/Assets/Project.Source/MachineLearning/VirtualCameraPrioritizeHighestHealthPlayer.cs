using Cinemachine;
using UniDi;
using UnityEngine;

namespace Project.Source.MachineLearning
{
    public class VirtualCameraPrioritizeHighestHealthPlayer : MonoBehaviour
    {
        public CinemachineVirtualCamera VirtualCamera;

        [Inject]
        private GameContext gameContext;

        private void Update()
        {
            VirtualCamera.Priority = (int)(gameContext.CurrentHealth * 100);
        }
    }
}