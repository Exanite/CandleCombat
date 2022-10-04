using Project.Source.Gameplay.Abilities;
using UnityEngine;

namespace Project.Source.Gameplay.Guns.Projectile
{
    public enum FireManipulatorType
    {
        SetFire,
        RemoveFire,
    }

    public class FireManipulatorProjectile : OrbProjectile
    {
        [Header("Fire Manipulator Projectile Settings")]
        public FireManipulatorType FireManipulatorType;
        [Tooltip("Set fire -> Add flame DPS, Remove fire -> Remove flame DPS")]
        public int FlameManipulationDPS = 10;

        protected override void OnCollide(RaycastHit hit)
        {
            base.OnCollide(hit);

            var burnable = hit.collider.GetComponent<IBurn>();

            if (burnable == null)
            {
                return;
            }

            if (FireManipulatorType == FireManipulatorType.SetFire)
            {
                burnable.AddFire(FlameManipulationDPS);
            }
            else if (FireManipulatorType == FireManipulatorType.RemoveFire)
            {
                burnable.RemoveFire(FlameManipulationDPS);
            }
        }
    }
}