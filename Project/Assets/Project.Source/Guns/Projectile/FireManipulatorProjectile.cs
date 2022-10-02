using System.Collections;
using System.Collections.Generic;
using Project.Source.Characters;
using UnityEngine;

public enum FireManipulatorType
{
    SetFire,
    RemoveFire
}

public class FireManipulatorProjectile : OrbProjectile
{
    [Header("Fire Manipulator Projectile Settings")]
    public FireManipulatorType FireManipulatorType;
    [Tooltip("Set fire -> Add flame DPS, Remove fire -> Remove flame DPS")]
    public int FlameManipulationDPS = 10;

    protected override void HandleTrigger(Collider other)
    {
        base.HandleTrigger(other);

        IBurn burnable = other.GetComponent<IBurn>();

        if (burnable == null) return;
        
        if(FireManipulatorType == FireManipulatorType.SetFire)
            burnable.AddFire(FlameManipulationDPS);
        else if (FireManipulatorType == FireManipulatorType.RemoveFire)
            burnable.RemoveFire(FlameManipulationDPS);
    }
}
