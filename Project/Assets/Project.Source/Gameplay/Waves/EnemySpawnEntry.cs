using System;
using Project.Source.Gameplay.Characters;

namespace Project.Source.Gameplay.Waves
{
    [Serializable]
    public class EnemySpawnEntry
    {
        public Character EnemyPrefab;
        public float Weight;
    }
}