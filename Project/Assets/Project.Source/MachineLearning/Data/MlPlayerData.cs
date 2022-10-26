using UnityEngine;

namespace Project.Source.MachineLearning.Data
{
    public class MlPlayerData
    {
        public const int DefaultNavigationRaycastCount = 8;
        public const int DefaultNavigationRaycastMaxDistance = 8;
        
        public float TimeAlive;
        
        public float CurrentHealth;
        public float MaxHealth;

        public Vector2 Position;
        public Vector2 Velocity;
        public float MovementSpeed;

        public float BurningShotCooldown;
        public float SoulTransferCooldown;
        public float DodgeCooldown;
        
        public float BurningShotCooldownDuration;
        public float SoulTransferCooldownDuration;
        public float DodgeCooldownDuration;

        public int CurrentAmmo;
        public int MaxAmmo;
        public bool IsReloading;

        public readonly float[] NavigationRaycasts = new float[DefaultNavigationRaycastCount];
        public float NavigationRaycastMaxDistance = DefaultNavigationRaycastMaxDistance;
    }
}