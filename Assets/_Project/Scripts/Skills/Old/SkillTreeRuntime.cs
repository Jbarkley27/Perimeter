using System.Collections.Generic;
using UnityEngine;

/*
 * SkillTreeRuntime
 * ----------------
 * Runtime manager for skill tree nodes and effects.
 * Uses SkillTreeDefinition + SkillTreeState.
 */
public class SkillTreeRuntime : MonoBehaviour
{
    public static SkillTreeRuntime Instance { get; private set; }

    [Header("Definitions")]
    public SkillTreeDefinition treeDefinition;

    [Header("Runtime State")]
    public SkillTreeState state = new SkillTreeState();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeState();
        RebuildAll();
    }


    private void Start()
    {

    }

    public void InitializeState()
    {
        state.nodes.Clear();
        state.activeExclusiveByGroup.Clear();

        if (treeDefinition == null || treeDefinition.nodes == null)
            return;

        foreach (var node in treeDefinition.nodes)
        {
            if (node == null || string.IsNullOrWhiteSpace(node.nodeId))
                continue;

            state.nodes[node.nodeId] = new SkillNodeRuntimeState
            {
                level = 0,
                unlocked = false
            };
        }
    }

    public bool IsUnlocked(SkillNodeDefinition node)
    {
        if (node == null || !state.nodes.TryGetValue(node.nodeId, out var rt))
            return false;
        return rt.unlocked || rt.level > 0;
    }

    public int GetLevel(SkillNodeDefinition node)
    {
        if (node == null || !state.nodes.TryGetValue(node.nodeId, out var rt))
            return 0;
        return rt.level;
    }

    public bool IsExclusiveActive(SkillNodeDefinition node)
    {
        if (node == null || string.IsNullOrEmpty(node.exclusiveGroupId))
            return false;

        return state.activeExclusiveByGroup.TryGetValue(node.exclusiveGroupId, out string activeId)
               && activeId == node.nodeId;
    }

    public bool IsAtMaxLevel(SkillNodeDefinition node)
    {
        if (node == null || !state.nodes.TryGetValue(node.nodeId, out var rt))
            return false;
        return rt.level >= node.maxLevel;
    }

    public bool IsAvailable(SkillNodeDefinition node)
    {
        if (node == null)
        {
            Debug.LogWarning("[SkillTreeRuntime] IsAvailable called with null node.");
            return false;
        }

        if (node.prerequisites == null || node.prerequisites.Count == 0)
        {
            Debug.Log($"[SkillTreeRuntime] Node {node.displayName} has no prerequisites. Available by default.");
            return true;
        }

        for (int i = 0; i < node.prerequisites.Count; i++)
        {
            SkillNodePrereq prereq = node.prerequisites[i];
            if (prereq.node == null)
                continue;


            int reqLevel = Mathf.Max(1, prereq.requiredLevel);
            if (GetLevel(prereq.node) < reqLevel)
            {
                Debug.Log($"[SkillTreeRuntime] Node {node.displayName} is missing prerequisite: {prereq.node.displayName} level {reqLevel} (current level: {GetLevel(prereq.node)})");
                return false;
            }
        }

        Debug.Log($"[SkillTreeRuntime] Node {node.displayName} prerequisites met. Available.");

        return true;
    }

    public int GetCostForNextLevel(SkillNodeDefinition node)
    {
        if (node == null)
            return 0;

        int currentLevel = GetLevel(node);
        int nextLevel = Mathf.Clamp(currentLevel + 1, 1, node.maxLevel);

        int baseCost = node.baseCost;
        int perLevel = node.costPerLevel;

        float globalBase = treeDefinition != null ? treeDefinition.baseCostMultiplier : 1f;
        float globalPer = treeDefinition != null ? treeDefinition.perLevelCostMultiplier : 1f;

        int cost = Mathf.RoundToInt((baseCost * globalBase) + (perLevel * globalPer * (nextLevel - 1)));
        return Mathf.Max(1, cost);
    }


        // Returns the cost for a specific level (1-based).
    public int GetCostForLevel(SkillNodeDefinition node, int level)
    {
        if (node == null)
            return 0;

        int clampedLevel = Mathf.Clamp(level, 1, node.maxLevel);

        int baseCost = node.baseCost;
        int perLevel = node.costPerLevel;

        float globalBase = treeDefinition != null ? treeDefinition.baseCostMultiplier : 1f;
        float globalPer = treeDefinition != null ? treeDefinition.perLevelCostMultiplier : 1f;

        int cost = Mathf.RoundToInt((baseCost * globalBase) + (perLevel * globalPer * (clampedLevel - 1)));
        return Mathf.Max(1, cost);
    }

    // Returns total spent on a node so far.
    public double GetSpentOnNode(SkillNodeDefinition node)
    {
        if (node == null || !state.nodes.TryGetValue(node.nodeId, out var rt))
            return 0;

        return rt.spentGlass;
    }

    // Attempts to purchase/upgrade a node (handles Passive/Active/Exclusive).
    public bool TryPurchaseOrUpgrade(SkillNodeDefinition node)
    {
        Debug.Log($"[SkillTreeRuntime] TryPurchase {node.displayName} hasState={state.nodes.ContainsKey(node.nodeId)} glass={(GlassManager.Instance != null ? GlassManager.Instance.GetTotalGlassShardsCollected() : -1)}");

        if (node == null)
            return false;

        if (!IsAvailable(node))
            return false;

        if (node.IsExclusive)
            return TryActivateExclusive(node);

        if (node.IsActive)
            return TryUnlockActive(node);

        return TryUpgradePassive(node);
    }

    private bool TryUnlockActive(SkillNodeDefinition node)
    {
        if (!state.nodes.TryGetValue(node.nodeId, out var rt))
        {
            Debug.LogError($"[SkillTreeRuntime] No runtime state found for node {node.displayName} (ID: {node.nodeId}). Cannot unlock.");
            return false;
        }

        if (rt.level >= 1)
        {
            Debug.Log($"[SkillTreeRuntime] Node {node.displayName} is already unlocked.");
            return false;
        }

        int cost = GetCostForLevel(node, 1);
        if (!GlassManager.Instance.SpendGlass(cost))
        {
            Debug.Log($"[SkillTreeRuntime] Not enough glass to unlock {node.displayName}. Cost={cost}, Available={(GlassManager.Instance != null ? GlassManager.Instance.GetTotalGlassShardsCollected() : -1)}");
            return false;
        }

        Debug.Log($"[SkillTreeRuntime] Unlock {node.displayName} cost={cost} glass={GlassManager.Instance?.GetTotalGlassShardsCollected()} baseMult={treeDefinition?.baseCostMultiplier} perMult={treeDefinition?.perLevelCostMultiplier}");
        bool spent = GlassManager.Instance.SpendGlass(cost);
        Debug.Log($"[SkillTreeRuntime] Spend result={spent}");


        rt.level = 1;
        rt.unlocked = true;
        rt.spentGlass += cost;
        state.nodes[node.nodeId] = rt;

        RebuildAll();
        return true;
    }

    private bool TryUpgradePassive(SkillNodeDefinition node)
    {
        if (!state.nodes.TryGetValue(node.nodeId, out var rt))
            return false;

        if (rt.level >= node.maxLevel)
            return false;

        int nextLevel = Mathf.Clamp(rt.level + 1, 1, node.maxLevel);
        int cost = GetCostForLevel(node, nextLevel);

        if (!GlassManager.Instance.SpendGlass(cost))
            return false;

        rt.level = nextLevel;
        rt.unlocked = true;
        rt.spentGlass += cost;
        state.nodes[node.nodeId] = rt;

        RebuildAll();
        return true;
    }

    private bool TryActivateExclusive(SkillNodeDefinition node)
    {
        if (!state.nodes.TryGetValue(node.nodeId, out var rt))
            return false;

        // First time purchase costs glass
        if (rt.level <= 0)
        {
            int cost = GetCostForLevel(node, 1);
            if (!GlassManager.Instance.SpendGlass(cost))
                return false;

            rt.level = 1;
            rt.unlocked = true;
            rt.spentGlass += cost;
            state.nodes[node.nodeId] = rt;
        }

        // Switching is free
        state.activeExclusiveByGroup[node.exclusiveGroupId] = node.nodeId;

        RebuildAll();
        return true;
    }

    // Full refund for a node (resets level/unlocked + returns spent glass).
    public double RefundNode(SkillNodeDefinition node)
    {
        if (node == null || !state.nodes.TryGetValue(node.nodeId, out var rt))
            return 0;

        double refund = rt.spentGlass;

        rt.level = 0;
        rt.unlocked = false;
        rt.spentGlass = 0;
        state.nodes[node.nodeId] = rt;

        if (node.IsExclusive && state.activeExclusiveByGroup.TryGetValue(node.exclusiveGroupId, out string activeId))
        {
            if (activeId == node.nodeId)
                state.activeExclusiveByGroup.Remove(node.exclusiveGroupId);
        }

        if (refund > 0 && GlassManager.Instance != null)
            GlassManager.Instance.AddGlass(refund);

        return refund;
    }

    // Refunds any nodes that become invalid after a change.
    public void RefundInvalidNodes()
    {
        if (treeDefinition == null)
            return;

        bool changed = true;

        while (changed)
        {
            changed = false;

            foreach (var node in treeDefinition.nodes)
            {
                if (node == null)
                    continue;

                if (!state.nodes.TryGetValue(node.nodeId, out var rt))
                    continue;

                if (rt.level <= 0)
                    continue;

                if (!IsAvailable(node))
                {
                    RefundNode(node);
                    changed = true;
                }
            }
        }

        RebuildAll();
    }



        // Returns true if a node is considered "active" for visuals/effects.
    // Exclusive nodes are active only if they are the chosen option.
    public bool IsNodeActive(SkillNodeDefinition node)
    {
        if (node == null)
            return false;

        if (node.IsExclusive)
            return IsExclusiveActive(node);

        return GetLevel(node) > 0;
    }



    public void RebuildAll()
    {
        // Reset global stats
        if (StatsManager.Instance != null)
            StatsManager.Instance.ResetSkillModifiers();

        // Reset skill runtime values
        if (treeDefinition != null)
        {
            foreach (var node in treeDefinition.nodes)
            {
                if (node == null || node.skill == null)
                    continue;

                node.skill.ResetRuntimeStats();
            }
        }

        // Apply effects for each active node
        if (treeDefinition == null)
            return;

        foreach (var node in treeDefinition.nodes)
        {
            if (node == null)
                continue;

            if (node.IsExclusive && !IsExclusiveActive(node))
                continue;

            int level = GetLevel(node);
            if (level <= 0)
                continue;

            ApplyNodeEffects(node, level);
        }

        if (SkillLoadout.Instance != null)
            SkillLoadout.Instance.RefreshSlotElementColors();
    }

    private void ApplyNodeEffects(SkillNodeDefinition node, int level)
    {
        if (node.levelEffects == null || node.levelEffects.Count == 0)
            return;

        for (int i = 0; i < node.levelEffects.Count; i++)
        {
            if (node.levelEffects[i].level > level)
                continue;

            var effects = node.levelEffects[i].effects;
            if (effects == null) continue;

            effects.Sort((a, b) => a.priority.CompareTo(b.priority));

            foreach (var effect in effects)
                if (effect != null) effect.Apply();
        }
    }
}
