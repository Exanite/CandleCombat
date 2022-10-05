using Cinemachine;
using UnityEngine;

namespace Project.Source.MachineLearning
{
    public class PrioritizeFirstActiveVirtualCamera : MonoBehaviour
    {
        public static int CurrentGlobalPriority;

        public CinemachineVirtualCamera VirtualCamera;

        private void Start()
        {
            VirtualCamera.Priority = CurrentGlobalPriority;
            CurrentGlobalPriority--;
        }
    }
}