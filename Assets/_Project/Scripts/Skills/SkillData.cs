using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSkill", menuName = "Skills/Skill Data")]
public class SkillData : ScriptableObject
{
    [Header("General Info")]
    public string skillName = "New Skill";
    public Sprite icon;
    public Element element = Element.Kinetic;
    [TextArea(3, 10)]
    public string description = "Skill Description";
    public int cost = 1;
    public int currentLevel = 0;
    public int maxLevel = 5;
    public bool isPassive = false;




    [Header("Skill Classification")]
    public SkillType type = SkillType.Projectile;
    public bool isProjectile = true;




    [Header("Combat Stats")]
    public float cooldownRate = 1.5f;
    public float cooldownRestartDelay = 1f;
    public int damage = 10;



    [Header("Projectile Settings (If Projectile)")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;

    

    public bool IsProjectileSkill => isProjectile && projectilePrefab != null;
    public bool IsInstantSkill => !isProjectile;

    [Header("Skill Tree Info")]
    public bool isUnlocked = false;

    [Tooltip("0 = straight shot, 1+ = bigger arc curve")]
    public float arcHeight = 0f; 

    [Header("Projectile Accuracy")]
    [Tooltip("Degrees of inaccuracy. 0 = perfect, 60+ = very inaccurate")]
    public float accuracyAngle = 45f;    

    [Header("Cursor Type")]
    public WorldCursorMode cursorMode = WorldCursorMode.SINGLE_TARGET;



    [Header("Upgrade Effects")]
    public List<SkillUpgrade> upgrades = new List<SkillUpgrade>();

    [Header("Exclusive Grouping")]
    public string exclusiveGroupId; // empty = not exclusive
    public bool isExclusiveActive;  // runtime state

    [Header("Base Values (for rebuild)")]
    public int baseDamage;
    public float baseCooldownRate;
    public Element baseElement;
    public float baseAccuracyAngle;
    public float cooldownRestartDelayBase;

    public void ResetRuntimeStats()
    {
        damage = baseDamage;
        cooldownRate = baseCooldownRate;
        element = baseElement;
        accuracyAngle = baseAccuracyAngle;
        cooldownRestartDelay = cooldownRestartDelayBase;
    }

    public void ResetProgress()
    {
        isExclusiveActive = false;
        currentLevel = 0;
        isUnlocked = false;
    }
    
}


[System.Serializable]
public class SkillUpgrade
{
    public int level;
    public List<SkillEffect> effects;
}


