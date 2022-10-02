using System.Collections;
using System.Collections.Generic;
using Project.Source;
using Project.Source.Characters;
using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    public abstract void Fire(Character characterFrom, Vector3 direction, Vector3 visualPosition);
    public abstract void Hit(Character character);
}
