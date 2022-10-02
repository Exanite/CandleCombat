using System;
using System.Collections;
using System.Collections.Generic;
using Project.Source;
using Project.Source.Characters;
using UnityEngine;
using Random = UnityEngine.Random;

public enum GunHoldType{
    OneHandGun,
    OneHandFan
}

public class Gun : MonoBehaviour
{
    public Action OnShoot;
    public Action OnReload;
    
    [Header("Dependencies")]
    [SerializeField] private Transform model;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform firePointVisual;
    [SerializeField] private List<Projectile> projectilePrefabs = new List<Projectile>();

    [Header("Sounds")]
    [SerializeField] private AudioClip fireAudioClip;
    [Range(0, 1)]
    [SerializeField] private float fireAudioScale; 
    
    [Header("Settings")]
    public GunHoldType GunHoldType = GunHoldType.OneHandGun;
    public float TimeToHolsterGun = 4f;
    public bool ReloadOnSwitchTo = false;
    public int SelectedProjectile = 0;
    public int Burst = 1;
    public float BurstRadius = 1f;
    public float TimeBetweenShots = 1f;
    public int MaxAmmo = 1;
    public float ReloadTime = 2f;

    private float elapsedTimeSinceHolstered = 0f;
    private float elapsedTimeSinceShot = 0f;
    private float elapsedReloadTime = 0f;
    private int ammo = 0;
    private bool isReloading = false;

    private void Awake()
    {
        Reload();
    }
    
    private void Update()
    {
        elapsedTimeSinceShot += Time.deltaTime;
        elapsedTimeSinceShot = Mathf.Clamp(elapsedTimeSinceShot, 0, TimeBetweenShots);
        elapsedTimeSinceHolstered += Time.deltaTime;
        elapsedTimeSinceHolstered = Mathf.Clamp(elapsedTimeSinceHolstered, 0, TimeToHolsterGun);
        
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
            if (ammo <= 0) continue;

            if (i > 0)
            {
                Vector2 randomVector = Random.insideUnitCircle * BurstRadius;
                direction += new Vector3(randomVector.x, 0, 0);
            }
            
            Projectile projectile = Instantiate(projectilePrefabs[SelectedProjectile], firePoint.position, Quaternion.Euler(direction));
            projectile.Fire(characterFrom, direction, firePointVisual.position);
            
            ammo--;
            OnShoot?.Invoke();

            if (fireAudioClip != null)
                GameContext.Instance.AudioSource.PlayOneShot(fireAudioClip, fireAudioScale);
        }

        elapsedTimeSinceShot -= TimeBetweenShots;
        elapsedTimeSinceHolstered = 0;
    }

    public void SwitchAmmo(int index)
    {
        if (index >= 0 && index < projectilePrefabs.Count)
            SelectedProjectile = index;
    }

    public void OnSwitch()
    {
        if(ReloadOnSwitchTo)
            Reload();
    }

    public bool IsFiring()
    {
        return elapsedTimeSinceShot < TimeBetweenShots;
    }

    //For animation purposes
    public bool IsHolstered()
    {
        return elapsedTimeSinceHolstered >= TimeToHolsterGun;
    }

    public Transform GetModel()
    {
        return model;
    }

    private void Reload()
    {
        OnReload?.Invoke();
        
        ammo = MaxAmmo;
        isReloading = false;
        elapsedReloadTime = 0;
        elapsedTimeSinceShot = TimeBetweenShots;
        elapsedTimeSinceHolstered = TimeToHolsterGun;
    }

    private void OnValidate()
    {
        SelectedProjectile = Mathf.Clamp(SelectedProjectile, 0, projectilePrefabs.Count - 1);
        MaxAmmo = Mathf.Clamp(MaxAmmo, Burst, 100); //Arbitrary max
    }
}
