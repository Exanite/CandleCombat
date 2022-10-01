using System.Collections.Generic;
using System.Linq;
using Project.Source.Abilities;
using Project.Source.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Source
{
    public class GameContext : SingletonBehaviour<GameContext>, AbilityInputActions.IPlayerAbilitiesActions
    {
        // public Character ControlledCharacter;

        public float CurrentHealth = 100;
        public float MaxHealth = 100;

        public float HealthDecayPerSecond = 5f;

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
            UpdateHealthDecay();
            CheckDeath();

            UpdateAbilityCooldowns();
        }

        private void CheckDeath()
        {
            if (CurrentHealth < 0)
            {
                OnDeath();
            }
        }

        private void UpdateHealthDecay()
        {
            CurrentHealth -= Time.deltaTime * HealthDecayPerSecond;
        }

        private void OnDeath()
        {
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
            if (index < 0 || index >= Abilities.Count)
            {
                return;
            }

            var ability = Abilities[index];
            if (ability == null)
            {
                return;
            }

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