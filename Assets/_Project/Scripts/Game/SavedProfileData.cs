using System;
using System.Collections.Generic;

/*
 * Save Schema Types
 * -----------------
 * This file intentionally contains only serializable data containers.
 * Save/load logic should live in a separate manager/service class.
 */

[Serializable]
public class SavedSkillProgress
{
    // Stable ID from SkillData (recommended) instead of display name.
    public string skillId;

    // Skill progression state.
    public int currentLevel;
    public bool isUnlocked;
}

[Serializable]
public class SavedProfileData
{
    // Bump this when schema changes; use migrations to upgrade old saves.
    public int saveVersion = 1;

    // Core profile progression.
    public int runAttempts = 0;
    public double totalGlass = 0;
    public double totalCores = 0;

    // Skill persistence.
    public List<SavedSkillProgress> savedSkills = new List<SavedSkillProgress>();
    public List<string> equippedSkillIds = new List<string>();

    // Add future systems here, for example:
    // public List<SavedAugmentData> savedAugments = new List<SavedAugmentData>();
    // public List<string> equippedAugmentIds = new List<string>();
}
