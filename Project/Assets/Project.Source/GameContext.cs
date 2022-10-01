using System.Collections.Generic;
using System.Linq;
using Project.Source.Abilities;
using UnityEngine;

namespace Project.Source
{
    public class GameContext : SingletonBehaviour<GameContext>
    {
        // public Character ControlledCharacter;

        public float CurrentHealth = 100;
        public float MaxHealth = 100;

        public float HealthDecayPerSecond = 5f;

        public List<Ability> Abilities = new List<Ability>();

        protected override void Awake()
        {
            Abilities = Abilities.Select(asset => Instantiate(asset)).ToList();
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
    }
}