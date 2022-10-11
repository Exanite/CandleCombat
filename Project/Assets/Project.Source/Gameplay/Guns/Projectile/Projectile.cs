using Project.Source.Gameplay.Characters;
using UnityEngine;

namespace Project.Source.Gameplay.Guns.Projectile
{
    public abstract class Projectile : MonoBehaviour
    {
        public Character OwningCharacter { get; protected set; }
        
        public abstract void Fire(Character owningCharacter, Vector3 direction, Vector3 visualPosition);
        public abstract void Hit(Character character);
    }
}
