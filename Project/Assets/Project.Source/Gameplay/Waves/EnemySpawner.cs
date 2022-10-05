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

        public bool IsCoolingDown => spawnCooldown > 0;
        
        private WaveManager WaveManager => gameContext.WaveManager;

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

                Instantiate(entry.EnemyPrefab, transform.position, Quaternion.identity);
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