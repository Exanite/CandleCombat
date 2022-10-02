using System;
using Project.Source.Characters;

namespace Project.Source.Waves
{
    [Serializable]
    public class EnemySpawnEntry
    {
        public Character EnemyPrefab;
        public float Weight;
    }
}