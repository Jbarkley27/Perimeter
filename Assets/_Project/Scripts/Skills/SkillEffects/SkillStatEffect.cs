using UnityEngine;

public enum SkillStatType
{
    Damage,
    CooldownRate,
    AccuracyAngle,
    ProjectileSpeed,
    CooldownRestartDelay
}

public enum SkillStatOp
{
    Add,
    Multiply,
    Override
}

[CreateAssetMenu(fileName = "SkillStatEffect", menuName = "Skills/Effects/Skill Stat")]
public class SkillStatEffect : SkillEffect
{
    public SkillData targetSkill;
    public SkillStatType stat;
    public SkillStatOp op = SkillStatOp.Add;
    public float value;

    public override void Apply()
    {
        if (targetSkill == null) return;

        float current = GetValue();
        float next = current;

        switch (op)
        {
            case SkillStatOp.Add:
                next = current + value;
                break;
            case SkillStatOp.Multiply:
                next = current * value; // e.g. 0.9f = 10% reduction
                break;
            case SkillStatOp.Override:
                next = value;
                break;
        }

        SetValue(next);
    }

    public override string GetDescription()
    {
        return $"{op} {value} {stat}";
    }

    private float GetValue()
    {
        return stat switch
        {
            SkillStatType.Damage => targetSkill.damage,
            SkillStatType.CooldownRate => targetSkill.cooldownRate,
            SkillStatType.AccuracyAngle => targetSkill.accuracyAngle,
            SkillStatType.ProjectileSpeed => targetSkill.projectileSpeed,
            SkillStatType.CooldownRestartDelay => targetSkill.cooldownRestartDelay,
            _ => 0f
        };
    }

    private void SetValue(float valueToSet)
    {
        switch (stat)
        {
            case SkillStatType.Damage:
                targetSkill.damage = Mathf.RoundToInt(valueToSet);
                break;
            case SkillStatType.CooldownRate:
                targetSkill.cooldownRate = valueToSet;
                break;
            case SkillStatType.AccuracyAngle:
                targetSkill.accuracyAngle = valueToSet;
                break;
            case SkillStatType.ProjectileSpeed:
                targetSkill.projectileSpeed = valueToSet;
                break;
            case SkillStatType.CooldownRestartDelay:
                targetSkill.cooldownRestartDelay = valueToSet;
                break;
        }
    }
}
