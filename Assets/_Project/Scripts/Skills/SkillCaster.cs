using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SkillCaster : MonoBehaviour
{
    [Header("References")]
    public Transform firePoint;       
    public LayerMask enemyMask;       

    [Header("Settings")]
    public float targetSearchRadius = 20f;
    public Transform ClosestTarget;



    void Update()
    {
        // For testing: constantly update closest target
        ClosestTarget = GetClosestEnemy();
    }



    public bool HasTarget()
    {
        return ClosestTarget != null;
    }





    public Transform GetClosestEnemy()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, targetSearchRadius, enemyMask);
        if (hits.Length == 0) return null;

        return hits
            .OrderBy(h => Vector3.Distance(transform.position, h.transform.position))
            .First()
            .transform;
    }








    public void FireProjectile(SkillData skill, Transform target)
    {
        SkillData data = skill;

        if (!data.isProjectile || data.projectilePrefab == null)
        {
            Debug.LogWarning($"[SkillCaster] Tried to fire non-projectile skill: {data.skillName}");
            return;
        }

        Vector3 spawnPos = firePoint ? firePoint.position : transform.position;
        GameObject proj = Instantiate(data.projectilePrefab, spawnPos, Quaternion.identity);

        // LAUNCH WITH ARC + HOMING + SPEED SETTINGS
        proj.GetComponent<Projectile>().Launch(
            target,
            data.damage,
            data.projectileSpeed,
            data.accuracyAngle
        );



        if (target) Debug.Log($"[SkillCaster] Fired {data.skillName} → {target.name}");
    }
}
