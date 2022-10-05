using UniDi;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Source.MachineLearning
{
    public class MachineLearningSceneManager : MonoBehaviour
    {
        public string InstanceSceneName = "MachineLearningInstance";

        public int TargetInstanceCount = 10;

        [Inject]
        private SceneLoader sceneLoader;

        [Inject]
        private Scene scene;

        private void Start()
        {
            for (var i = 0; i < TargetInstanceCount; i++)
            {
                sceneLoader.LoadAdditiveScene(InstanceSceneName, scene);
            }
        }
    }
}