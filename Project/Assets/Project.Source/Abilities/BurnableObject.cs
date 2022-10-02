using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

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
    [SerializeField] private VisualEffect fireVisualEffect;

    [Header("Optional")]
    [SerializeField] private GameObject fracturedVersionPrefab;
    
    [Header("Settings")]
    public int MaxHealth = 100;

    [Header("Light Settings")]
    [Range(1, 14)]
    public float MaxFireSize = 6;
    [Range(1, 14)]
    public int MinFireSize = 2;
    public int LightStartingHeight = 6;
    public int MinIntensity = 80;
    public int MaxIntensity = 170;
    public AnimationCurve healthToIntensityCurve;

    private MeshRenderer meshRenderer;
    
    private float CurrentHealth = 0;
    private int currentFireDPS = 0;
    private bool isOnFire = false;
    private bool isFractured = false; 

    private float timeElapsedSinceDamaged = 0;
    private Light spawnedLight;
    private VisualEffect spawnedFireVisualEffect;
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
        
        CurrentHealth -= currentFireDPS * Time.deltaTime;

        if(spawnedLight != null)
            UpdateLightIntensity();

        if (CurrentHealth <= MaxHealth && !isFractured)
            Fracture();

        if(CurrentHealth <= 0)
            Expire();
    }

    public void AddFire(int fireDPS)
    {
        if (isOnFire || fireDPS < currentFireDPS) return;
        
        currentFireDPS = fireDPS;
        isOnFire = true;

        if (spawnedLight == null)
        {
            Vector3 highSpawnPosition = transform.position + (transform.up * LightStartingHeight);
            spawnedLight = Instantiate(lightObjectPrefab, highSpawnPosition, Quaternion.identity);

            if (fireVisualEffect != null)
            {
                
                spawnedFireVisualEffect = Instantiate(fireVisualEffect, transform.position, Quaternion.identity);
                spawnedFireVisualEffect.SetFloat("FlamesSize", MaxFireSize);
            }
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
        float healthPercent = 1 - (float)CurrentHealth/MaxHealth;
        float intensity = healthToIntensityCurve.Evaluate(healthPercent) * MaxIntensity;
        intensity = Mathf.Clamp(intensity, MinIntensity, MaxIntensity);
        
        Debug.Log("Intensite: " + intensity);

        spawnedLight.intensity = intensity;

        float size = healthToIntensityCurve.Evaluate(healthPercent) * MaxFireSize;
        size = Mathf.Clamp(size, MinFireSize, MaxFireSize);
        
        if(fireVisualEffect != null)
            fireVisualEffect.SetFloat("FlamesSize", size);
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
            Destroy(spawnedLight.gameObject);
        
        if(spawnedFracturedObject != null)
            Destroy(spawnedFracturedObject.gameObject);
        
        if(spawnedFireVisualEffect != null)
            Destroy(spawnedFireVisualEffect.gameObject);
        
        Destroy(gameObject);
    }
}
