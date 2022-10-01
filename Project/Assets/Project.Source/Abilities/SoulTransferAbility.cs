using UnityEngine;

namespace Project.Source.Abilities
{
    [CreateAssetMenu(menuName = "Project/Abilities/SoulTransfer")]
    public class SoulTransferAbility : Ability
    {
        public override void Execute()
        {
            Debug.Log("SoulTransferAbility");
        }
    }
}