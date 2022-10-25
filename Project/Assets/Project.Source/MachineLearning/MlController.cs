using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Project.Source.Gameplay.Characters;
using Project.Source.Gameplay.Guns;
using Project.Source.Gameplay.Player;
using Project.Source.SceneManagement;
using Project.Source.Serialization;
using Project.Source.UserInterface;
using UniDi;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace Project.Source.MachineLearning
{
    [DefaultExecutionOrder(-60)]
    public class MlController : MonoBehaviour
    {
        public string InstanceSceneName = "MachineLearningInstance";

        public string PipeName = "CandleCombatMachineLearning";
        public int TargetInstanceCount = 10;
        public bool LogInputOutputs;
        public bool RespawnPlayers = true;

        [Inject] private SceneLoader sceneLoader;
        [Inject] private Scene scene;
        [Inject] private ProjectJsonSerializer serializer;
        [Inject] private UiContext uiContext;

        private NamedPipeServerStream server;
        private StreamReader streamReader;
        private StreamWriter streamWriter;

        private bool hasInitialized;

        private long tickCount;

        public List<MlGameContext> GameContexts { get; } = new List<MlGameContext>();

        private void Start()
        {
            TryReadCommandLineArguments();

            server = new NamedPipeServerStream(PipeName, PipeDirection.InOut);
            server.WaitForConnectionAsync(this.GetCancellationTokenOnDestroy());

            streamReader = new StreamReader(server);
            streamWriter = new StreamWriter(server);

            Debug.Log($"Starting named pipe: {PipeName}");
            Debug.Log($"Target instance count: {TargetInstanceCount}");
            Debug.Log($"Log input/outputs: {LogInputOutputs}");
            Debug.Log($"Should respawn players: {RespawnPlayers}");
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

            if (server.IsConnected && hasInitialized && GameContexts.Count > 0)
            {
                Debug.Log($"Running tick: {tickCount}");
                tickCount++;

                // Output data and wait for input
                // Gather outputs
                var outputs = new List<MlGameOutput>();
                foreach (var mlGameContext in GameContexts)
                {
                    var game = mlGameContext.GameContext;

                    var output = new MlGameOutput();
                    outputs.Add(output);

                    output.Id = game.Id;

                    var player = game.CurrentPlayer;
                    var playerPosition = player ? player.transform.position : Vector3.zero;
                    var playerVelocity = player ? player.Rigidbody.velocity : Vector3.zero;

                    // Player
                    {
                        output.Player.TimeAlive = game.TimeAlive;
                        output.Player.CurrentHealth = game.CurrentHealth;
                        output.Player.MaxHealth = game.MaxHealth;

                        output.Player.Position = new Vector2(playerPosition.x, playerPosition.z);
                        output.Player.Velocity = new Vector2(playerVelocity.x, playerVelocity.z);
                        output.Player.MovementSpeed = game.PlayerMovement.MovementSpeed;

                        output.Player.BurningShotCooldown = Mathf.Clamp(game.Abilities[0].CurrentCooldown, 0, float.PositiveInfinity);
                        output.Player.SoulTransferCooldown = Mathf.Clamp(game.Abilities[1].CurrentCooldown, 0, float.PositiveInfinity);
                        output.Player.DodgeCooldown = Mathf.Clamp(game.Abilities[2].CurrentCooldown, 0, float.PositiveInfinity);

                        output.Player.BurningShotCooldownDuration = game.Abilities[0].CooldownDuration;
                        output.Player.SoulTransferCooldownDuration = game.Abilities[1].CooldownDuration;
                        output.Player.DodgeCooldownDuration = game.Abilities[2].CooldownDuration;

                        output.Player.CurrentAmmo = game.PlayerGunController.GetCurrentAmmo();
                        output.Player.MaxAmmo = game.PlayerGunController.GetMaxAmmo();
                        // Technically not accurate, but AI probably doesn't need to know the difference between
                        // switching and reloading.
                        output.Player.IsReloading = game.PlayerGunController.GunState != GunState.Ready;
                    }

                    // Enemies
                    {
                        output.Enemies.Clear();
                        foreach (var character in game.AllCharacters)
                        {
                            if (!character.IsPlayer && !character.IsDead)
                            {
                                var enemyData = new MlEnemyData();
                                output.Enemies.Add(enemyData);

                                var offsetFromPlayer = character.transform.position - playerPosition;
                                offsetFromPlayer.y = 0;

                                enemyData.OffsetFromPlayer = new Vector2(offsetFromPlayer.x, offsetFromPlayer.z);

                                var canSeeFromPlayer = game.PhysicsScene.Raycast(
                                        playerPosition + Vector3.up,
                                        offsetFromPlayer.normalized,
                                        out var hit, offsetFromPlayer.magnitude)
                                    && hit.collider.TryGetComponent(out Character hitCharacter)
                                    && hitCharacter == character;

                                enemyData.CanSeeFromPlayer = canSeeFromPlayer;
                            }
                        }
                    }

                    // Navigation raycasts
                    {
                        var direction = Vector3.forward;
                        for (var i = 0; i < MlPlayerData.DefaultNavigationRaycastCount; i++)
                        {
                            var distance = NavMesh.Raycast(
                                playerPosition,
                                playerPosition + direction * MlPlayerData.DefaultNavigationRaycastMaxDistance,
                                out var hit, NavMesh.AllAreas)
                                ? hit.distance
                                : MlPlayerData.DefaultNavigationRaycastMaxDistance;

                            direction = Quaternion.AngleAxis(360f / MlPlayerData.DefaultNavigationRaycastCount, Vector3.up) * direction;

                            output.Player.NavigationRaycasts[i] = distance;
                        }

                        output.Player.NavigationRaycastMaxDistance = MlPlayerData.DefaultNavigationRaycastMaxDistance;
                    }

                    // Projectiles
                    {
                        output.Projectiles.Clear();
                        foreach (var orbProjectile in game.AllOrbProjectiles)
                        {
                            var projectileData = new MlProjectileData();
                            output.Projectiles.Add(projectileData);

                            var offsetFromPlayer = orbProjectile.transform.position - playerPosition;
                            projectileData.OffsetFromPlayer = new Vector2(offsetFromPlayer.x, offsetFromPlayer.z);
                            projectileData.IsOwnedByPlayer = orbProjectile.OwningCharacter.IsPlayer;
                        }
                    }
                }

                // Serialize and send outputs
                var outputJson = Serialize(outputs);

                if (LogInputOutputs)
                {
                    print(outputJson);
                }

                streamWriter.WriteLine(outputJson);
                streamWriter.Flush();

                // Read and deserialize inputs
                var inputJson = streamReader.ReadLine();

                if (LogInputOutputs)
                {
                    print(inputJson);
                }

                var inputs = Deserialize<List<MlGameInput>>(inputJson);

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
                    var mlGameContext = GameContexts[i];
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
                streamWriter.Dispose();
                streamReader.Dispose();
            }
            catch (Exception)
            {
                // Ignore
            }

            try
            {
                server.Dispose();
            }
            catch (Exception)
            {
                // Ignore
            }
        }

        public void RegisterGameContext(GameContext gameContext)
        {
            Debug.Log("Registering GameContext");

            var index = GameContexts.FindIndex(x => x.GameContext == gameContext);
            if (index == -1)
            {
                GameContexts.Add(new MlGameContext
                {
                    GameContext = gameContext,
                    Controller = gameContext.GetComponent<ExternalPlayerController>(),
                });
            }
        }

        public void UnregisterGameContext(GameContext gameContext)
        {
            Debug.Log("Unregistering GameContext");

            var index = GameContexts.FindIndex(x => x.GameContext == gameContext);
            if (index != -1)
            {
                GameContexts.RemoveAt(index);
            }
        }

        public void UnloadInstanceScene(Scene scene)
        {
            sceneLoader.UnloadScene(scene).Forget();

            if (RespawnPlayers)
            {
                LoadInstanceScene();
            }
        }

        private void LoadInstanceScenes()
        {
            for (var i = 0; i < TargetInstanceCount; i++)
            {
                LoadInstanceScene();
            }
        }

        private void LoadInstanceScene()
        {
            sceneLoader.LoadAdditiveScene(InstanceSceneName, scene, LocalPhysicsMode.Physics3D);
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

                if (arg == "--respawn-players")
                {
                    RespawnPlayers = bool.Parse(args[i + 1]);
                }

                if (arg == "--log-input-outputs")
                {
                    LogInputOutputs = bool.Parse(args[i + 1]);
                }
            }
        }
    }
}