using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

public class UnitAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Transform bulletProjectilePrefab, shootPointTransform, rifleTransform, knifeTransform;

    private void Awake()
    {
        if(TryGetComponent(out MoveAction moveAction))
        {
            moveAction.OnStartMoving += MoveAction_OnStartMoving;
            moveAction.OnStopMoving += MoveAction_OnStopMoving;
        }
        
        if(TryGetComponent(out ShootAction shootAction))
        {
            shootAction.OnShoot += ShootAction_OnShoot;
        }
        
        if(TryGetComponent(out OverwatchAction overwatchAction))
        {
            overwatchAction.OnShoot += OverwatchAction_OnShoot;
        }
        
        if(TryGetComponent(out MeleeAction meleeAction))
        {
            meleeAction.OnMeleeActionStarted += MeleeActionOnMeleeActionStarted;
            meleeAction.OnMeleeActionCompleted += MeleeActionOnMeleeActionCompleted;
        }
    }

    private void Start()
    {
        EquipRifle();
    }

    private void MeleeActionOnMeleeActionStarted(object sender, EventArgs e)
    {
        EquipKnife();
        animator.SetTrigger("Melee");
    }
    private void MeleeActionOnMeleeActionCompleted(object sender, EventArgs e)
    {
        EquipRifle();
    }

    private void MoveAction_OnStartMoving(object sender, EventArgs e)
    {
        animator.SetBool("IsWalking", true);
    }
    
    private void MoveAction_OnStopMoving(object sender, EventArgs e)
    {
        animator.SetBool("IsWalking", false);
    }

    private void ShootAction_OnShoot(object sender, ShootAction.OnShootEventArgs e)
    {
        animator.SetTrigger("Shoot");

        Transform bulletProjectileTransform = Instantiate(bulletProjectilePrefab, shootPointTransform.position, Quaternion.identity);
        BulletProjectile bulletProjectile = bulletProjectileTransform.GetComponent<BulletProjectile>();

        Vector3 targetUnitShootAtPosition = e.targetUnit.GetWorldPosition();
        targetUnitShootAtPosition.y = shootPointTransform.position.y;

        if (!e.hit)
        {
            Vector3 missShotPosition = targetUnitShootAtPosition;
            Vector3 directionToMissShotPosition = (missShotPosition - shootPointTransform.position).normalized;
            Vector3 missDirection = Vector3.Cross(directionToMissShotPosition, Vector3.down);

            missShotPosition += missDirection * 0.25f;
            directionToMissShotPosition = (missShotPosition - shootPointTransform.position).normalized;

            targetUnitShootAtPosition = missShotPosition + directionToMissShotPosition * 40f;
        }
        
        bulletProjectile.Setup(targetUnitShootAtPosition);
    }
    
    private void OverwatchAction_OnShoot(object sender, OverwatchAction.OnOverwatchEventArgs e)
    {
        animator.SetTrigger("Shoot");

        Transform bulletProjectileTransform = Instantiate(bulletProjectilePrefab, shootPointTransform.position, Quaternion.identity);
        BulletProjectile bulletProjectile = bulletProjectileTransform.GetComponent<BulletProjectile>();

        Vector3 targetUnitShootAtPosition = e.targetUnit.GetWorldPosition();
        targetUnitShootAtPosition.y = shootPointTransform.position.y;

        if (!e.hit)
        {
            Vector3 missShotPosition = targetUnitShootAtPosition;
            Vector3 directionToMissShotPosition = (missShotPosition - shootPointTransform.position).normalized;
            Vector3 missDirection = Vector3.Cross(directionToMissShotPosition, Vector3.down);

            missShotPosition += missDirection * 0.25f;
            directionToMissShotPosition = (missShotPosition - shootPointTransform.position).normalized;

            targetUnitShootAtPosition = missShotPosition + directionToMissShotPosition * 40f;
        }
        
        bulletProjectile.Setup(targetUnitShootAtPosition);
    }

    private void EquipKnife()
    {
        knifeTransform.gameObject.SetActive(true);
        rifleTransform.gameObject.SetActive(false);
    }

    private void EquipRifle()
    {
        rifleTransform.gameObject.SetActive(true);
        knifeTransform.gameObject.SetActive(false);
    }

    public void PauseAnimation()
    {
        animator.speed = 0;
    }
    
    public void ResumeAnimation()
    {
        animator.speed = 1;
    }
}
