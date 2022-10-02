using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBurn
{
    public void AddFire(int fireDPS);
    public void RemoveFire(int fireDPSRemoved);
}

public class BurnableObject : MonoBehaviour, IBurn
{
    [Header("Dependencies")]
    [SerializeField] private Light lightObjectPrefab;

    [Header("Settings")]
    public int MaxHealth = 100;
    public int CurrentHealth = 100;
    
    [Header("Light Settings")]
    public int LightStartingHeight = 6;
    public int MinIntensity = 80;
    public int MaxIntensity = 170;
    public AnimationCurve healthToIntensityCurve;
    
    private int currentFireDPS = 0;
    private bool isOnFire = false;

    private float timeElapsedSinceDamaged = 0;
    private Light spawnedLight;

    private void Awake()
    {
        CurrentHealth = MaxHealth;
    }

    private void Update()
    {
        if (!isOnFire) return;

        timeElapsedSinceDamaged += Time.deltaTime;

        if (timeElapsedSinceDamaged > 1)
        {
            CurrentHealth -= currentFireDPS;
            timeElapsedSinceDamaged--;
        }

        float healthPercent = (float)CurrentHealth/MaxHealth;
        float intensity = healthToIntensityCurve.Evaluate(healthPercent) * MaxIntensity;
        intensity = Mathf.Clamp(intensity, MinIntensity, MaxIntensity);

        spawnedLight.intensity = intensity;

        if(CurrentHealth <= 0)
            Expire();
    }

    public void AddFire(int fireDOT)
    {
        currentFireDPS = fireDOT;
        isOnFire = true;

        if (spawnedLight == null)
        {
            Vector3 spawnPosition = transform.position + (transform.up * LightStartingHeight);
            spawnedLight = Instantiate(lightObjectPrefab, spawnPosition, Quaternion.identity);
        }
        else
            spawnedLight.gameObject.SetActive(true);
    }

    public void RemoveFire(int fireDPSRemoved)
    {
        isOnFire = false;

        if (spawnedLight != null)
        {
            currentFireDPS -= fireDPSRemoved;
            
            if (currentFireDPS <= 0)
            {
                currentFireDPS = 0;
                spawnedLight.gameObject.SetActive(false);
            }
        }
    }

    private void Expire()
    {
        if (spawnedLight != null) 
            Destroy(spawnedLight);
        
        Destroy(gameObject);
    }
}
