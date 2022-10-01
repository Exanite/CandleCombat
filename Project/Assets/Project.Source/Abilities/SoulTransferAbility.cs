using System.Collections;
using Project.Source.Characters;
using UnityEngine;

namespace Project.Source.Abilities
{
    [CreateAssetMenu(menuName = "Project/Abilities/SoulTransfer")]
    public class SoulTransferAbility : Ability
    {
        public override void Execute()
        {
            Debug.Log("SoulTransferAbility");

            Character player = GameContext.Instance.CurrentPlayer;

            if (player == null) return;
            
            //TODO: Don't reach into directly.
            var gunController = GameContext.Instance.gameObject.GetComponent<GunController>();
            gunController.SwitchGun(1);
            gunController.Fire();
            gunController.SwitchGun(0);
        }
    }
}