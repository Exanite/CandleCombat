using Project.Source.Gameplay.Player;
using UniDi;
using UnityEngine;

namespace Project.Source.Gameplay.Abilities
{
    public class SoulTransferAbility : Ability
    {
        [SerializeField] private int soulGunIndex = 2;

        [Inject]
        private GameContext gameContext;
        
        public override void Execute()
        {
            var player = gameContext.CurrentPlayer;
            if (player == null)
            {
                return;
            }

            // TODO: Don't reach into Ability directly.
            var gunController = gameContext.PlayerGunController;
            int previousGun = gunController.EquippedGunIndex;
            gunController.SwitchGun(soulGunIndex);
            gunController.Fire();
            gunController.SwitchGun(previousGun);
        }
    }
}