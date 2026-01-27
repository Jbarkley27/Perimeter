using UnityEngine;

[CreateAssetMenu(fileName = "SkillElementOverrideEffect", menuName = "Skills/Effects/Skill Element Override")]
public class SkillElementOverrideEffect : SkillEffect
{
    public SkillData targetSkill;
    public Element newElement;

    public override void Apply()
    {
        Debug.Log($"Applying SkillElementOverrideEffect: Setting {targetSkill.skillName} element to {newElement}");
        if (targetSkill != null)
        {
            Debug.Log($"Before Override: {targetSkill.skillName} element is {targetSkill.element}");
            targetSkill.element = newElement;
        }
            
    }
}
