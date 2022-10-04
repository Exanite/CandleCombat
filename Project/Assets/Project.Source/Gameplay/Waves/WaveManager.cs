using System.Collections.Generic;
using System.Linq;
using Project.Source.Gameplay.Characters;
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
        
        public List<EnemySpawner> Spawners = new List<EnemySpawner>();
        public List<EnemySpawner> ActiveSpawners = new List<EnemySpawner>();

        private float spawnCooldown;

        private void Start()
        {
            Spawners = FindObjectsOfType<EnemySpawner>().ToList();
        }

        private void Update()
        {
            spawnCooldown -= Time.deltaTime;
            
            ActiveCharacterCount = Character.ActiveCharacters.Count;
            ActiveEnemyCount = 0;
            
            foreach (var character in Character.ActiveCharacters)
            {
                if (!character.IsPlayer)
                {
                    ActiveEnemyCount++;
                }
            }
            
            if (spawnCooldown < 0 && ActiveEnemyCount < MaxEnemies)
            { 
                UpdateActiveSpawners();
                if (ActiveSpawners.Count > 0)
                {
                    ActiveSpawners[Random.Range(0, ActiveSpawners.Count)].TrySpawn();
                }

                spawnCooldown = GlobalSpawnCooldownTime;
            }
        }

        private void UpdateActiveSpawners()
        {
            ActiveSpawners.Clear();

            Character player = GameContext.Instance.CurrentPlayer;

            if (player == null) return;
            
            var playerPosition = player.transform.position;

            foreach (var spawner in Spawners)
            {
                var spawnerDistance = (spawner.transform.position - playerPosition).magnitude;

                if (spawnerDistance > MinSpawnRange && spawnerDistance < MaxSpawnRange && !spawner.IsCoolingDown)
                {
                    ActiveSpawners.Add(spawner);
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