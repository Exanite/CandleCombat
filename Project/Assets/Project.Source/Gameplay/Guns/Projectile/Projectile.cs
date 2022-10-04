using Project.Source.Gameplay.Characters;
using UnityEngine;

namespace Project.Source.Gameplay.Guns.Projectile
{
    public abstract class Projectile : MonoBehaviour
    {
        public abstract void Fire(Character characterFrom, Vector3 direction, Vector3 visualPosition);
        public abstract void Hit(Character character);
    }
}
