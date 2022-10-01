using System;
using System.Collections;
using System.Collections.Generic;
using Project.Source;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public int selectedProjectile = 0;
    public int burst = 1;

    [SerializeField] private Transform firePoint;
    [SerializeField] private List<Projectile> projectilePrefabs = new List<Projectile>();
    
    public void Fire(Character characterFrom, Vector2 characterVelocity)
    {
        Vector3 direction = firePoint.forward;
        for (int i = 0; i < burst; i++)
        {
            Projectile projectile = Instantiate(projectilePrefabs[selectedProjectile], firePoint.position, Quaternion.Euler(firePoint.forward));
            projectile.Fire(characterFrom, characterVelocity, direction);
        }
    }

    private void OnValidate()
    {
        selectedProjectile = Mathf.Clamp(selectedProjectile, 0, projectilePrefabs.Count);
    }
}
