using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using Project.Source.Gameplay.Characters;
using Project.Source.Gameplay.Player;
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

        private readonly List<MlGameContext> gameContexts = new List<MlGameContext>();

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
                
                // Gather outputs
                var outputs = new List<MlGameOutput>();
                foreach (var mlGameContext in gameContexts)
                {
                    var game = mlGameContext.GameContext;
                    
                    var output = new MlGameOutput();
                    outputs.Add(output);
                    
                    output.Player.TimeAlive = game.TimeAlive;
                    output.Player.CurrentHealth = game.CurrentHealth;
                    output.Player.MaxHealth = game.MaxHealth;
                    output.Player.Velocity = game.CurrentPlayer ? new Vector2(game.CurrentPlayer.Rigidbody.velocity.x, game.CurrentPlayer.Rigidbody.velocity.z) : Vector2.zero;
                    output.Player.BurningShotCooldown = Mathf.Clamp(game.Abilities[0].CurrentCooldown, 0, float.PositiveInfinity);
                    output.Player.SoulTransferCooldown = Mathf.Clamp(game.Abilities[1].CurrentCooldown, 0, float.PositiveInfinity);
                    output.Player.DodgeCooldown = Mathf.Clamp(game.Abilities[2].CurrentCooldown, 0, float.PositiveInfinity);
                    output.Player.CurrentAmmo = game.GunController.GetCurrentAmmo();
                    output.Player.MaxAmmo = game.GunController.GetMaxAmmo();
                    output.Player.IsReloading = game.GunController.IsReloading();

                    if (game.CurrentPlayer)
                    {
                        foreach (var character in game.AllCharacters)
                        {
                            if (!character.IsPlayer && !character.IsDead)
                            {
                                var enemyData = new MlEnemyData();
                                output.Enemies.Add(enemyData);

                                var offsetFromPlayer = game.CurrentPlayer.transform.position - character.transform.position;
                                enemyData.OffsetFromPlayer = new Vector2(offsetFromPlayer.x, offsetFromPlayer.z);

                                var canSeeFromPlayer = game.PhysicsScene.Raycast(
                                    game.CurrentPlayer.transform.position + Vector3.one, 
                                    offsetFromPlayer.normalized,
                                    out var hit, offsetFromPlayer.magnitude) 
                                    && hit.collider.TryGetComponent(out Character hitCharacter)
                                    && hitCharacter == character;
                                enemyData.CanSeeFromPlayer = canSeeFromPlayer;
                            }
                        }
                    }
                }

                // Serialize and send outputs
                
                // Read and deserialize inputs
                var inputs = new List<MlGameInput>();

                // Apply inputs
                if (inputs.Count != outputs.Count)
                {
                    throw new ArgumentException($"Did not receive the same number of inputs as outputs. " +
                        $"Input count: {inputs.Count}. " +
                        $"Output count: {outputs.Count}.");
                }
                
                for (var i = 0; i < inputs.Count; i++)
                {
                    var input = inputs[i];
                    var mlGameContext = gameContexts[i];
                    var playerPosition = mlGameContext.GameContext.CurrentPlayer
                        ? mlGameContext.GameContext.CurrentPlayer.transform.position
                        : Vector3.zero;

                    input.CopyTo(mlGameContext.Controller.PlayerInputData, playerPosition);
                }
            }
        }

        private void OnDestroy()
        {
            reader.Dispose();
            
            if (server.IsConnected)
            {
                writer.Dispose();
                server.Dispose();
            }
        }

        public void RegisterGameContext(GameContext gameContext)
        {
            var index = gameContexts.FindIndex(x => x.GameContext == gameContext);
            if (index == -1)
            {
                gameContexts.Add(new MlGameContext()
                {
                    GameContext = gameContext,
                    Controller = gameContext.GetComponent<ExternalPlayerController>(),
                });
            }
        }

        public void UnregisterGameContext(GameContext gameContext)
        {
            var index = gameContexts.FindIndex(x => x.GameContext == gameContext);
            if (index != -1)
            {
                gameContexts.RemoveAt(index);
            }
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