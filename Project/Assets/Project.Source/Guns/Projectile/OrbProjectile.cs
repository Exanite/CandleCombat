using Project.Source.Characters;
using UnityEngine;
using UnityEngine.VFX;

public class OrbProjectile : Projectile
{
    [Header("Dependencies")]
    [SerializeField]
    protected Rigidbody rb;
    [SerializeField]
    protected VisualEffect hitEffect;

    [Header("Settings")]
    [SerializeField]
    protected bool retainCharacterVelocity;
    [SerializeField]
    protected float damage = 1f;
    [SerializeField]
    protected float timeToExpire = 1f;
    [SerializeField]
    protected float speed = 1;
    [SerializeField]
    protected float acceleration = 0.1f;
    [SerializeField]
    protected float colliderRadius = 0.1f;

    protected Character owner;
    protected float lifetime;

    public override void Fire(Character characterFrom, Vector3 direction, Vector3 visualPosition)
    {
        owner = characterFrom;
        transform.forward = direction;
        rb.velocity = direction * speed;

        if (retainCharacterVelocity)
        {
            rb.velocity += characterFrom.Rigidbody.velocity;
        }
    }

    protected virtual void OnCollide(RaycastHit hit)
    {
        if (hitEffect)
        {
            Instantiate(hitEffect, hit.point, Quaternion.LookRotation(hit.normal, Vector3.up));
        }

        var otherCharacter = hit.collider.GetComponent<Character>();
        if (otherCharacter != null)
        {
            Hit(otherCharacter);
        }

        // Debug.Log($"{name} Hit");

        Destroy(gameObject);
    }

    public override void Hit(Character character)
    {
        var isSameTeam = owner.IsPlayer == character.IsPlayer;
        if (isSameTeam)
        {
            return;
        }

        character.TakeDamage(damage);

        Expire();
    }

    private void FixedUpdate()
    {
        lifetime += Time.deltaTime;

        if (lifetime > timeToExpire)
        {
            Expire();
        }

        var distance = Time.deltaTime * rb.velocity.magnitude;
        if (Physics.SphereCast(transform.position, colliderRadius, rb.velocity.normalized, out var hit, distance))
        {
            OnCollide(hit);
        }

        if (acceleration == 0)
        {
            return;
        }

        var velocity = rb.velocity;
        velocity += new Vector3(velocity.x * acceleration, 0, velocity.z * acceleration);
        rb.velocity = velocity;
    }

    protected void Expire()
    {
        // Debug.Log("Destroy!");

        Destroy(gameObject);
    }
}