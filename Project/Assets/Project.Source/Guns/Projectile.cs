using System.Collections;
using System.Collections.Generic;
using Project.Source;
using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    public abstract void Fire(Character characterFrom, Vector3 direction);
    public abstract void Hit(Character character);
}
