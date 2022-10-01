using System;
using System.Collections;
using System.Collections.Generic;
using Project.Source;
using Project.Source.Characters;
using UnityEngine;

public class LineProjectile : Projectile
{
    [Header("Dependencies")]
    [SerializeField] private LineRenderer linePrefab;

    [Header("Settings")]
    [SerializeField] private float damage = 1f;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private float timeToExpire = 1f;
    //[SerializeField] private float timeToFire;
    //[SerializeField] private float hitDelay;

    GameObject spawned;
    private Character owner;
    private float lifetime = 0;
    private bool fired;

    public override void Fire(Character characterFrom, Vector3 direction)
    {
        owner = characterFrom;
        var tPosition = transform.position;
        LineRenderer line = Instantiate(linePrefab, tPosition, Quaternion.Euler(direction));
        line.SetPosition(0, Vector3.zero);

        RaycastHit hit;
        if (Physics.Raycast(tPosition, direction, out hit))
        {
            line.SetPosition(1, direction * hit.distance);
            Debug.Log("Hit: " + hit.collider.gameObject);

            Character character = hit.collider.gameObject.GetComponent<Character>();
            if(character != null)
                Hit(character);
        }
        else
        {
            line.SetPosition(1, direction * maxDistance);
        }
        
        spawned = line.gameObject;
    }

    public override void Hit(Character character)
    {
        bool sameTeam = owner.IsPlayer == character.IsPlayer;
        if (sameTeam || character.IsDodging) return;

        character.CurrentHealth -= damage;
    }
    
    private void Update()
    {
        lifetime += Time.deltaTime;

        if(lifetime > timeToExpire)
            Expire();
    }

    private void Expire()
    {
        Debug.Log("Destroy!");
        
        if(spawned != null)
            Destroy(spawned);
        Destroy(gameObject);
    }
}
