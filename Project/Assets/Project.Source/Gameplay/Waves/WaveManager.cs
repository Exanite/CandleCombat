using System.Collections.Generic;
using System.Linq;
using UniDi;
using UnityEngine;

namespace Project.Source.Gameplay.Waves
{
    public class WaveManager : MonoBehaviour
    {
        [Header("Entries")]
        public List<EnemySpawnEntry> EnemySpawnEntries = new List<EnemySpawnEntry>();

        [Header("Configuration")]
        public int MaxEnemies = 64;
        public float MinSpawnRange = 20f;
        public float MaxSpawnRange = 50f;
        public float GlobalSpawnCooldownTime = 0.1f;

        [Header("Runtime")]
        public int ActiveCharacterCount;
        public int ActiveEnemyCount;

        public HashSet<EnemySpawner> AllSpawners { get; set; } = new HashSet<EnemySpawner>();
        public List<EnemySpawner> SpawnersInRange { get; set; } = new List<EnemySpawner>();

        private float spawnCooldown;

        [Inject]
        private GameContext gameContext;

        private void Update()
        {
            spawnCooldown -= Time.deltaTime;

            ActiveCharacterCount = gameContext.AllCharacters.Count;
            ActiveEnemyCount = 0;

            foreach (var character in gameContext.AllCharacters)
            {
                if (!character.IsPlayer)
                {
                    ActiveEnemyCount++;
                }
            }

            if (spawnCooldown < 0 && ActiveEnemyCount < MaxEnemies)
            {
                UpdateActiveSpawners();
                if (SpawnersInRange.Count > 0)
                {
                    SpawnersInRange[Random.Range(0, SpawnersInRange.Count)].TrySpawn();
                }

                spawnCooldown = GlobalSpawnCooldownTime;
            }
        }

        private void UpdateActiveSpawners()
        {
            SpawnersInRange.Clear();

            var player = gameContext.CurrentPlayer;
            if (player == null)
            {
                return;
            }

            var playerPosition = player.transform.position;
            foreach (var spawner in AllSpawners)
            {
                var spawnerDistance = (spawner.transform.position - playerPosition).magnitude;

                if (spawnerDistance > MinSpawnRange && spawnerDistance < MaxSpawnRange && !spawner.IsCoolingDown)
                {
                    SpawnersInRange.Add(spawner);
                }
            }
        }

        public EnemySpawnEntry GetRandomEntry()
        {
            var totalWeight = 0f;
            foreach (var entry in EnemySpawnEntries)
            {
                totalWeight += entry.Weight;
            }

            var rng = Random.Range(0, totalWeight);
            var current = 0f;

            for (var i = 0; i < EnemySpawnEntries.Count; i++)
            {
                var currentEntry = EnemySpawnEntries[i];
                var min = current;
                var max = current + currentEntry.Weight;

                if (rng > min && rng < max)
                {
                    return currentEntry;
                }

                current += currentEntry.Weight;
            }

            return null;
        }
    }
}