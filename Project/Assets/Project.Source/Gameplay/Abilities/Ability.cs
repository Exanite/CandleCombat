using UnityEngine;
using UnityEngine.Serialization;

namespace Project.Source.Gameplay.Abilities
{
    public abstract class Ability : ScriptableObject
    {
        [Header("UI")]
        public Sprite Icon;

        [FormerlySerializedAs("CooldownTime")]
        [Header("Configuration")]
        public float CooldownDuration = 1;
        public float HealthCost = 1;
        
        [Header("Runtime")]
        public float CurrentCooldown;

        public abstract void Execute();
    }
}