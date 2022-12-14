using System;
using System.Collections.Generic;
using Project.Source.Gameplay.Characters;
using UniDi;
using UnityEngine;
using UnityEngine.Serialization;
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

    [Serializable]
    public enum GunState
    {
        Ready = 0,
        Reloading,
        Switching,
    }

    public class Gun : MonoBehaviour
    {
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

        [Space]
        public int SelectedProjectile = 0;
        public int MaxAmmo = 1;
        public float TimeBetweenShots = 1f;
        public int BurstCount = 1;

        [Space]
        public float SpreadAngle = 0f;
        public float BurstSpreadAngle = 0f;

        [Space]
        public SwitchReloadBehaviour SwitchReloadBehaviour;
        public float SwitchDuration = 0f;
        
        [Space]
        [FormerlySerializedAs("ReloadTime")]
        public float ReloadDuration = 2f;
        public bool CanReload = true;

        private float elapsedTimeSinceHolstered;
        private float elapsedTimeSinceShot;
        private float elapsedReloadTime;
        private float elapsedTimeSinceSwitch = float.PositiveInfinity;

        [Inject] private AudioSource mainAudioSource;
        [Inject] private IInstantiator instantiator;

        public int AmmoCount { get; private set; }

        public GunState State { get; private set; } = GunState.Ready;
        public bool IsFiring => elapsedTimeSinceShot < TimeBetweenShots;
        public bool IsHolstered => elapsedTimeSinceHolstered >= TimeToHolsterGun;

        public Transform ModelTransform => model;

        public Action BulletShot;
        public Action Reloaded;

        private void Awake()
        {
            OnReloaded();
        }

        private void Update()
        {
            elapsedTimeSinceShot += Time.deltaTime;
            elapsedTimeSinceShot = Mathf.Clamp(elapsedTimeSinceShot, 0, TimeBetweenShots);
            elapsedTimeSinceHolstered += Time.deltaTime;
            elapsedTimeSinceHolstered = Mathf.Clamp(elapsedTimeSinceHolstered, 0, TimeToHolsterGun);

            switch (State)
            {
                case GunState.Reloading:
                {
                    elapsedReloadTime += Time.deltaTime;

                    if (elapsedReloadTime >= ReloadDuration)
                    {
                        OnReloaded();
                    }

                    break;
                }
                case GunState.Switching:
                {
                    elapsedTimeSinceSwitch += Time.deltaTime;

                    if (elapsedTimeSinceSwitch > SwitchDuration)
                    {
                       OnSwitched();
                    }

                    break;
                }
                case GunState.Ready:
                default:
                {
                    if (AmmoCount == 0)
                    {
                        StartReload();
                    }

                    break;
                }
            }
        }

        public void Fire(Character characterFrom)
        {
            var canFire = !(
                elapsedTimeSinceShot < TimeBetweenShots
                || State != GunState.Ready
                || AmmoCount <= 0
            );

            if (!canFire)
            {
                return;
            }

            AmmoCount--;

            for (var i = 0; i < BurstCount; i++)
            {
                var direction = Vector3.Cross(firePoint.right, Vector3.up);
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

        public void OnSwitching()
        {
            if (SwitchDuration > float.Epsilon)
            {
                State = GunState.Switching;
                elapsedTimeSinceSwitch = 0;
            }
            else
            {
                OnSwitched();
            }
        }

        public void StartReload()
        {
            if (State == GunState.Reloading || !CanReload)
            {
                return;
            }

            State = GunState.Reloading;
            elapsedReloadTime = 0;
        }

        public void ForceReload()
        {
            OnReloaded();
        }

        private void OnSwitched()
        {
            switch (SwitchReloadBehaviour)
            {
                case SwitchReloadBehaviour.SetToFullAmmo:
                {
                    OnReloaded();

                    break;
                }
                case SwitchReloadBehaviour.SetToZeroAmmo:
                {
                    AmmoCount = 0;

                    break;
                }
                case SwitchReloadBehaviour.KeepPrevious:
                default:
                {
                    break;
                }
            }

            State = GunState.Ready;
        }

        private void OnReloaded()
        {
            Reloaded?.Invoke();

            AmmoCount = MaxAmmo;
            State = GunState.Ready;
            elapsedTimeSinceShot = TimeBetweenShots;
            elapsedTimeSinceHolstered = TimeToHolsterGun;
        }

        private void OnValidate()
        {
            SelectedProjectile = Mathf.Clamp(SelectedProjectile, 0, projectilePrefabs.Count - 1);
            MaxAmmo = Mathf.Clamp(MaxAmmo, 0, int.MaxValue);
        }
    }
}