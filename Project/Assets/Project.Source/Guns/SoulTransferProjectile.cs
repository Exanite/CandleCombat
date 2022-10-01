using System.Collections;
using System.Collections.Generic;
using Project.Source;
using Project.Source.Characters;
using UnityEngine;

public class SoulTransferProjectile :  LineProjectile
{
    public override void Hit(Character character)
    {
        GameContext.Instance.CurrentPlayer = character;
        
        //TODO: Don't reach into directly.
        var gunController = GameContext.Instance.gameObject.GetComponent<PlayerGunController>();
        gunController.EquippedGunIndex = 0;
    }
}
