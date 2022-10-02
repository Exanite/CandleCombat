using System;
using System.Collections;
using System.Collections.Generic;
using Project.Source;
using Project.Source.Characters;
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

    [SerializeField] private string flameSizeAttribute = "FlamesSize";
    [SerializeField] private string flameColorAttribute = "Color";

    [Header("Light Settings")]
    [ColorUsage(true, true)]
    public Color PossessColor = new Vector4(0,4.7311759f,22.0403557f,1);
    public Color PossessLightColor = new Vector4(0,4.7311759f,22.0403557f, 1);
    public float PossessSwitchTime = 0.5f;
    
    [Range(1, 14)]
    public float MaxFireSize = 6;
    [Range(1, 14)]
    public int MinFireSize = 2;
    public int LightStartingHeight = 6;
    public int MinIntensity = 80;
    public int MaxIntensity = 170;
    public AnimationCurve healthToIntensityCurve;

    private MeshRenderer meshRenderer;

    private Character currentCharacter;
    private float CurrentHealth = 0;
    private float timeElapsedSinceDamaged = 0;
    private int currentFireDPS = 0;
    private bool isOnFire = false;
    private bool isFractured = false; 
    
    private float timeElapsedSincePossess = 0;
    private Color originalLightColor = Color.white;
    private float originalLightColorTemperature;
    private Color originalFireColor = Color.white;
    private bool wasPossessed = false;

    private Light spawnedLight;
    private VisualEffect spawnedFireVisualEffect;
    private GameObject spawnedFracturedObject;

    private int possesses = 0;
    
    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        CurrentHealth = MaxHealth;
        originalFireColor.a = 255;
    }

    private void OnEnable()
    {
        GameContext.Instance.Possessed += HandlePossessed;
    }

    private void OnDisable()
    {
        GameContext.Instance.Possessed -= HandlePossessed;
    }

    private void Update()
    {
        if (!isOnFire) return;

        timeElapsedSinceDamaged += Time.deltaTime;
        timeElapsedSincePossess += Time.deltaTime;
        
        CurrentHealth -= currentFireDPS * Time.deltaTime;

        if(spawnedLight != null)
            UpdateLightIntensity();

        if (CurrentHealth <= MaxHealth && !isFractured)
            Fracture();

        if(CurrentHealth <= 0)
            Expire();

        if (wasPossessed && timeElapsedSincePossess > PossessSwitchTime)
        {
            HandleReturnFromPossessed();
        }
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
                spawnedFireVisualEffect.SetFloat(flameSizeAttribute, MaxFireSize);
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
        
        Debug.Log("Intensity: " + intensity);

        spawnedLight.intensity = intensity;

        float size = healthToIntensityCurve.Evaluate(healthPercent) * MaxFireSize;
        size = Mathf.Clamp(size, MinFireSize, MaxFireSize);
        
        if(fireVisualEffect != null)
            fireVisualEffect.SetFloat(flameSizeAttribute, size);
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

    private void HandlePossessed(Character character)
    {
        //TODO: Remove this hack.
        possesses++;
        if (possesses < 2) return;
        
        wasPossessed = true;
        
        if (spawnedLight != null)
        {
            originalLightColor = spawnedLight.color;
            spawnedLight.color = PossessLightColor;

            originalLightColorTemperature = spawnedLight.colorTemperature; 
            spawnedLight.colorTemperature = 4500f; //TODO: Remove magic number. 
        }
        
        if (spawnedFireVisualEffect != null)
        {
            originalFireColor = spawnedFireVisualEffect.GetVector4(flameColorAttribute);
            spawnedFireVisualEffect.SetVector4(flameColorAttribute, PossessColor);
        }
        
        timeElapsedSincePossess = 0;
    }

    private void HandleReturnFromPossessed()
    {
        //TODO: Remove this hack.
        if (possesses < 2) return;
        
        wasPossessed = false;
        
        if (spawnedLight != null)
        {
            spawnedLight.color = originalLightColor;
            spawnedLight.colorTemperature = originalLightColorTemperature;
        }

        if (spawnedFireVisualEffect != null)
        {
            spawnedFireVisualEffect.SetVector4(flameColorAttribute, originalFireColor);
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
