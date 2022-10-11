using Project.Source.Gameplay.Guns;
using Project.Source.Gameplay.Player;
using UniDi;
using UnityEngine;

namespace Project.Source.Gameplay.Abilities
{
    public class BurningShotAbility : Ability
    {
        [Header("Settings")]
        [SerializeField]
        private int fireGunIndex = 1;

        [Inject]
        private GameContext gameContext;

        public override void Execute()
        {
            var player = gameContext.CurrentPlayer;
            if (player == null)
            {
                return;
            }

            var controller = gameContext.PlayerGunController;
            var previousGunIndex = controller.EquippedGunIndex;
            
            controller.SwitchGun(fireGunIndex);
            AddBulletShotHandler(controller, previousGunIndex);
        }

        private void AddBulletShotHandler(GunController controller, int previousGunIndex)
        {
            var gun = controller.GetEquippedGun();
            var shotsLeft = gun.MaxAmmo;
            
            gun.BulletShot += OnBulletShot;

            void OnBulletShot()
            {
                shotsLeft--;

                if (shotsLeft <= 0)
                {
                    gun.BulletShot -= OnBulletShot;
                    controller.SwitchGun(previousGunIndex);
                }
            }
        }
    }
}