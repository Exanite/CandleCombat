using System;
using System.Collections;
using System.Collections.Generic;
using Exanite.Core.Events;
using Project.Source.Gameplay.Guns;
using UniDi;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.VFX;
using Object = UnityEngine.Object;

namespace Project.Source.Gameplay.Characters
{
    public class Character : MonoBehaviour
    {
        public static HashSet<Character> ActiveCharacters = new HashSet<Character>();
        
        [Header("Dependencies")]
        public Rigidbody Rigidbody;
        public DissolveShader DissolveShader;
        
        [Header("Configuration")]
        [SerializeField]
        [FormerlySerializedAs("CurrentHealth")]
        private float currentHealth = 100;
        public float MaxHealth = 100;

        [Header("Configuration")]
        public float HealthRegenPerSecond;
        public GunPosition GunPosition;
        public List<Light> PointLights;
        
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
        
        private VisualEffect playerWick;

        [Inject]
        private GameContext gameContext;

        public bool IsPlayer => this == gameContext.CurrentPlayer;
        
        public float CurrentHealth
        {
            get => currentHealth;
            private set => currentHealth = value;
        }

        public event Action<Character> Dead;
        public event EventHandler<Character, float> TookDamage;
        
        private void Start()
        {
            if (IsPlayer)
            {
                OnPossessed();
            }
            else
            {
                foreach(var pointLight in PointLights)
                    pointLight.gameObject.SetActive(false);
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
                Rigidbody.velocity = Vector3.zero;
                
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
                gameContext.CurrentHealth -= damageAmount;
            }
            
            TookDamage?.Invoke(this, damageAmount);
        }

        public void OverwriteHealth(float value)
        {
            CurrentHealth = value;
        }
        
        public void OnPossessed()
        {
            int playerLayer = LayerMask.NameToLayer("Player");
            gameObject.layer = playerLayer;
            Rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            
            foreach(var pointLight in PointLights)
                pointLight.gameObject.SetActive(true);
        }

        public void SwitchLights(bool onToffF)
        {
            foreach(var pointLight in PointLights)
                pointLight.gameObject.SetActive(onToffF);
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
                    playerWick = Instantiate(gameContext.PlayerWickPrefab);
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

        private void OnDead()
        {
            StartCoroutine(OnDeadCoroutine());
        }

        private IEnumerator OnDeadCoroutine()
        {
            IsDead = true;
            Dead?.Invoke(this);

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

            yield return new WaitForSeconds(1);
            
            DissolveShader.StartDissolve();
            Destroy(gameObject, OnDeathDestroyDelay);
        }
    }
}