using System.Collections.Generic;

namespace Project.Source.MachineLearning.Data
{
    public class MlGameOutput
    {
        public string Id;
        
        public readonly MlPlayerData Player = new MlPlayerData();
        public readonly List<MlEnemyData> Enemies = new List<MlEnemyData>();
        public readonly List<MlProjectileData> Projectiles = new List<MlProjectileData>();
    }
}