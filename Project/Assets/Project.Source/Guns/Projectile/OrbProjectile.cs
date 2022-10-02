using System;
using System.Collections;
using System.Collections.Generic;
using Project.Source.Characters;
using UnityEngine;

public class OrbProjectile : Projectile
{
    [Header("Dependencies")]
    [SerializeField] protected Rigidbody rb;

    [Header("Settings")]
    [SerializeField] protected bool retainCharacterVelocity = false;
    [SerializeField] protected float damage = 1f;
    [SerializeField] protected float timeToExpire = 1f;
    [SerializeField] protected float speed = 1;
    [SerializeField] protected float acceleration = 0.1f;

    protected Character owner;
    protected float lifetime = 0;
    
    public override void Fire(Character characterFrom, Vector3 direction, Vector3 visualPosition)
    {
        owner = characterFrom;
        rb.velocity = direction * speed;

        if (retainCharacterVelocity)
            rb.velocity += characterFrom.Rigidbody.velocity;
    }

    public override void Hit(Character character)
    {
        if (owner.IsPlayer == character.IsPlayer) return;

        character.TakeDamage(damage);
        
        Expire();
    }

    private void FixedUpdate()
    {
        lifetime += Time.deltaTime;

        if(lifetime > timeToExpire)
            Expire();
        
        if (acceleration == 0) return;

        var velocity = rb.velocity;
        velocity += new Vector3(velocity.x * acceleration,0, velocity.z * acceleration);
        rb.velocity = velocity;
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleTrigger(other);
    }
    
    protected virtual void HandleTrigger(Collider other)
    {
        Character otherCharacter = other.gameObject.GetComponent<Character>();
        
        if(otherCharacter != null)
            Hit(otherCharacter);
    }

    protected void Expire()
    {
        Debug.Log("Destroy!");
        
        Destroy(gameObject);
    }
}
