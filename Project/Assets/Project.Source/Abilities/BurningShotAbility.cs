using UnityEngine;

namespace Project.Source.Abilities
{
    [CreateAssetMenu(menuName = "Project/Abilities/BurningShot")]
    public class BurningShotAbility : Ability
    {
        public override void Execute()
        {
            Debug.Log("BurningShotAbility");
        }
    }
}