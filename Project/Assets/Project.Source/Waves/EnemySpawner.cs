using UnityEngine;

namespace Project.Source.Waves
{
    public class EnemySpawner : MonoBehaviour
    {
        public float SpawnCooldownTime = 5f;

        private WaveManager WaveManager => GameContext.Instance.waveManager;
        private float spawnCooldown;

        private void Update()
        {
            spawnCooldown -= Time.deltaTime;
            if (spawnCooldown < 0)
            {
                var entry = WaveManager.GetRandomEntry();
                if (entry == null)
                {
                    Debug.LogWarning("Failed to get a random enemy");

                    return;
                }
                
                Instantiate(entry.EnemyPrefab, transform.position, Quaternion.identity);
                spawnCooldown = SpawnCooldownTime;
            }
        }
    }
}