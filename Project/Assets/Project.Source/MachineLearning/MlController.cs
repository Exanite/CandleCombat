using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using Project.Source.SceneManagement;
using UniDi;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Source.MachineLearning
{
    [DefaultExecutionOrder(-60)]
    public class MlController : MonoBehaviour
    {
        public string InstanceSceneName = "MachineLearningInstance";

        public int TargetInstanceCount = 10;

        public List<GameContext> GameContexts { get; } = new List<GameContext>();

        [Inject]
        private SceneLoader sceneLoader;

        [Inject]
        private Scene scene;

        private NamedPipeServerStream server;
        private StreamReader reader;
        private StreamWriter writer;

        private bool hasInitialized;

        private void Start()
        {
            server = new NamedPipeServerStream("CandleCombatMachineLearning");
            reader = new StreamReader(server);
            writer = new StreamWriter(server);
        }

        private void Update()
        {
            if (server.IsConnected && !hasInitialized)
            {
                // Initialize
                hasInitialized = true;
                LoadInstanceScenes();
            }

            if (!server.IsConnected && hasInitialized)
            {
                // Shutdown
                Application.Quit();
            }

            if (server.IsConnected && hasInitialized)
            {
                // Output data and wait for input
            }
        }

        private void OnDestroy()
        {
            try
            {
                server.Dispose();
            }
            catch (ObjectDisposedException)
            {
                // Ignore
            }

            reader.Dispose();
            writer.Dispose();
        }

        private void LoadInstanceScenes()
        {
            for (var i = 0; i < TargetInstanceCount; i++)
            {
                sceneLoader.LoadAdditiveScene(InstanceSceneName, scene, LocalPhysicsMode.Physics3D);
            }
        }
    }
}