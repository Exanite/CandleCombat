using Project.Source.Gameplay.Player;
using UnityEngine;

namespace Project.Source.Gameplay.Abilities
{
    public class SoulTransferAbility : Ability
    {
        [SerializeField] private int soulGunIndex = 2;
        
        public override void Execute()
        {
            var player = GameContext.Instance.CurrentPlayer;
            if (player == null)
            {
                return;
            }

            // TODO: Don't reach into Ability directly.
            var gunController = GameContext.Instance.gameObject.GetComponent<GunController>();
            int previousGun = gunController.EquippedGunIndex;
            gunController.SwitchGun(soulGunIndex);
            gunController.Fire();
            gunController.SwitchGun(previousGun);
        }
    }
}