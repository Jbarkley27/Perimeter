using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class SkillCaster : MonoBehaviour
{
    [Header("References")]
    public Transform firePoint;       
    public LayerMask enemyMask;
    public List<SkillUISlot> allSkillSlots = new List<SkillUISlot>();       
    public Transform skillSlotParent;

    [Header("Settings")]
    public float targetSearchRadius = 20f;
    public Transform ClosestTarget;


    [Header("Active Manual Skill")]
    public SkillUISlot activeManualSkillSlot;
    public bool assigningManualSkill = false;


    void Start()
    {
        // Get all children skill slots
        allSkillSlots = skillSlotParent.GetComponentsInChildren<SkillUISlot>().ToList();
    }


    void Update()
    {
        // For testing: constantly update closest target
        ClosestTarget = GetClosestEnemy();
    }



    public void AssignActiveManualSkill(SkillUISlot slot)
    {
        // Disable all other skill ui slots from manual
        // this way only one can be active at a time
        // and blocks against weird states
        if (assigningManualSkill) return;
        assigningManualSkill = true;

        // Clear previous
        if (activeManualSkillSlot)
            activeManualSkillSlot.SetFireMode(true);



        activeManualSkillSlot = slot;
        slot.SetFireMode(false);

        // Tell World Cursor to switch modes
        WorldCursor.instance.SwitchCursorMode(slot.currentSkill.cursorMode);


        // Done
        assigningManualSkill = false;
    }



    public void ClearActiveManualSkill()
    {
        if (activeManualSkillSlot)
            activeManualSkillSlot.SetFireMode(true);
        activeManualSkillSlot = null;
        WorldCursor.instance.SwitchCursorMode(WorldCursorMode.DEFAULT);
    }



    public void UseActiveManualSkill()
    {
        if (activeManualSkillSlot == null
        || !activeManualSkillSlot.IsReadyToFire()
        || GameManager.Instance.GamePaused)
        {
            Debug.Log("[SkillCaster] No active manual skill to use.");
            return;
        }

        Debug.Log("[SkillCaster] Using active manual skill.");
        StartCoroutine(activeManualSkillSlot.TriggerSkill(true));
    }
    




    public SkillData GetActiveManualSkillData()
    {
        return activeManualSkillSlot ? activeManualSkillSlot.currentSkill : null;
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



    public void ResetAllSkillCooldowns()
    {
        foreach (var slot in allSkillSlots)
        {
            if (slot) slot.ForceCooldownReset();
        }
    }
}
