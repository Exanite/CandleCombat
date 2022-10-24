using Project.Source.Gameplay.Characters;
using UniDi;
using UnityEngine;
using UnityEngine.VFX;

namespace Project.Source.Gameplay.Guns.Projectile
{
    public class OrbProjectile : Projectile
    {
        [Header("Dependencies")]
        [SerializeField] protected Rigidbody rb;
        [SerializeField] protected VisualEffect hitEffect;

        [Header("Settings")]
        [SerializeField] protected bool retainCharacterVelocity;
        [SerializeField] protected float damage = 1f;
        [SerializeField] protected float timeToExpire = 1f;
        [SerializeField] protected float speed = 1;
        [SerializeField] protected float acceleration = 0.1f;
        [SerializeField] protected float colliderRadius = 0.1f;

        protected float lifetime;

        [Inject]
        private IInstantiator instantiator;

        [Inject]
        private PhysicsScene physicsScene;

        [Inject]
        private GameContext gameContext;

        private void OnEnable()
        {
            gameContext.AllOrbProjectiles.Add(this);
        }

        private void OnDisable()
        {
            gameContext.AllOrbProjectiles.Remove(this);
        }

        private void FixedUpdate()
        {
            lifetime += Time.deltaTime;
            if (lifetime > timeToExpire)
            {
                Despawn();

                return;
            }

            var distance = Time.deltaTime * rb.velocity.magnitude;
            if (physicsScene.SphereCast(transform.position, colliderRadius, rb.velocity.normalized, out var hit, distance,
                queryTriggerInteraction: QueryTriggerInteraction.Ignore))
            {
                OnCollide(hit);
            }

            if (acceleration != 0)
            {
                var velocity = rb.velocity;
                velocity += new Vector3(velocity.x * acceleration, 0, velocity.z * acceleration);
                rb.velocity = velocity;
            }
        }

        public override void Fire(Character owningCharacter, Vector3 direction, Vector3 visualPosition)
        {
            OwningCharacter = owningCharacter;
            transform.position = visualPosition;
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            rb.velocity = direction * speed;

            if (retainCharacterVelocity)
            {
                rb.velocity += owningCharacter.Rigidbody.velocity;
            }
        }

        protected virtual void OnCollide(RaycastHit hit)
        {
            if (hitEffect)
            {
                instantiator.InstantiatePrefabForComponent<VisualEffect>(hitEffect, hit.point, Quaternion.LookRotation(hit.normal, Vector3.up), null);
            }

            if (hit.collider.attachedRigidbody && hit.collider.attachedRigidbody.TryGetComponent(out Character otherCharacter))
            {
                Hit(otherCharacter);
            }
            else
            {
                Despawn();
            }
        }

        public override void Hit(Character character)
        {
            var isSameTeam = OwningCharacter.IsPlayer == character.IsPlayer;
            if (isSameTeam)
            {
                return;
            }

            character.TakeDamage(damage);

            Despawn();
        }

        public override void Despawn()
        {
            Destroy(gameObject);
        }
    }
}