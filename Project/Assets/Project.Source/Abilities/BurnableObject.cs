using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBurn
{
    public void AddFire(int fireDPS);
    public void RemoveFire(int fireDPSRemoved);
}

[RequireComponent(typeof(MeshRenderer))]
public class BurnableObject : MonoBehaviour, IBurn
{
    [Header("Dependencies")]
    [SerializeField] private Light lightObjectPrefab;

    [Header("Optional")]
    [SerializeField] private GameObject fracturedVersionPrefab;
    
    [Header("Settings")]
    public int MaxHealth = 100;
    
    [Header("Light Settings")]
    public int LightStartingHeight = 6;
    public int MinIntensity = 80;
    public int MaxIntensity = 170;
    public AnimationCurve healthToIntensityCurve;

    private MeshRenderer meshRenderer;
    
    private int CurrentHealth = 0;
    private int currentFireDPS = 0;
    private bool isOnFire = false;
    private bool isFractured = false; 

    private float timeElapsedSinceDamaged = 0;
    private Light spawnedLight;
    private GameObject spawnedFracturedObject;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
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

        if(spawnedLight != null)
            UpdateLightIntensity();

        if (CurrentHealth != MaxHealth && !isFractured)
            Fracture();

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

    private void UpdateLightIntensity()
    {
        float healthPercent = (float)CurrentHealth/MaxHealth;
        float intensity = healthToIntensityCurve.Evaluate(healthPercent) * MaxIntensity;
        intensity = Mathf.Clamp(intensity, MinIntensity, MaxIntensity);

        spawnedLight.intensity = intensity;
    }
    
    private void Fracture()
    {
        isFractured = true;

        if (fracturedVersionPrefab != null && spawnedFracturedObject == null)
        {
            spawnedFracturedObject = Instantiate(fracturedVersionPrefab, transform.position, transform.rotation);
            meshRenderer.enabled = false;
        }
    }

    private void Expire()
    {
        if (spawnedLight != null) 
            Destroy(spawnedLight);
        
        if(spawnedFracturedObject != null)
            Destroy(spawnedFracturedObject);
        
        Destroy(gameObject);
    }
}
