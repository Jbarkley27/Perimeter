using UnityEngine;

[CreateAssetMenu(fileName = "SkillElementOverrideEffect", menuName = "Skills/Effects/Skill Element Override")]
public class SkillElementOverrideEffect : SkillEffect
{
    public SkillData targetSkill;
    public Element newElement;

    public override void Apply()
    {
        if (targetSkill != null)
            targetSkill.element = newElement;
    }
}
