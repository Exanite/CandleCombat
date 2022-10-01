using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
{
    //Using separate CinemachineTargetGroup.
    [Header("Dependencies")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform startObject;
    [SerializeField] private Transform endObject;

    [Header("Settings")]
    [SerializeField] private float groundHeight = 1f;
    [SerializeField] private float maxRadius = 5f;

    private Plane plane;
    private Vector3 pointerPosition;

    private void Start()
    {
        plane = new Plane();
        plane.SetNormalAndPosition(Vector3.up, new Vector3(0, groundHeight, 0));
    }

    private void Update()
    {
        Vector3 endPosition = PointerToWorldPosition(pointerPosition);
        endObject.position = ClampEndPosition(startObject.position, endPosition, maxRadius);
    }

    public void SetPointerPosition(Vector3 pointerPos)
    {
        pointerPosition = pointerPos;
    }

    private Vector3 PointerToWorldPosition(Vector3 pointerPositio)
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
}
