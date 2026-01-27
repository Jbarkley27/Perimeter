using System.Collections.Generic;
using UnityEngine;

// Singleton class to manage the skill tree data, non-UI related

public class SkillTreeData : MonoBehaviour
{
    public static SkillTreeData Instance;
    public TreeNode rootNode;
    public List<TreeNode> allNodes = new List<TreeNode>();
    public List<SkillData> allSkills = new List<SkillData>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        InitializeTree();
    }

    public void AddToAllNodes(TreeNode node)
    {
        if (node != null && !allNodes.Contains(node))
        {
            allNodes.Add(node);
        }
    }

    public void InitializeTree()
    {
        RefreshAllNodes();

        if (rootNode != null)
        {
            rootNode.InitializeNode();
        }

        // get all skillData from nodes
        allSkills.Clear();

        foreach (var node in allNodes)
        {
            if (node.skillData != null && !allSkills.Contains(node.skillData))
            {
                allSkills.Add(node.skillData);
            }
        }

        RebuildAll();
    }


    public void SetExclusiveState(SkillData selected)
    {
        if (selected == null || string.IsNullOrEmpty(selected.exclusiveGroupId))
            return;

        foreach (var node in allNodes)
        {
            if (node == null || node.skillData == null)
                continue;

            if (node.skillData.exclusiveGroupId != selected.exclusiveGroupId)
                continue;

            node.skillData.isExclusiveActive = (node.skillData == selected);
        }

        RebuildAll();
    }

    public bool IsNodeActive(TreeNode node)
    {
        if (node == null || node.skillData == null)
            return false;

        var data = node.skillData;

        if (!string.IsNullOrEmpty(data.exclusiveGroupId))
            return data.isExclusiveActive;

        if (data.isPassive)
            return data.currentLevel > 0;

        return data.isUnlocked;
    }

    public void RebuildAll()
    {
        // Reset global stats and per-skill runtime values
        if (StatsManager.Instance != null)
            StatsManager.Instance.ResetModifiers();

        var uniqueSkills = new HashSet<SkillData>();
        foreach (var node in allNodes)
        {
            if (node != null && node.skillData != null)
                uniqueSkills.Add(node.skillData);
        }

        foreach (var skill in uniqueSkills)
            skill.ResetRuntimeStats();

        // Apply active node effects
        foreach (var node in allNodes)
        {
            if (!IsNodeActive(node))
                continue;

            var skill = node.skillData;
            if (skill == null)
                continue;

            foreach (var upgrade in skill.upgrades)
            {
                if (upgrade == null)
                    continue;

                if (upgrade.level <= skill.currentLevel)
                    ApplyEffects(upgrade.effects);
            }
        }

        // Refresh dependent systems
        if (GlobalDataStore.Instance != null && GlobalDataStore.Instance.BarrierModule != null)
            GlobalDataStore.Instance.BarrierModule.ResetHealthBarrier();

        if (SkillLoadout.Instance != null)
            SkillLoadout.Instance.RefreshSlotElementColors();

    }

    private void ApplyEffects(List<SkillEffect> effects)
    {
        if (effects == null || effects.Count == 0)
            return;

        // Apply in priority order
        effects.Sort((a, b) => a.priority.CompareTo(b.priority));

        foreach (var effect in effects)
        {
            if (effect != null)
                effect.Apply();
        }
    }


    private void RefreshAllNodes()
    {
        allNodes.Clear();
        allNodes.AddRange(FindObjectsByType<TreeNode>(
                FindObjectsSortMode.None
            )); // include inactive
    }



    public void ResetTree()
    {
         RefreshAllNodes();

        foreach (var node in allNodes)
        {
            if (node == null || node.skillData == null)
                continue;

            Debug.Log($"Resetting Skill: {node.skillData.name}");
            node.skillData.ResetRuntimeStats();
            node.skillData.ResetProgress();
        }

        InitializeTree();  // re-sync availability + UI
        RebuildAll();      // reapply effects from scratch
    }
}