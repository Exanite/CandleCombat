using System.Collections.Generic;
using Project.Source.Gameplay.Player;
using Project.Source.Input;
using UnityEngine;

namespace Project.Source.MachineLearning
{
    public class MlGameContext
    {
        public GameContext GameContext;
        public ExternalPlayerController Controller;
    }
    
    public class MlGameOutput
    {
        public string Id;
        
        public readonly MlPlayerData Player = new MlPlayerData();
        public readonly List<MlEnemyData> Enemies = new List<MlEnemyData>();
        
        // Add projectiles
    }

    public class MlPlayerData
    {
        public const int NavigationRaycastCount = 8;
        public const int DefaultNavigationRaycastMaxDistance = 8;
        
        public float TimeAlive;
        
        public float CurrentHealth;
        public float MaxHealth;

        public Vector2 Position;
        public Vector2 Velocity;

        public float BurningShotCooldown;
        public float SoulTransferCooldown;
        public float DodgeCooldown;

        public int CurrentAmmo;
        public int MaxAmmo;
        public bool IsReloading;

        public readonly float[] NavigationRaycasts = new float[NavigationRaycastCount];
        public float NavigationRaycastMaxDistance = DefaultNavigationRaycastMaxDistance;
    }

    public class MlEnemyData
    {
        public Vector2 OffsetFromPlayer;
        public bool CanSeeFromPlayer;
    }

    public class MlGameInput
    {
        public Vector2 MovementDirection;
        public Vector2 TargetDirection;
        
        public bool IsBurningShotPressed;
        public bool IsSoulTransferPressed;
        public bool IsDodgePressed;
        public bool IsShootPressed;
        public bool IsReloadPressed;
        
        public void CopyTo(PlayerInputData input, Vector3 playerPosition)
        {
            input.MovementDirection = MovementDirection;
            input.TargetPosition = playerPosition + new Vector3(TargetDirection.x, 0, TargetDirection.y);
            input.IsBurningShotPressed = IsBurningShotPressed;
            input.IsSoulTransferPressed = IsSoulTransferPressed;
            input.IsDodgePressed = IsDodgePressed;
            input.IsShootPressed = IsShootPressed;
            input.IsReloadPressed = IsReloadPressed;
        }
    }
}