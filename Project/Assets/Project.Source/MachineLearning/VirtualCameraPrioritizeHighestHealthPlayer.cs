using Cinemachine;
using Project.Source.UserInterface;
using UniDi;

namespace Project.Source.MachineLearning
{
    public class VirtualCameraPrioritizeHighestHealthPlayer : GameUiBehaviour
    {
        public CinemachineVirtualCamera VirtualCamera;

        private void Update()
        {
            VirtualCamera.Priority = (int)(GameContext.CurrentHealth * 100);
        }
    }
}