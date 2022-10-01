using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Project.Source;
using Project.Source.Characters;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
{
    public Transform LookAt => targetGroup.transform;

    //Using separate CinemachineTargetGroup.
    [Header("Dependencies")]
    [SerializeField] private CinemachineTargetGroup targetGroupPrefab;
    [SerializeField] private Transform endObjectPrefab;

    [Header("Settings")]
    [SerializeField] private float groundHeight = 1f;
    [SerializeField] private float maxRadius = 5f;
    [Range(0,1)]
    [SerializeField] private float playerWeight = 1f;
    [Range(0,1)]
    [SerializeField] private float pointerWeight = 0.2f;
    
    private CinemachineTargetGroup targetGroup;
    private Transform endObject;
    
    private Plane plane;
    private Character character;
    private Camera mainCamera;
    private Vector3 pointerPosition;

    private void Awake()
    {
        targetGroup = Instantiate(targetGroupPrefab);
        endObject = Instantiate(endObjectPrefab);
        endObject.position = Vector3.zero;
    }

    private void Start()
    {
        plane = new Plane();
        plane.SetNormalAndPosition(Vector3.up, new Vector3(0, groundHeight, 0));
        
        targetGroup.AddMember(endObject, pointerWeight, 0);
    }

    private void Update()
    {
        if (character == null) return;
        
        Vector3 endPosition = PointerToWorldPosition(pointerPosition);
        endObject.position = ClampEndPosition(character.transform.position, endPosition, maxRadius);

        Transform cT = character.transform;

        Quaternion rotationTowardsEnd = Quaternion.LookRotation(GetXZDirection(cT.position, endObject.position));
        cT.rotation = rotationTowardsEnd;
        //if (character.GunPosition != null)
        //    character.GunPosition.rotation = rotationTowardsEnd;
    }
    
    public void SetCharacter(Character newCharacter)
    {
        if (character != null)
        {
            if (targetGroup.FindMember(character.transform) > -1)
                targetGroup.RemoveMember(character.transform);
            
            targetGroup.AddMember(newCharacter.transform, playerWeight, 0);
        }

        character = newCharacter;
    }

    public void SetPointerPosition(Vector3 pointerPos)
    {
        pointerPosition = pointerPos;
    }

    public void SetCamera(Camera camera)
    {
        mainCamera = camera;
    }

    private Vector3 PointerToWorldPosition(Vector3 pointerPosition)
    {
        if (mainCamera == null)
        {
            Debug.Log("Main camera or pointer reference is not set");
            return transform.position;
        }
        
        Ray ray = mainCamera.ScreenPointToRay(pointerPosition);
        float enter = 0;
        bool success = plane.Raycast(ray, out enter);
        Vector3 worldPosition = ray.GetPoint(enter);
        return worldPosition;
    }

    private Vector3 ClampEndPosition(Vector3 startPosition, Vector3 endPosition, float maxRadius)
    {
        float distance = Vector3.Distance(startPosition, endPosition);
        Vector3 direction = (endPosition - startPosition).normalized;
        
        if (distance > maxRadius)
            return startPosition + (direction * maxRadius);
        
        return endPosition;
    }

    private Vector3 GetXZDirection(Vector3 startPosition, Vector3 endPosition)
    {
        Vector3 direction = (endPosition - startPosition).normalized;
        return new Vector3(direction.x, 0, direction.z).normalized;
    }
}
