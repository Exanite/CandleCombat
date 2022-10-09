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
                var outputs = new List<MlGameOutput>();
                foreach (var context in GameContexts)
                {
                    var output = new MlGameOutput();
                    outputs.Add(output);
                    
                    output.Player.TimeAlive = context.TimeAlive;
                    output.Player.CurrentHealth = context.CurrentHealth;
                    output.Player.MaxHealth = context.MaxHealth;
                    output.Player.Velocity = context.CurrentPlayer ? new Vector2(context.CurrentPlayer.Rigidbody.velocity.x, context.CurrentPlayer.Rigidbody.velocity.z) : Vector2.zero;
                    output.Player.BurningShotCooldown = Mathf.Clamp(context.Abilities[0].CurrentCooldown, 0, float.PositiveInfinity);
                    output.Player.SoulTransferCooldown = Mathf.Clamp(context.Abilities[1].CurrentCooldown, 0, float.PositiveInfinity);
                    output.Player.DodgeCooldown = Mathf.Clamp(context.Abilities[2].CurrentCooldown, 0, float.PositiveInfinity);
                    output.Player.CurrentAmmo = context.GunController.GetCurrentAmmo();
                    output.Player.MaxAmmo = context.GunController.GetMaxAmmo();
                    output.Player.IsReloading = context.GunController.IsReloading();

                    if (context.CurrentPlayer)
                    {
                        foreach (var character in context.AllCharacters)
                        {
                            if (!character.IsPlayer && !character.IsDead)
                            {
                                var enemyData = new MlEnemyData();
                                output.Enemies.Add(enemyData);

                                var offsetFromPlayer = context.CurrentPlayer.transform.position - character.transform.position;
                                enemyData.OffsetFromPlayer = new Vector2(offsetFromPlayer.x, offsetFromPlayer.z);
                            }
                        }
                    }
                }
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