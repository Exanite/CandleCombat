using UniDi;
using UnityEngine;

namespace Project.Source.Gameplay.Abilities
{
    public class SoulTransferAbility : Ability
    {
        [SerializeField]
        private int soulGunIndex = 2;

        [Inject]
        private GameContext gameContext;

        public override void Execute()
        {
            var player = gameContext.CurrentPlayer;
            if (player == null)
            {
                return;
            }

            var gunController = gameContext.PlayerGunController;
            var previousGun = gunController.EquippedGunIndex;
            gunController.SwitchGun(soulGunIndex);
            gunController.Fire();
            gunController.SwitchGun(previousGun);
        }
    }
}