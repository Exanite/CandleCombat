using System.Collections;
using System.Collections.Generic;
using Project.Source.Characters;
using UnityEngine;

public class BlazeProjectile : OrbProjectile
{
    [Header("Blaze Projectile Settings")]
    public int flameDPS = 10;

    protected override void HandleTrigger(Collider other)
    {
        base.HandleTrigger(other);

        IBurn burnable = other.GetComponent<IBurn>();

        if (burnable != null)
        {
            burnable.AddFire(flameDPS);
        }
    }
}
