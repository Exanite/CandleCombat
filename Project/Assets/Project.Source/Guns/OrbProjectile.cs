using System;
using System.Collections;
using System.Collections.Generic;
using Project.Source.Characters;
using UnityEngine;

public class OrbProjectile : Projectile
{
    [Header("Dependencies")]
    [SerializeField] private Rigidbody rb;

    [Header("Settings")]
    [SerializeField] private bool retainCharacterVelocity = false;
    [SerializeField] private float damage = 1f;
    [SerializeField] private float timeToExpire = 1f;
    [SerializeField] private float speed = 1;
    [SerializeField] private float acceleration = 0.1f;

    private Character owner;
    private float lifetime = 0;
    
    public override void Fire(Character characterFrom, Vector3 direction)
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
        Character otherCharacter = other.gameObject.GetComponent<Character>();
        
        if(otherCharacter != null)
            Hit(otherCharacter);
    }

    private void Expire()
    {
        Debug.Log("Destroy!");
        
        Destroy(gameObject);
    }
}
