using System.Collections;
using System.Collections.Generic;
using Project.Source.Characters;
using UnityEngine;

public class GunPosition : MonoBehaviour
{
    public Transform GunPoint;
    
    [Header("Dependencies")]
    [SerializeField] private Transform animationPoint;
    [SerializeField] private Animator gunTypeAnimator;

    private static readonly int IsFiring = Animator.StringToHash("IsFiring");
    private static readonly int IsHolstered = Animator.StringToHash("IsHolstered");

    public void HandleHolster(GunHoldType gunHoldType)
    {
        gunTypeAnimator.SetBool(IsHolstered, true);
    }

    public void HandleDraw(GunHoldType gunHoldType)
    {
        gunTypeAnimator.SetBool(IsHolstered, false);
    }
    
    public void HandleFire(GunHoldType gunHoldType)
    {
        gunTypeAnimator.SetBool(IsFiring, true);
    }

    public void HandleStopFiring(GunHoldType gunHoldType)
    {
        gunTypeAnimator.SetBool(IsFiring, false);
    }

    public Vector3 GetAnimationPosition()
    {
        return animationPoint.position;
    }

    public Quaternion GetAnimationRotation()
    {
        return animationPoint.rotation;
    }
}
