using UnityEngine;

[CreateAssetMenu(
    fileName = "SkillDescriptionOverrideEffect",
    menuName = "Skills/Effects/Skill Description Override")]
public class SkillDescriptionOverrideEffect : SkillEffect
{
    public SkillData targetSkill;
    [TextArea(3, 10)] public string newDescription;

    public override void Apply()
    {
        if (targetSkill == null)
            return;

        targetSkill.description = newDescription;
    }

    public override string GetDescription()
    {
        if (targetSkill == null)
            return "Override skill description";

        return $"{targetSkill.skillName}: Description override";
    }
}
