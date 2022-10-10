using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using Newtonsoft.Json;
using Project.Source.Gameplay.Characters;
using Project.Source.Gameplay.Player;
using Project.Source.SceneManagement;
using Project.Source.Serialization;
using UniDi;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Source.MachineLearning
{
    [DefaultExecutionOrder(-60)]
    public class MlController : MonoBehaviour
    {
        public string InstanceSceneName = "MachineLearningInstance";

        public string PipeName = "CandleCombatMachineLearning";
        public int TargetInstanceCount = 10;

        private readonly List<MlGameContext> gameContexts = new List<MlGameContext>();

        [Inject]
        private SceneLoader sceneLoader;

        [Inject]
        private Scene scene;

        [Inject]
        private ProjectJsonSerializer serializer;

        private NamedPipeServerStream server;
        private StreamReader streamReader;
        private StreamWriter streamWriter;

        private bool hasInitialized;

        private long tickCount;

        private void Start()
        {
            TryReadCommandLineArguments();
            
            server = new NamedPipeServerStream(PipeName, PipeDirection.InOut);

            streamReader = new StreamReader(server);
            streamWriter = new StreamWriter(server);

            Debug.Log($"Starting named pipe: {PipeName}");
            Debug.Log($"Target instance count: {TargetInstanceCount}");
        }

        private void Update()
        {
            if (server.IsConnected && !hasInitialized)
            {
                Debug.Log("Detected connection");
                Debug.Log("Initializing scenes");

                // Initialize
                hasInitialized = true;
                LoadInstanceScenes();
            }

            if (!server.IsConnected && hasInitialized)
            {
                Debug.Log("Connection lost... exiting");

                // Shutdown
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }

            if (server.IsConnected && hasInitialized && gameContexts.Count > 0)
            {
                Debug.Log($"Running tick: {tickCount}");
                tickCount++;

                // Output data and wait for input
                // Gather outputs
                var outputs = new List<MlGameOutput>();
                foreach (var mlGameContext in gameContexts)
                {
                    var game = mlGameContext.GameContext;

                    var output = new MlGameOutput();
                    outputs.Add(output);

                    output.Id = game.Id;

                    output.Player.TimeAlive = game.TimeAlive;
                    output.Player.CurrentHealth = game.CurrentHealth;
                    output.Player.MaxHealth = game.MaxHealth;
                    output.Player.Position = game.CurrentPlayer
                        ? new Vector2(game.CurrentPlayer.transform.position.x, game.CurrentPlayer.transform.position.z)
                        : Vector2.zero;
                    output.Player.Velocity = game.CurrentPlayer
                        ? new Vector2(game.CurrentPlayer.Rigidbody.velocity.x, game.CurrentPlayer.Rigidbody.velocity.z)
                        : Vector2.zero;
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
                streamWriter.WriteLine(Serialize(outputs));
                streamWriter.Flush();

                // Read and deserialize inputs
                var json = streamReader.ReadLine();
                var inputs = Deserialize<List<MlGameInput>>(json);

                // Apply inputs
                if (inputs.Count != outputs.Count)
                {
                    throw new ArgumentException("Did not receive the same number of inputs as outputs. " +
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
            try
            {
                server.Dispose();
            }
            catch (Exception)
            {
                // Ignore
            }

            try
            {
                streamWriter.Dispose();
                streamReader.Dispose();
            }
            catch (Exception)
            {
                // Ignore
            }
        }

        public void RegisterGameContext(GameContext gameContext)
        {
            Debug.Log("Registering GameContext");

            var index = gameContexts.FindIndex(x => x.GameContext == gameContext);
            if (index == -1)
            {
                gameContexts.Add(new MlGameContext
                {
                    GameContext = gameContext,
                    Controller = gameContext.GetComponent<ExternalPlayerController>(),
                });
            }
        }

        public void UnregisterGameContext(GameContext gameContext)
        {
            Debug.Log("Unregistering GameContext");

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

        private string Serialize(object value)
        {
            using (var stringWriter = new StringWriter())
            using (var jsonWriter = new JsonTextWriter(stringWriter))
            {
                serializer.Serialize(jsonWriter, value);
                var json = stringWriter.ToString();

                return json;
            }
        }

        private T Deserialize<T>(string json)
        {
            using (var stringReader = new StringReader(json))
            using (var jsonReader = new JsonTextReader(stringReader))
            {
                return serializer.Deserialize<T>(jsonReader);
            }
        }

        private void TryReadCommandLineArguments()
        {
            Debug.Log("Attempting to read command line arguments");
            var args = Environment.GetCommandLineArgs();
            
            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                if (arg == "--instance-count")
                {
                    TargetInstanceCount = int.Parse(args[i + 1]);
                }
                
                if (arg == "--pipe-name")
                {
                    PipeName = args[i + 1];
                }
            }
        }
    }
}