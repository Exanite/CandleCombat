using System;
using UnityEngine;

namespace Project.Source.Characters
{
    public class Character : MonoBehaviour
    {
        public bool IsPlayer => this == GameContext.Instance.CurrentPlayer;
        public float CurrentHealth = 100;
        public float MaxHealth = 100;

        public bool IsDead;

        public float HealthRegenPerSecond;

        public Transform GunPosition;
        public Rigidbody Rigidbody;

        public event Action<Character> Dead;

        private void Update()
        {
            if (IsDead)
            {
                return;
            }

            UpdateHealthDecay();
            CheckDeath();
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

        protected virtual void OnDead()
        {
            IsDead = true;

            Debug.Log($"{name} died");
            
            Dead?.Invoke(this);
        }
    }
}