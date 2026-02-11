using System.Collections.Generic;
using UnityEngine;

/*
 * SkillNodeDefinition
 * -------------------
 * Data for one node in the skill tree.
 */
[CreateAssetMenu(fileName = "SkillNodeDefinition", menuName = "Skills/Skill Node Definition")]
public class SkillNodeDefinition : ScriptableObject
{
    public string nodeId;
    public string displayName;
    [TextArea(3, 8)] public string description;

    public SkillNodeType nodeType = SkillNodeType.Passive;

    [Header("Skill Reference (Optional)")]
    public SkillData skill; // used by Active nodes or exclusive modifiers

    [Header("Element")]
    public Element element = Element.Kinetic;

    [Header("Leveling")]
    public int maxLevel = 5;

    [Header("Cost (Linear)")]
    public int baseCost = 1;
    public int costPerLevel = 1;

    [Header("Prerequisites (AND)")]
    public List<SkillNodePrereq> prerequisites = new List<SkillNodePrereq>();

    [Header("Exclusive Group")]
    public string exclusiveGroupId;

    [Header("Effects Per Level")]
    public List<SkillNodeLevelEffects> levelEffects = new List<SkillNodeLevelEffects>();

    public bool IsExclusive => !string.IsNullOrEmpty(exclusiveGroupId);
    public bool IsActive => nodeType == SkillNodeType.Active;
    public bool IsPassive => nodeType == SkillNodeType.Passive;


    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(nodeId))
            nodeId = System.Guid.NewGuid().ToString();
    }


    [ContextMenu("Regenerate Node ID")]
    private void RegenerateId()
    {
        nodeId = System.Guid.NewGuid().ToString();
    }


}

public enum SkillNodeType
{
    Passive,
    Active,
    Exclusive
}

[System.Serializable]
public struct SkillNodePrereq
{
    public SkillNodeDefinition node;
    public int requiredLevel;
}

[System.Serializable]
public struct SkillNodeLevelEffects
{
    public int level;
    public List<SkillEffect> effects;
}
