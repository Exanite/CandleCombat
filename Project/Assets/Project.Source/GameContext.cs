using System.Collections.Generic;
using System.Linq;
using Project.Source.Abilities;
using Project.Source.Characters;
using Project.Source.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Source
{
    public class GameContext : SingletonBehaviour<GameContext>, AbilityInputActions.IPlayerAbilitiesActions
    {
        public Camera MainCamera;
        public Character CurrentPlayer;

        public float CurrentHealth = 100;
        public float MaxHealth = 100;

        public float HealthRegenPerSecond = -5f;

        public bool IsDead;

        public List<Ability> Abilities = new List<Ability>();

        private AbilityInputActions abilityInputActions;

        protected override void Awake()
        {
            base.Awake();

            abilityInputActions = new AbilityInputActions();
            abilityInputActions.PlayerAbilities.SetCallbacks(this);

            Abilities = Abilities.Select(asset => Instantiate(asset)).ToList();
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

            UpdateAbilityCooldowns();
        }

        private void CheckDeath()
        {
            if (CurrentHealth <= 0)
            {
                OnDead();
            }
        }

        private void UpdateHealthDecay()
        {
            CurrentHealth += Time.deltaTime * HealthRegenPerSecond;
        }

        private void OnDead()
        {
            IsDead = true;
            
            Debug.Log("Character died");
        }

        private void UpdateAbilityCooldowns()
        {
            foreach (var ability in Abilities)
            {
                ability.CurrentCooldown -= Time.deltaTime;
            }
        }

        public void OnExecuteAbility(int index)
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
                OnExecuteAbility(0);
            }
        }

        void AbilityInputActions.IPlayerAbilitiesActions.OnExecuteAbility1(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                OnExecuteAbility(1);
            }
        }
    }
}