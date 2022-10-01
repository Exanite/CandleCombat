using System;
using System.Collections;
using System.Collections.Generic;
using Project.Source;
using Project.Source.Characters;
using Unity.Mathematics;
using UnityEngine;

public class GunController : MonoBehaviour
{
    public int EquippedGunIndex = 0;
    [SerializeField] private List<Gun> equippableGuns = new List<Gun>();

    private Character character;
    private int currentEquippedGunIndex = 0;
    private Gun equippedGun;

    private void Update()
    {
        if (equippableGuns.Count == 0) return;
        
        SwitchGun(EquippedGunIndex);

        if (equippedGun == null || character == null) return;
        
        MoveGunToCharacterGunPoint(character);
    }
    
    public void Fire()
    {
        if (equippedGun == null) return;
        
        equippedGun.Fire(character);
    }

    public void SetCharacter(Character newCharacter)
    {
        character = newCharacter;
    }

    public void SwitchGun(int gunIndex)
    {
        if (equippedGun != null && gunIndex == currentEquippedGunIndex) return;
        
        for (int i = 0; i < equippableGuns.Count; i++)
        {
            if (gunIndex == i)
            {
                EquippedGunIndex = i;
                currentEquippedGunIndex = i;
                equippedGun = equippableGuns[i];
                equippedGun.gameObject.SetActive(true);
                
                if(character != null)
                    MoveGunToCharacterGunPoint(character);
                
                equippedGun.OnSwitch();
            }
            else
                equippableGuns[i].gameObject.SetActive(false); 
        }
    }

    private void MoveGunToCharacterGunPoint(Character character)
    {
        Transform eT = equippedGun.transform;
        Transform cgT = character.GunPosition.transform;
        eT.position = cgT.position;
        eT.rotation = cgT.rotation;
    }

    private void OnValidate()
    {
        EquippedGunIndex = Mathf.Clamp(EquippedGunIndex, 0, equippableGuns.Count - 1);
    }
}