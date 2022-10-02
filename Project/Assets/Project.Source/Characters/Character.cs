using System;
using System.Collections.Generic;
using Exanite.Core.Events;
using UnityEngine;
using UnityEngine.Serialization;

namespace Project.Source.Characters
{
    public class Character : MonoBehaviour
    {
        public bool IsPlayer => this == GameContext.Instance.CurrentPlayer;

        [SerializeField]
        [FormerlySerializedAs("CurrentHealth")]
        private float currentHealth = 100;
        public float MaxHealth = 100;

        public bool IsDead;
        public bool IsDodging;
        public bool IsInvulnerable;

        public float HealthRegenPerSecond;

        public Transform GunPosition;
        public Rigidbody Rigidbody;
        
        [Header("On Death")]
        public List<Behaviour> DisableOnDeathBehaviours = new List<Behaviour>();
        public List<Collider> DisableOnDeathColliders = new List<Collider>();
        public float OnDeathDestroyDelay = 3f;

        public float CurrentHealth
        {
            get => currentHealth;
            private set => currentHealth = value;
        }

        public event Action<Character> Dead;
        public event EventHandler<Character, float> TookDamage;

        private void Update()
        {
            if (IsDead)
            {
                return;
            }

            UpdateHealthDecay();
            CheckDeath();
        }

        public void TakeDamage(float damageAmount)
        {
            CurrentHealth -= damageAmount;
            
            TookDamage?.Invoke(this, damageAmount);
        }

        public void OverwriteHealth(float value)
        {
            CurrentHealth = value;
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
            
            foreach (var behaviour in DisableOnDeathBehaviours)
            {
                behaviour.enabled = false;
            }
            
            foreach (var collider in DisableOnDeathColliders)
            {
                collider.enabled = false;
            }

            Destroy(gameObject, OnDeathDestroyDelay);

            Dead?.Invoke(this);
        }
    }
}