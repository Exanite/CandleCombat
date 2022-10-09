using UniDi;
using UnityEngine;

namespace Project.Source.SceneManagement
{
    [DefaultExecutionOrder(-50)]
    public class PhysicsSceneTicker : MonoBehaviour
    {
        [Inject]
        private PhysicsScene physicsScene;

        private void FixedUpdate()
        {
            physicsScene.Simulate(Time.fixedDeltaTime);
        }
    }
}