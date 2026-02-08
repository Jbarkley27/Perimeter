using System.Collections.Generic;
using UnityEngine;

/*
 * SkillTreeDefinition
 * -------------------
 * Asset that defines the skill tree nodes and shared tuning.
 */
[CreateAssetMenu(fileName = "SkillTreeDefinition", menuName = "Skills/Skill Tree Definition")]
public class SkillTreeDefinition : ScriptableObject
{
    public List<SkillNodeDefinition> nodes = new List<SkillNodeDefinition>();

    [Header("Global Cost Tuning (Linear)")]
    public float baseCostMultiplier = 1f;
    public float perLevelCostMultiplier = 1f;
}
