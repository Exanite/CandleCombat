using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Exanite.Drawing;
using Project.Source.Abilities;
using Project.Source.Characters;
using Project.Source.Input;
using Project.Source.Waves;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.VFX;

namespace Project.Source
{
    public class GameContext : SingletonBehaviour<GameContext>, AbilityInputActions.IPlayerAbilitiesActions
    {
        [Header("Dependencies")]
        public Camera MainCamera;
        public Character CurrentPlayer;
        [FormerlySerializedAs("EnemySpawnManager")]
        public WaveManager waveManager;
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
        
        //TODO: Move event??
        public event Action<Character> Possessed;

        protected override void Awake()
        {
            base.Awake();

            abilityInputActions = new AbilityInputActions();
            abilityInputActions.PlayerAbilities.SetCallbacks(this);

            Abilities = Abilities.Select(asset => Instantiate(asset)).ToList();

            //TODO: Ensure component is on main camera with separate script w/ require component.
            AudioSource = MainCamera.GetComponent<AudioSource>();
        }

        private void Start()
        {
            if(CurrentPlayer != null)
                Possess(CurrentPlayer);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

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
                CurrentHealth = Instance.MaxHealth;
            }
            
            CurrentPlayer = character;
            
            Possessed?.Invoke(character);
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
            
            Debug.Log("Character died");

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