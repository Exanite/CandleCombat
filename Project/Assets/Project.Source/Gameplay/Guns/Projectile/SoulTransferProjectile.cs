using Project.Source.Gameplay.Characters;
using UniDi;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace Project.Source.Gameplay.Guns.Projectile
{
    public class SoulTransferProjectile : LineProjectile
    {
        [Header("Soul Transfer Specific")]
        [SerializeField] private VisualEffect vfxPrefab;

        private static readonly ExposedProperty PlayerVelocityAttribute = "Player Velocity";

        [Inject]
        private GameContext gameContext;

        [Inject]
        private IInstantiator instantiator;

        public override void Hit(Character character)
        {
            if (character.IsDead)
            {
                return;
            }

            gameContext.Possess(character);
        }

        public override void CreateVisual(Vector3 startPosition, Vector3 endPosition, float distance, Vector3 direction)
        {
            var vfx = instantiator.InstantiatePrefabForComponent<VisualEffect>(vfxPrefab, startPosition, Quaternion.identity, null);
            vfx.transform.up = direction;
            vfx.SetVector3(PlayerVelocityAttribute, new Vector3(0, -distance, 0));

            visual = vfx.gameObject;
        }
    }
}