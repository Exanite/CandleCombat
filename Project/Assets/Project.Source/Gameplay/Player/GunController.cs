using System.Collections.Generic;
using Project.Source.Gameplay.Characters;
using Project.Source.Gameplay.Guns;
using UnityEngine;

namespace Project.Source.Gameplay.Player
{
    public class GunController : MonoBehaviour
    {
        [Header("Settings")]
        public bool IsOwnedByPlayer;
        public int EquippedGunIndex;
        [SerializeField]
        private List<Gun> equippableGuns = new List<Gun>();

        private Character character;
        private int currentEquippedGunIndex;
        private Gun equippedGun;

        public GunState GunState => equippedGun ? equippedGun.State : GunState.Ready;

        private void Start()
        {
            SwitchGun(EquippedGunIndex);
        }

        private void Update()
        {
            if (equippableGuns.Count == 0)
            {
                return;
            }

            if (equippedGun == null || character == null)
            {
                return;
            }

            if (character.IsDead)
            {
                Cleanup();

                return;
            }

            MoveGunToCharacterGunPoint(character);

            if (equippedGun.IsHolstered)
            {
                character.GunPosition.HandleHolster(equippedGun.GunHoldType);
            }
            else
            {
                character.GunPosition.HandleDraw(equippedGun.GunHoldType);
            }

            if (equippedGun.IsFiring)
            {
                character.GunPosition.HandleFire(equippedGun.GunHoldType);
            }
            else
            {
                character.GunPosition.HandleStopFiring(equippedGun.GunHoldType);
            }
        }

        public void Fire()
        {
            if (equippedGun == null)
            {
                return;
            }

            equippedGun.Fire(character);
        }

        public void SetCharacter(Character newCharacter)
        {
            character = newCharacter;
        }

        public void SwitchGun(int gunIndex)
        {
            if (equippedGun != null && gunIndex == currentEquippedGunIndex)
            {
                return;
            }

            for (var i = 0; i < equippableGuns.Count; i++)
            {
                if (gunIndex == i)
                {
                    EquippedGunIndex = i;
                    currentEquippedGunIndex = i;
                    equippedGun = equippableGuns[i];
                    equippedGun.gameObject.SetActive(true);

                    if (character != null)
                    {
                        MoveGunToCharacterGunPoint(character);
                    }

                    equippedGun.OnSwitching();
                }
                else
                {
                    equippableGuns[i].gameObject.SetActive(false);
                }
            }
        }

        public void SwitchAmmo(int ammoIndex)
        {
            if (equippedGun == null)
            {
                return;
            }

            equippedGun.SwitchAmmo(ammoIndex);
        }

        public int GetCurrentAmmo()
        {
            if (equippedGun == null)
            {
                return 0;
            }

            return equippedGun.AmmoCount;
        }

        public int GetMaxAmmo()
        {
            if (equippedGun == null)
            {
                return 0;
            }

            return equippedGun.MaxAmmo;
        }

        public void ReloadEquippedGun()
        {
            if (equippedGun == null)
            {
                return;
            }

            equippedGun.StartReload();
        }

        public Gun GetEquippedGun()
        {
            return equippedGun;
        }

        private void MoveGunToCharacterGunPoint(Character character)
        {
            var gunTransform = equippedGun.transform;
            var modelTransform = equippedGun.ModelTransform;
            var characterGun = character.GunPosition;
            
            gunTransform.position = characterGun.transform.position;
            gunTransform.rotation = characterGun.transform.rotation;
            
            modelTransform.position = characterGun.GetAnimationPosition();
            modelTransform.rotation = characterGun.GetAnimationRotation();
        }

        public void Cleanup()
        {
            foreach (var gun in equippableGuns)
            {
                gun.gameObject.SetActive(false);
            }
        }

        private void OnValidate()
        {
            EquippedGunIndex = Mathf.Clamp(EquippedGunIndex, 0, equippableGuns.Count - 1);
        }
    }
}