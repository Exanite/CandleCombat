using System;
using System.Collections;
using System.Collections.Generic;
using Project.Source;
using Project.Source.Characters;
using UnityEngine;

public enum ForwardType
{
    RedX,
    GreenY,
    BlueZ
}

public class LineProjectile : Projectile
{
    [Header("Dependencies")]
    [SerializeField] private LineRenderer linePrefab;

    [Header("Settings")]
    [SerializeField] private ForwardType projectileVisualForward = ForwardType.BlueZ; 
    [SerializeField] private float startPositionOffset = -1f;
    [SerializeField] private float damage = 1f;
    [SerializeField] private float radius = 1f;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private float timeToExpire = 1f;
    //[SerializeField] private float timeToFire;
    //[SerializeField] private float hitDelay;

    protected GameObject spawned;
    private Character owner;
    private float lifetime = 0;
    private bool fired;
    
    public override void Fire(Character characterFrom, Vector3 direction, Vector3 visualPosition)
    {
        owner = characterFrom;

        Vector3 startPosition = transform.position;
        Vector3 endPosition = Vector3.zero;

        Ray ray = new Ray(startPosition, direction);

        if (Physics.SphereCast(ray, radius, out RaycastHit hit, maxDistance))
        {
            endPosition = direction * hit.distance;
            Debug.Log("Hit: " + hit.collider.gameObject);

            Character character = hit.collider.gameObject.GetComponent<Character>();
            if(character != null)
                Hit(character);
        }
        else
        {
            endPosition = direction * maxDistance;
        }
        
        CreateVisual(visualPosition, endPosition, hit.distance, direction);
    }

    public override void Hit(Character character)
    {
        bool sameTeam = owner.IsPlayer == character.IsPlayer;
        if (sameTeam || character.IsInvulnerable) return;

        character.TakeDamage(damage);
    }
    
    private void Update()
    {
        lifetime += Time.deltaTime;

        if(lifetime > timeToExpire)
            Expire();
    }

    public virtual void CreateVisual(Vector3 startPosition, Vector3 endPosition, float distance, Vector3 direction)
    {
        LineRenderer line = Instantiate(linePrefab, startPosition, Quaternion.Euler(direction));
        line.SetPosition(0, Vector3.zero);
        line.SetPosition(1, endPosition);
        
        spawned = line.gameObject;
    }

    private int GetLayerToHit(Character character)
    {
        /*
        int playerLayer = LayerMask.NameToLayer("Player");
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        
        int layerMask;
        if (character.IsPlayer)
            layerMask = ~playerLayer;
        else
            layerMask = ~enemyLayer;
        */
        return 0;
    }

    private Vector3 GetStartOffsetVector()
    {
        Vector3 offsetDirection = Vector3.zero;
        
        return transform.forward * startPositionOffset;
    }
    
    private void Expire()
    {
        Debug.Log("Destroy!");
        
        if(spawned != null)
            Destroy(spawned);
        Destroy(gameObject);
    }
}
