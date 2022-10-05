using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Exanite.Drawing;
using Project.Source.Gameplay.Abilities;
using Project.Source.Gameplay.Characters;
using Project.Source.Gameplay.Player;
using Project.Source.Gameplay.Waves;
using Project.Source.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.VFX;

namespace Project.Source
{
    public class GameContext : MonoBehaviour, AbilityInputActions.IPlayerAbilitiesActions
    {
        [Header("Dependencies")]
        public Camera MainCamera;
        public GunController GunController;
        public CinemachineVirtualCamera VirtualCamera;
        public Character CurrentPlayer;
        [FormerlySerializedAs("waveManager")]
        public WaveManager WaveManager;
        public VisualEffect PlayerWickPrefab;
        public DrawingService DrawingService;
        public AudioSource AudioSource;

        [Header("Health")]
        public float CurrentHealth = 100;
        public float MaxHealth = 100;

        public float HealthRegenPerSecond = -5f;

        public bool IsDead;

        [Header("Abilities")]
        public List<Ability> Abilities = new List<Ability>();

        private AbilityInputActions abilityInputActions;
        public GunController PlayerGunController;

        //TODO: Move event??
        public event Action<Character> Possessed;

        private void Awake()
        {
            abilityInputActions = new AbilityInputActions();
            abilityInputActions.PlayerAbilities.SetCallbacks(this);
        }

        private void Start()
        {
            if (CurrentPlayer != null)
            {
                Possess(CurrentPlayer);
            }
        }

        private void OnEnable()
        {
            abilityInputActions.Enable();
        }

        private void OnDisable()
        {
            abilityInputActions.Disable();
        }

        private void Update()
        {
            if (IsDead)
            {
                return;
            }

            UpdateHealthDecay();
            CheckDeath();
            SyncHealth();

            UpdateAbilityCooldowns();
        }

        public void Possess(Character character)
        {
            if (CurrentPlayer != null)
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

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void UpdateAbilityCooldowns()
        {
            foreach (var ability in Abilities)
            {
                ability.CurrentCooldown -= Time.deltaTime;
            }
        }

        private void ExecuteAbility(int index)
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

        void AbilityInputActions.IPlayerAbilitiesActions.OnExecuteAbility0(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                ExecuteAbility(0);
            }
        }

        void AbilityInputActions.IPlayerAbilitiesActions.OnExecuteAbility1(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                ExecuteAbility(1);
            }
        }
    }
}