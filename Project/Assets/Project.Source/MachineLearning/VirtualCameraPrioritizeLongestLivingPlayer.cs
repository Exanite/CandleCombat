using Cinemachine;
using Project.Source.UserInterface;

namespace Project.Source.MachineLearning
{
    public class VirtualCameraPrioritizeLongestLivingPlayer : GameUiBehaviour
    {
        public CinemachineVirtualCamera VirtualCamera;

        private void Update()
        {
            VirtualCamera.Priority = (int)(GameContext.TimeAlive * 100);
        }
    }
}