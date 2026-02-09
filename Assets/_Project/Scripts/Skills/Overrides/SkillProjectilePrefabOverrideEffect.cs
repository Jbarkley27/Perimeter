using UnityEngine;

[CreateAssetMenu(
    fileName = "SkillProjectilePrefabOverrideEffect",
    menuName = "Skills/Effects/Skill Projectile Prefab Override")]
public class SkillProjectilePrefabOverrideEffect : SkillEffect
{
    public SkillData targetSkill;
    public GameObject newProjectilePrefab;

    public override void Apply()
    {
        if (targetSkill == null || newProjectilePrefab == null)
            return;

        targetSkill.projectilePrefab = newProjectilePrefab;
    }

    public override string GetDescription()
    {
        if (targetSkill == null || newProjectilePrefab == null)
            return "Override projectile visual";

        return $"{targetSkill.skillName}: Projectile -> {newProjectilePrefab.name}";
    }
}
