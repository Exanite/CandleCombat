using System;
using Project.Source.Gameplay.Characters;
using UniDi;
using UnityEngine;

namespace Project.Source.Gameplay.Waves
{
    public class EnemySpawner : MonoBehaviour
    {
        public float SpawnCooldownTime = 5f;

        private float spawnCooldown;

        [Inject]
        private GameContext gameContext;

        [Inject]
        private IInstantiator instantiator;

        public bool IsCoolingDown => spawnCooldown > 0;
        
        private WaveManager WaveManager => gameContext.WaveManager;

        private void OnEnable()
        {
            gameContext.WaveManager.AllSpawners.Add(this);
        }

        private void OnDisable()
        {
            gameContext.WaveManager.AllSpawners.Remove(this);
        }

        private void Update()
        {
            spawnCooldown -= Time.deltaTime;
        }

        public bool TrySpawn()
        {
            if (!IsCoolingDown)
            {
                var entry = WaveManager.GetRandomEntry();
                if (entry == null)
                {
                    Debug.LogWarning("Failed to get a random enemy");

                    return false;
                }

                instantiator.InstantiatePrefabForComponent<Character>(entry.EnemyPrefab, transform.position, Quaternion.identity, null);
                
                spawnCooldown = SpawnCooldownTime;

                return true;
            }

            return false;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, 0.5f);
        }
    }
}