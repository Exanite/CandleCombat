using System;
using System.Collections;
using System.Collections.Generic;
using Project.Source;
using Project.Source.Characters;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private List<Projectile> projectilePrefabs = new List<Projectile>();
    
    [Header("Settings")]
    public int SelectedProjectile = 0;
    public int Burst = 1;
    public float TimeBetweenShots = 1f;
    public int MaxAmmo = 1;
    public float ReloadTime = 2f;
    
    private float elapsedTimeSinceShot = 0f;
    private float elapsedReloadTime = 0f;
    private int ammo = 0;
    private bool isReloading = false;

    private void Start()
    {
        ammo = MaxAmmo;
    }
    
    private void Update()
    {
        elapsedTimeSinceShot += Time.deltaTime;
        elapsedTimeSinceShot = Mathf.Clamp(elapsedTimeSinceShot, 0, TimeBetweenShots);

        if (isReloading && elapsedReloadTime >= ReloadTime)
        {
            Reload();
        }
        else if(ammo == 0)
        {
            isReloading = true;
            elapsedReloadTime += Time.deltaTime;
        }
    }

    public void Fire(Character characterFrom)
    {
        if (elapsedTimeSinceShot < TimeBetweenShots || isReloading) return;

        Vector3 direction = firePoint.forward;
        
        for (int i = 0; i < Burst; i++)
        {
            if (ammo == 0) return;
            
            Projectile projectile = Instantiate(projectilePrefabs[SelectedProjectile], firePoint.position, Quaternion.Euler(firePoint.forward));
            projectile.Fire(characterFrom, direction);
            
            ammo--;
        }

        elapsedTimeSinceShot -= TimeBetweenShots;
    }

    private void Reload()
    {
        ammo = MaxAmmo;
        isReloading = false;
        elapsedReloadTime = 0;
    }

    private void OnValidate()
    {
        SelectedProjectile = Mathf.Clamp(SelectedProjectile, 0, projectilePrefabs.Count - 1);
        MaxAmmo = Mathf.Clamp(MaxAmmo, Burst, 100); //Arbitrary max
    }
}
