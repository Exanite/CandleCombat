using System.Collections.Generic;
using UnityEngine;

namespace Project.Source.MachineLearning
{
    public class MlGameOutput
    {
        public float TimeAlive;
        
        public readonly MlPlayerData Player = new MlPlayerData();
        public readonly List<MlEnemyData> Enemies = new List<MlEnemyData>();
    }

    public class MlPlayerData
    {
        public float CurrentHealth;
        public float MaxHealth;

        public Vector2 Velocity;

        public float BurningShotCooldown;
        public float SoulTransferCooldown;
        public float DodgeCooldown;

        public int CurrentAmmo;
        public int MaxAmmo;
        public bool IsReloading;
    }

    public class MlEnemyData
    {
        public Vector2 OffsetFromPlayer;
        public bool CanSeeFromPlayer;
    }
}