using UniDi;
using UnityEngine;

namespace Project.Source.Gameplay.Abilities
{
    public class BurningShotAbility : Ability
    {
        [Header("Settings")]
        [SerializeField] private int fireGunIndex = 1;

        [Inject] private GameContext gameContext;

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
            
            var fireGun = controller.GetEquippedGun();
            fireGun.ForceReload();
            fireGun.BulletShot += OnBulletShot;

            void OnBulletShot()
            {
                if (controller.GetEquippedGun() != fireGun)
                {
                    fireGun.BulletShot -= OnBulletShot;
                }
                else if (fireGun.AmmoCount <= 0)
                {
                    fireGun.BulletShot -= OnBulletShot;
                    controller.SwitchGun(previousGunIndex);
                }
            }
        }
    }
}