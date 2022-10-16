using Project.Source.Gameplay.Characters;
using UniDi;
using UnityEngine;

namespace Project.Source.Gameplay.Guns.Projectile
{
    public enum ForwardType
    {
        RedX,
        GreenY,
        BlueZ,
    }

    public class LineProjectile : Projectile
    {
        [Header("Dependencies")]
        [SerializeField] private LineRenderer linePrefab;

        [Header("Settings")]
        [SerializeField] private float damage = 1f;
        [SerializeField] private float radius = 1f;
        [SerializeField] private float maxDistance = 10f;
        [SerializeField] private float timeToExpire = 1f;

        protected GameObject visual;
        private float lifetime = 0;
        private bool fired;

        [Inject]
        private IInstantiator instantiator;

        [Inject]
        private PhysicsScene physicsScene;

        public override void Fire(Character owningCharacter, Vector3 direction, Vector3 visualPosition)
        {
            OwningCharacter = owningCharacter;

            var startPosition = transform.position;
            var endPosition = direction * maxDistance;

            if (physicsScene.SphereCast(startPosition, radius, direction, out var hit, maxDistance))
            {
                endPosition = direction * hit.distance;

                if (hit.collider.attachedRigidbody && hit.collider.attachedRigidbody.TryGetComponent(out Character character))
                {
                    Hit(character);
                }
            }
            
            CreateVisual(visualPosition, endPosition, hit.distance, direction);
        }

        public override void Hit(Character character)
        {
            var sameTeam = OwningCharacter.IsPlayer == character.IsPlayer;
            if (sameTeam || character.IsInvulnerable)
            {
                return;
            }

            character.TakeDamage(damage);
        }

        private void Update()
        {
            lifetime += Time.deltaTime;

            if (lifetime > timeToExpire)
            {
                Expire();
            }
        }

        public virtual void CreateVisual(Vector3 startPosition, Vector3 endPosition, float distance, Vector3 direction)
        {
            var line = instantiator.InstantiatePrefabForComponent<LineRenderer>(linePrefab, startPosition, Quaternion.Euler(direction), null);
            line.SetPosition(0, Vector3.zero);
            line.SetPosition(1, endPosition);

            visual = line.gameObject;
        }

        private void Expire()
        {
            Destroy(visual);
            Destroy(gameObject);
        }
    }
}