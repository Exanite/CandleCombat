using UnityEngine;

namespace Project.Source.Abilities
{
    [CreateAssetMenu(menuName = "Project/Abilities/BurningShot")]
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
        
        public override void Execute()
        {
            var player = GameContext.Instance.CurrentPlayer;
            if (player == null)
            {
                return;
            }

            // TODO: Don't reach into directly
            var gunController = GameContext.Instance.gameObject.GetComponent<GunController>();
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