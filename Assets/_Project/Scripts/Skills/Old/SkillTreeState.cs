using System.Collections.Generic;

/*
 * SkillTreeState
 * --------------
 * Runtime-only state for skill tree progress.
 * Keeps asset data (definitions) clean.
 */
public class SkillTreeState
{
    public Dictionary<string, SkillNodeRuntimeState> nodes = new Dictionary<string, SkillNodeRuntimeState>();
    public Dictionary<string, string> activeExclusiveByGroup = new Dictionary<string, string>();
}

public struct SkillNodeRuntimeState
{
    public int level;
    public bool unlocked;
    public double spentGlass;
}

