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

[RequireComponent(typeof(MeshRenderer), typeof(AudioSource))]
public class BurnableObject : MonoBehaviour, IBurn
{
    [Header("Dependencies")]
    [SerializeField] private Light lightObjectPrefab;
    [SerializeField] private VisualEffect fireVisualEffect;

    [Header("Optional")]
    [SerializeField] private GameObject fracturedVersionPrefab;
    [Tooltip("Audio is only enabled and disabled.")]
    private AudioSource audioSource;
    
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

    private GameContext subscribedGameContext;
    
    private void Awake()
    {
        audioSource = GetComponent <AudioSource>();
        meshRenderer = GetComponent<MeshRenderer>();
        CurrentHealth = MaxHealth;
        originalFireColor.a = 255;
        audioSource.enabled = false;
    }

    private void Start()
    {
        SpawnLight();
        SpawnFire();
        SpawnFracture();
    }

    private void OnEnable()
    {
        try
        {
            subscribedGameContext = GameContext.Instance;
        }
        catch (NullReferenceException)
        {
            // Blank
        }

        if (subscribedGameContext)
        {
            subscribedGameContext.Possessed += HandlePossessed;
        }
    }

    private void OnDisable()
    {
        if (subscribedGameContext)
        {
            subscribedGameContext.Possessed -= HandlePossessed;
        }
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
        
        spawnedLight.gameObject.SetActive(true);
        spawnedFireVisualEffect.gameObject.SetActive(true);
        audioSource.enabled = true;
    }

    public void RemoveFire(int fireDPSRemoved)
    {
        isOnFire = false;
        
        currentFireDPS -= fireDPSRemoved;
            
        if (currentFireDPS <= 0)
        {
            currentFireDPS = 0;
            spawnedLight.gameObject.SetActive(false);
            fireVisualEffect.gameObject.SetActive(false);
            audioSource.enabled = false;
        }
    }

    private void UpdateLightIntensity()
    {
        float healthPercent = 1 - (float)CurrentHealth/MaxHealth;
        float intensity = healthToIntensityCurve.Evaluate(healthPercent) * MaxIntensity;
        intensity = Mathf.Clamp(intensity, MinIntensity, MaxIntensity);
        
        // Debug.Log("Intensity: " + intensity);

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
            spawnedLight.color = PossessLightColor;
            spawnedLight.colorTemperature = 4500f; //TODO: Remove magic number. 
        }
        
        if (spawnedFireVisualEffect != null)
        {
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

    private void SpawnLight()
    {
        if (lightObjectPrefab != null)
        {
            Vector3 highSpawnPosition = transform.position + (transform.up * LightStartingHeight);
            spawnedLight = Instantiate(lightObjectPrefab, highSpawnPosition, Quaternion.identity);
            spawnedLight.gameObject.SetActive(false);
            originalLightColor = spawnedLight.color;
            originalLightColorTemperature = spawnedLight.colorTemperature;
        }
    }
    
    private void SpawnFire()
    {
        if (fireVisualEffect != null)
        {
            spawnedFireVisualEffect = Instantiate(fireVisualEffect, transform.position, Quaternion.identity);
            spawnedFireVisualEffect.gameObject.SetActive(false);
            spawnedFireVisualEffect.SetFloat(flameSizeAttribute, MaxFireSize);
            originalFireColor = spawnedFireVisualEffect.GetVector4(flameColorAttribute);
        }
    }

    private void SpawnFracture()
    {
        if (fracturedVersionPrefab != null && spawnedFracturedObject == null)
        {
            spawnedFracturedObject = Instantiate(fracturedVersionPrefab, transform.position, transform.rotation);
            spawnedFracturedObject.gameObject.SetActive(false);
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
