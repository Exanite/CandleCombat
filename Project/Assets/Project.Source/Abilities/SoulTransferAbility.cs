using UnityEngine;

namespace Project.Source.Abilities
{
    [CreateAssetMenu(menuName = "Project/Abilities/SoulTransfer")]
    public class SoulTransferAbility : Ability
    {
        public override void Execute()
        {
            Debug.Log("SoulTransferAbility");
            
            //TODO: Don't reach into directly.
            var gunController = GameContext.Instance.gameObject.GetComponent<PlayerGunController>();
            gunController.EquippedGunIndex = 1;
        }
    }
}