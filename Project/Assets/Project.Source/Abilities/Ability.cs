using UnityEngine;

namespace Project.Source.Abilities
{
    public abstract class Ability : ScriptableObject
    {
        [Header("UI")]
        public Sprite Icon;

        [Header("Configuration")]
        public float CooldownTime = 1;
        public float WaxCost = 1;
        
        [Header("Runtime")]
        public float CurrentCooldown;

        public abstract void Execute();
    }
}