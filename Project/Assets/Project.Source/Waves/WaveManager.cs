using System.Collections.Generic;
using UnityEngine;

namespace Project.Source.Waves
{
    public class WaveManager : MonoBehaviour
    {
        public List<EnemySpawnEntry> EnemySpawnEntries = new List<EnemySpawnEntry>();

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