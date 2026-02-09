using System;
using System.Collections.Generic;
using UnityEngine;

/*
 * SkillTreeLayoutDefinition
 * -------------------------
 * Defines UI positions for nodes in the skill tree.
 */
[CreateAssetMenu(fileName = "SkillTreeLayout", menuName = "Skills/Skill Tree Layout")]
public class SkillTreeLayoutDefinition : ScriptableObject
{
    public List<SkillTreeLayoutEntry> entries = new List<SkillTreeLayoutEntry>();

#if UNITY_EDITOR
    public static event Action<SkillTreeLayoutDefinition> LayoutChanged;

    private void OnValidate()
    {
        LayoutChanged?.Invoke(this);
    }
#endif
}

[System.Serializable]
public struct SkillTreeLayoutEntry
{
    public SkillNodeDefinition node;
    public Vector2 anchoredPosition;
}
