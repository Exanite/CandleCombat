using System.Collections;
using Project.Source.Gameplay.Characters;
using UniDi;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.VFX;

namespace Project.Source.Gameplay.Abilities
{
    public interface IBurn
    {
        public void AddFire(int fireDps);
        public void RemoveFire(int fireDpsRemoved);
    }

    public class BurnableObject : MonoBehaviour, IBurn
    {
        [Header("Dependencies")]
        [SerializeField] private Light lightObjectPrefab;
        [FormerlySerializedAs("fireVisualEffect")]
        [SerializeField] private VisualEffect fireVisualEffectPrefab;

        [Header("Optional")]
        [SerializeField] private GameObject fracturedVersionPrefab;
        [Tooltip("Audio is only enabled and disabled.")]
        private AudioSource audioSource;

        [Header("Settings")]
        public int MaxHealth = 100;

        [SerializeField] private string flameSizeAttribute = "FlamesSize";
        [SerializeField] private string flameColorAttribute = "Color";

        [Header("Light Settings")]
        [ColorUsage(true, true)] public Color PossessColor = new Vector4(0, 4.7311759f, 22.0403557f, 1);
        [ColorUsage(true, true)] public Color PossessLightColor = new Vector4(0, 4.7311759f, 22.0403557f, 1);
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

        private float CurrentHealth = 0;
        private int currentFireDps = 0;
        private bool isOnFire = false;
        private bool isFractured = false;

        private Color originalLightColor = Color.white;
        private float originalLightColorTemperature;
        private Color originalFireColor = Color.white;

        private Light spawnedLight;
        private VisualEffect fireVisualEffect;
        private GameObject spawnedFracturedObject;
        private Coroutine flashPossessedColorCoroutine;

        [InjectOptional]
        private GameContext gameContext;

        [Inject]
        private IInstantiator instantiator;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
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
            if (gameContext)
            {
                gameContext.Possessed += HandlePossessed;
            }
        }

        private void OnDisable()
        {
            if (gameContext)
            {
                gameContext.Possessed -= HandlePossessed;
            }
        }

        private void Update()
        {
            if (!isOnFire)
            {
                return;
            }

            CurrentHealth -= currentFireDps * Time.deltaTime;

            if (spawnedLight != null)
            {
                TickLightIntensity();
            }

            if (CurrentHealth <= MaxHealth && !isFractured)
            {
                Fracture();
            }

            if (CurrentHealth <= 0)
            {
                Expire();
            }
        }

        public void AddFire(int fireDps)
        {
            if (isOnFire || fireDps < currentFireDps)
            {
                return;
            }

            currentFireDps = fireDps;
            isOnFire = true;

            spawnedLight.gameObject.SetActive(true);
            fireVisualEffect.gameObject.SetActive(true);
            audioSource.enabled = true;
        }

        public void RemoveFire(int fireDpsRemoved)
        {
            isOnFire = false;

            currentFireDps -= fireDpsRemoved;

            if (currentFireDps <= 0)
            {
                currentFireDps = 0;
                spawnedLight.gameObject.SetActive(false);
                fireVisualEffectPrefab.gameObject.SetActive(false);
                audioSource.enabled = false;
            }
        }

        private void TickLightIntensity()
        {
            var healthPercent = 1 - CurrentHealth / MaxHealth;
            var intensity = healthToIntensityCurve.Evaluate(healthPercent) * MaxIntensity;
            intensity = Mathf.Clamp(intensity, MinIntensity, MaxIntensity);

            spawnedLight.intensity = intensity;

            var size = healthToIntensityCurve.Evaluate(healthPercent) * MaxFireSize;
            size = Mathf.Clamp(size, MinFireSize, MaxFireSize);

            if (fireVisualEffectPrefab != null)
            {
                fireVisualEffectPrefab.SetFloat(flameSizeAttribute, size);
            }
        }

        private void Fracture()
        {
            isFractured = true;

            if (fracturedVersionPrefab != null && spawnedFracturedObject == null)
            {
                spawnedFracturedObject = instantiator.InstantiatePrefab(fracturedVersionPrefab, transform.position, transform.rotation, null);
                meshRenderer.enabled = false;
            }
        }

        private void HandlePossessed(Character character)
        { 
            StopFlashingPossessedColor();
            flashPossessedColorCoroutine = StartCoroutine(FlashPossessedColor(PossessSwitchTime));
        }

        private IEnumerator FlashPossessedColor(float duration)
        {
            float elapsedTime = 0;

            if (spawnedLight != null)
            {
                spawnedLight.color = PossessLightColor;
                spawnedLight.colorTemperature = 4500f; //TODO: Remove magic number. 
            }

            if (fireVisualEffect != null)
            {
                fireVisualEffect.SetVector4(flameColorAttribute, PossessColor);
            }

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            StopFlashingPossessedColor();
        }

        private void StopFlashingPossessedColor()
        {
            if (spawnedLight != null)
            {
                spawnedLight.color = originalLightColor;
                spawnedLight.colorTemperature = originalLightColorTemperature;
            }

            if (fireVisualEffect != null)
            {
                fireVisualEffect.SetVector4(flameColorAttribute, originalFireColor);
            }
        }
        
        private void SpawnLight()
        {
            if (lightObjectPrefab != null)
            {
                var highSpawnPosition = transform.position + transform.up * LightStartingHeight;
                spawnedLight = instantiator.InstantiatePrefabForComponent<Light>(lightObjectPrefab, highSpawnPosition, Quaternion.identity, null);
                spawnedLight.gameObject.SetActive(false);
                originalLightColor = spawnedLight.color;
                originalLightColorTemperature = spawnedLight.colorTemperature;
            }
        }

        private void SpawnFire()
        {
            if (fireVisualEffectPrefab == null)
            {
                return;
            }

            fireVisualEffect =
                instantiator.InstantiatePrefabForComponent<VisualEffect>(fireVisualEffectPrefab, transform.position, Quaternion.identity, null);
            fireVisualEffect.gameObject.SetActive(false);
            fireVisualEffect.SetFloat(flameSizeAttribute, MaxFireSize);
            originalFireColor = fireVisualEffect.GetVector4(flameColorAttribute);
        }

        private void SpawnFracture()
        {
            if (fracturedVersionPrefab != null && spawnedFracturedObject == null)
            {
                spawnedFracturedObject =
                    instantiator.InstantiatePrefab(fracturedVersionPrefab, transform.position, transform.rotation, null);
                spawnedFracturedObject.gameObject.SetActive(false);
            }
        }

        private void Expire()
        {
            if(flashPossessedColorCoroutine != null)
                StopCoroutine(flashPossessedColorCoroutine);
            
            if (spawnedLight != null)
            {
                Destroy(spawnedLight.gameObject);
            }

            if (spawnedFracturedObject != null)
            {
                Destroy(spawnedFracturedObject.gameObject);
            }

            if (fireVisualEffect != null)
            {
                Destroy(fireVisualEffect.gameObject);
            }
            
            gameContext.Possessed -= HandlePossessed;

            Destroy(gameObject);
        }
    }
}