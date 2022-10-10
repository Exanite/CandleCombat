using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Cysharp.Threading.Tasks;
using Exanite.Drawing;
using Project.Source.Gameplay.Abilities;
using Project.Source.Gameplay.Characters;
using Project.Source.Gameplay.Player;
using Project.Source.Gameplay.Waves;
using Project.Source.MachineLearning;
using Project.Source.SceneManagement;
using UniDi;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.VFX;

namespace Project.Source
{
    public class GameContext : MonoBehaviour
    {
        [Header("Dependencies")]
        public GunController GunController;
        public CinemachineVirtualCamera VirtualCamera;
        public Character CurrentPlayer;
        [FormerlySerializedAs("waveManager")]
        public WaveManager WaveManager;
        public VisualEffect PlayerWickPrefab;
        public DrawingService DrawingService;

        [Header("Health")]
        public float CurrentHealth = 100;
        public float MaxHealth = 100;

        public float HealthRegenPerSecond = -5f;

        [Header("Runtime")]
        public bool IsDead;
        public float TimeAlive;

        [Header("Abilities")]
        public List<Ability> Abilities = new List<Ability>();

        public GunController PlayerGunController;

        public HashSet<Character> AllCharacters = new HashSet<Character>();

        public event Action<Character> Possessed;

        [InjectOptional(Id = SceneLoader.ParentSceneId)]
        private Scene parentScene;

        [Inject]
        private Scene selfScene;

        [Inject]
        private SceneLoader sceneLoader;

        [InjectOptional]
        private MlController mlController;

        [Inject]
        private PhysicsScene physicsScene;

        public PhysicsScene PhysicsScene => physicsScene;

        private void Start()
        {
            if (CurrentPlayer != null)
            {
                Possess(CurrentPlayer);
            }
            
            if (mlController != null)
            {
                mlController.RegisterGameContext(this);
            }
        }

        private void OnDestroy()
        {
            if (mlController != null)
            {
                mlController.UnregisterGameContext(this);
            }
        }

        private void Update()
        {
            if (IsDead)
            {
                return;
            }

            TimeAlive += Time.deltaTime;

            UpdateHealthDecay();
            CheckDeath();
            SyncHealth();

            UpdateAbilityCooldowns();
        }

        public void Possess(Character character)
        {
            if (CurrentPlayer != null && CurrentPlayer != character)
            {
                CurrentPlayer.OverwriteHealth(-1);
                CurrentHealth = MaxHealth;
            }

            CurrentPlayer = character;
            CurrentPlayer.OnPossessed();

            Possessed?.Invoke(character);
        }

        public void ScreenShake()
        {
            StartCoroutine(ShakeScreen(0.1f, 1));
        }

        private IEnumerator ShakeScreen(float duration, float intensity)
        {
            var timer = 0f;
            var channels = VirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            channels.m_AmplitudeGain = intensity;

            while (timer < duration)
            {
                timer += Time.deltaTime;

                yield return null;
            }

            channels.m_AmplitudeGain = 0f;
        }

        private void CheckDeath()
        {
            if (CurrentHealth <= 0 || CurrentPlayer == null)
            {
                OnDead();
            }
        }

        private void SyncHealth()
        {
            if (!CurrentPlayer)
            {
                return;
            }

            CurrentPlayer.OverwriteHealth(CurrentHealth);
            CurrentPlayer.MaxHealth = MaxHealth;
        }

        private void UpdateHealthDecay()
        {
            CurrentHealth += Time.deltaTime * HealthRegenPerSecond;
        }

        private void OnDead()
        {
            IsDead = true;

            StartCoroutine(Restart(5));
        }

        private IEnumerator Restart(float delay)
        {
            yield return new WaitForSeconds(delay);

            if (parentScene.IsValid())
            {
                sceneLoader.UnloadScene(selfScene).Forget();
                sceneLoader.LoadAdditiveScene(gameObject.scene.name, parentScene, LocalPhysicsMode.Physics3D).Forget();
            }
            else
            {
                sceneLoader.LoadSingleScene(gameObject.scene.name, LocalPhysicsMode.None).Forget();
            }
        }

        private void UpdateAbilityCooldowns()
        {
            foreach (var ability in Abilities)
            {
                ability.CurrentCooldown -= Time.deltaTime;
            }
        }

        public void ExecuteAbility(int index)
        {
            if (IsDead)
            {
                return;
            }

            if (index < 0 || index >= Abilities.Count)
            {
                return;
            }

            var ability = Abilities[index];
            if (ability == null)
            {
                return;
            }

            if (ability.CurrentCooldown > 0)
            {
                return;
            }

            ability.CurrentCooldown = ability.CooldownDuration;
            CurrentHealth -= ability.HealthCost;

            ability.Execute();
        }
    }
}