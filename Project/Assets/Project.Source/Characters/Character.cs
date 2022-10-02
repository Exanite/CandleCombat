using System;
using System.Collections.Generic;
using Exanite.Core.Events;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.VFX;

namespace Project.Source.Characters
{
    public class Character : MonoBehaviour
    {
        public static HashSet<Character> ActiveCharacters = new HashSet<Character>();
        
        public bool IsPlayer => this == GameContext.Instance.CurrentPlayer;

        [Header("Dependencies")]
        public Rigidbody Rigidbody;
        
        [Header("Configuration")]
        [SerializeField]
        [FormerlySerializedAs("CurrentHealth")]
        private float currentHealth = 100;
        public float MaxHealth = 100;

        [Header("Configuration")]
        public float HealthRegenPerSecond;
        public GunPosition GunPosition;
        
        [Header("Wick")]
        public Transform PlayerWickPosition;
        public string WickVelocityVfxString = "Player Velocity";
        
        [Header("Runtime")]
        public bool IsDead;
        public bool IsInvulnerable;
        
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
        public event Action<Character> Possessed;
        public event EventHandler<Character, float> TookDamage;

        private VisualEffect playerWick;

        private void Start()
        {
            if (IsPlayer)
            {
                OnPossessed(this);
            }
        }

        private void OnEnable()
        {
            ActiveCharacters.Add(this);
        }

        private void OnDisable()
        {
            ActiveCharacters.Remove(this);
        }

        private void Update()
        {
            if (IsDead)
            {
                return;
            }

            UpdateHealthDecay();
            CheckDeath();

            UpdatePlayerWick();
        }

        public void TakeDamage(float damageAmount)
        {
            CurrentHealth -= damageAmount;

            if (IsPlayer)
            {
                GameContext.Instance.CurrentHealth -= damageAmount;
            }
            
            TookDamage?.Invoke(this, damageAmount);
        }

        public void OverwriteHealth(float value)
        {
            CurrentHealth = value;
        }

        public void Possess()
        {
            OnPossessed(this);
        }

        private void UpdateHealthDecay()
        {
            CurrentHealth += Time.deltaTime * HealthRegenPerSecond;
        }
        
        private void UpdatePlayerWick()
        {
            if (IsPlayer)
            {
                if (!playerWick)
                {
                    playerWick = Instantiate(GameContext.Instance.PlayerWickPrefab);
                }

                playerWick.transform.position = PlayerWickPosition.transform.position;
                playerWick.SetVector3(WickVelocityVfxString, Rigidbody.velocity * 5);
            }
            else
            {
                Destroy(playerWick);
                playerWick = null;
            }
        }
        
        private void CheckDeath()
        {
            if (CurrentHealth <= 0)
            {
                OnDead();
            }
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

            Rigidbody.velocity = Vector3.zero;

            if (playerWick)
            {
                Destroy(playerWick);
            }
            
            Destroy(gameObject, OnDeathDestroyDelay);

            Dead?.Invoke(this);
        }

        protected virtual void OnPossessed(Character obj)
        {
            if (obj != GameContext.Instance.CurrentPlayer) return;
            Possessed?.Invoke(obj);
            
            int playerLayer = LayerMask.NameToLayer("Player");
            gameObject.layer = playerLayer;
            Rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
    }
}