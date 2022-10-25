using Cinemachine;
using Project.Source.UserInterface;
using UniDi;

namespace Project.Source.MachineLearning
{
    public class VirtualCameraPrioritizeSpectatedPlayer : GameUiBehaviour
    {
        public CinemachineVirtualCamera VirtualCamera;

        public int DefaultPriority = 0;
        public int SpectatedPriority = 100;

        [Inject]
        private GameContext selfGameContext;

        private void Update()
        {
            if (GameContext == null)
            {
                VirtualCamera.Priority = 0;

                return;
            }

            VirtualCamera.Priority = GameContext == selfGameContext ? SpectatedPriority : DefaultPriority;
        }
    }
}