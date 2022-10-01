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
    [SerializeField] private InputActionReference pointerReference;
    [SerializeField] private Transform startObject;
    [SerializeField] private Transform endObject;

    [Header("Settings")]
    [SerializeField] private float groundHeight = 1f;
    [SerializeField] private float maxRadius = 5f;

    private Plane plane;

    private void Start()
    {
        plane = new Plane();
        plane.SetNormalAndPosition(Vector3.up, new Vector3(0, groundHeight, 0));
    }

    private void Update()
    {
        Vector3 endPosition = GetPointerWorldPosition();
        endObject.position = ClampEndPosition(startObject.position, endPosition, maxRadius);
    }

    private Vector3 GetPointerWorldPosition()
    {
        if (mainCamera == null || pointerReference == null)
        {
            Debug.Log("Main camera or pointer reference is not set");
            return transform.position;
        }
        
        Vector3 pointerPosition = pointerReference.action.ReadValue<Vector2>();
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
