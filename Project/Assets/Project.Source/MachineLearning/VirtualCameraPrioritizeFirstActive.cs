using Cinemachine;
using UnityEngine;

namespace Project.Source.MachineLearning
{
    public class VirtualCameraPrioritizeFirstActive : MonoBehaviour
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