using Project.Source.Gameplay.Guns;
using Project.Source.Gameplay.Player;
using UniDi;
using UnityEngine;

namespace Project.Source.Gameplay.Abilities
{
    public class BurningShotAbility : Ability
    {
        [Header("Settings")]
        [SerializeField] private int shotsUntilWornOff = 1;
        [SerializeField] private int fireGunIndex = 1;

        //Saving this within the ability can be considered hacky.
        private GunController currentGunController;
        private Gun currentGun;
        private int currentReloads;
        private int currentShots;
        private int previousGun;

        [Inject]
        private GameContext gameContext;
        
        public override void Execute()
        {
            var player = gameContext.CurrentPlayer;
            if (player == null)
            {
                return;
            }

            // TODO: Don't reach into directly
            var gunController = gameContext.gameObject.GetComponent<GunController>();
            previousGun = gunController.EquippedGunIndex;
            gunController.SwitchGun(fireGunIndex);
            Gun gun = gunController.GetEquippedGun();
            gun.OnShoot += HandleShoot; //Dynamically subscribing is hacky.
            currentGunController = gunController;
            currentGun = gun;
        }

        private void HandleShoot()
        {
            currentShots++;
            
            if (currentShots < shotsUntilWornOff) return;

            currentGun.OnShoot -= HandleShoot;
            currentGunController.SwitchGun(previousGun);
            previousGun = 0;
            currentShots = 0;
        }
    }
}