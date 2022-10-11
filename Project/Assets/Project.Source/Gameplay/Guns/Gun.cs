using System;
using System.Collections.Generic;
using Project.Source.Gameplay.Characters;
using UniDi;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Project.Source.Gameplay.Guns
{
    [Serializable]
    public enum GunHoldType
    {
        OneHandGun,
        OneHandFan,
    }

    [Serializable]
    public enum SwitchReloadBehaviour
    {
        KeepPrevious = 0,
        SetToZeroAmmo,
        SetToFullAmmo,
    }

    public class Gun : MonoBehaviour
    {
        public Action BulletShot;
        public Action OnReload;

        [Header("Dependencies")]
        [SerializeField] private Transform model;
        [SerializeField] private Transform firePoint;
        [SerializeField] private Transform firePointVisual;
        [SerializeField] private List<Projectile.Projectile> projectilePrefabs = new List<Projectile.Projectile>();

        [Header("Sounds")]
        [SerializeField] private AudioClip fireAudioClip;
        [Range(0, 1)]
        [SerializeField] private float fireAudioScale;

        [Header("Settings")]
        public GunHoldType GunHoldType = GunHoldType.OneHandGun;
        public float TimeToHolsterGun = 4f;
        public SwitchReloadBehaviour SwitchReloadBehaviour;
        public int SelectedProjectile = 0;
        public int BurstCount = 1;
        public float SpreadAngle = 0f;
        public float BurstSpreadAngle = 0f;
        public float TimeBetweenShots = 1f;
        public int MaxAmmo = 1;
        public float ReloadTime = 2f;

        private float elapsedTimeSinceHolstered = 0f;
        private float elapsedTimeSinceShot = 0f;
        private float elapsedReloadTime = 0f;
        private int ammo = 0;
        private bool isReloading = false;

        [Inject]
        private AudioSource mainAudioSource;

        [Inject]
        private IInstantiator instantiator;

        private void Awake()
        {
            Reloaded();
        }

        private void Update()
        {
            elapsedTimeSinceShot += Time.deltaTime;
            elapsedTimeSinceShot = Mathf.Clamp(elapsedTimeSinceShot, 0, TimeBetweenShots);
            elapsedTimeSinceHolstered += Time.deltaTime;
            elapsedTimeSinceHolstered = Mathf.Clamp(elapsedTimeSinceHolstered, 0, TimeToHolsterGun);

            if (isReloading && elapsedReloadTime >= ReloadTime)
            {
                Reloaded();
            }
            else if (ammo == 0)
            {
                isReloading = true;
                elapsedReloadTime += Time.deltaTime;
            }
        }

        public void Fire(Character characterFrom)
        {
            if (elapsedTimeSinceShot < TimeBetweenShots || isReloading)
            {
                return;
            }

            if (ammo <= 0)
            {
                return;
            }

            ammo--;

            for (var i = 0; i < BurstCount; i++)
            {
                var direction = firePoint.forward;
                var angleOffsetDegrees = Random.Range(-SpreadAngle, SpreadAngle) / 2;
                if (BurstCount > 1)
                {
                    angleOffsetDegrees += -BurstSpreadAngle / 2f + i * (BurstSpreadAngle / BurstCount);
                }

                direction = Quaternion.AngleAxis(angleOffsetDegrees, Vector3.up) * direction;

                var projectile = instantiator.InstantiatePrefabForComponent<Projectile.Projectile>(projectilePrefabs[SelectedProjectile],
                    firePoint.position, Quaternion.Euler(direction), null);
                projectile.Fire(characterFrom, direction, firePointVisual.position);

                if (fireAudioClip != null)
                {
                    mainAudioSource.PlayOneShot(fireAudioClip, fireAudioScale);
                }
            }

            BulletShot?.Invoke();

            elapsedTimeSinceShot -= TimeBetweenShots;
            elapsedTimeSinceHolstered = 0;
        }

        public void SwitchAmmo(int index)
        {
            if (index >= 0 && index < projectilePrefabs.Count)
            {
                SelectedProjectile = index;
            }
        }

        public void OnSwitch()
        {
            switch (SwitchReloadBehaviour)
            {
                case SwitchReloadBehaviour.SetToFullAmmo:
                {
                    Reloaded();

                    break;
                }
                case SwitchReloadBehaviour.SetToZeroAmmo:
                {
                    ammo = 0;

                    break;
                }
                case SwitchReloadBehaviour.KeepPrevious:
                default:
                {
                    break;
                }
            }
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

        public int GetAmmo()
        {
            return ammo;
        }

        public void StartReload()
        {
            if (isReloading)
            {
                return;
            }

            ammo = 0;
            isReloading = true;
        }

        public bool IsReloading()
        {
            return isReloading;
        }

        private void Reloaded()
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
            MaxAmmo = Mathf.Clamp(MaxAmmo, BurstCount, 100); // Arbitrary max
        }
    }
}