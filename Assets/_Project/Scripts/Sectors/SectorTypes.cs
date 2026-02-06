using System;
using UnityEngine;

/*
 * SectorTypes
 * -----------
 * Shared enums and data structs for the sector/compass system.
 * Keep these lightweight and UI-friendly.
 */
public enum SectorDirection
{
    North,
    East,
    South,
    West
}

public enum SectorModifierRarity
{
    Common,
    Uncommon,
    Rare
}

/*
 * SectorRewardEntry
 * -----------------
 * Descriptive reward entries shown in UI.
 * Actual reward payout is handled by the run/sector system later.
 */
[Serializable]
public struct SectorRewardEntry
{
    public string rewardName;
    [TextArea] public string rewardDescription;

    public SectorRewardType rewardType;
    public float rewardValue; // interpretation depends on rewardType
}


public enum SectorRewardType
{
    GlassFlat,
    GlassPercentCurrent,
    GlassPercentTotal,
    AugmentChance,
    Placeholder
}


/*
 * SectorModifierEffectType
 * ------------------------
 * Generic effect types that sector modifiers can apply.
 * Not all are wired yet; these are the hooks for future systems.
 */
public enum SectorModifierEffectType
{
    None = 0,
    AllDamageMultiplier,
    EnemyDamageMultiplier,
    EnemyHealthMultiplier,
    PlayerDamageMultiplier,
    PlayerHealthMultiplier,
    PlayerBarrierMultiplier,
    PlayerBarrierRegenMultiplier,
    PlayerCooldownMultiplier,
    ProbeEffectivenessMultiplier,
    GlassEarnedMultiplier,
    GlassOnClearPercentOfCurrent,
    GlassOnClearPercentOfTotal,
    GlassOnWaveClearFlat,
    GlassOnStatusAppliedFlat,
    StatusApplyChanceMultiplier,
    StatusEffectStrengthMultiplier,
    ElementDamageMultiplier,
    ElementGlassMultiplier,
    EnemyAttackSpeedMultiplier,
    PlayerCritChanceMultiplier,
    EnemyCritImmune,
    AugmentChanceBonus
}

/*
 * SectorModifierEffect
 * --------------------
 * A single effect entry; value interpretation depends on effectType.
 * For multipliers: 1.0 = no change, 1.2 = +20%, 0.8 = -20%.
 */
[Serializable]
public struct SectorModifierEffect
{
    public SectorModifierEffectType effectType;
    public float value;
    public Element element; // used for element-specific effects

    // Debug helper for logs / inspector sanity.
    public string GetDebugLabel()
    {
        return effectType.ToString();
    }
}


/*
 * SectorEnemySpawnBonus
 * ---------------------
 * Adds extra enemy spawns on top of base wave spawns.
 * Rolled per base spawn (chance, extraCount).
 */
[Serializable]
public struct SectorEnemySpawnBonus
{
    public EnemyDataStore.EnemyType enemyType;

    [Range(0f, 1f)]
    public float chance;

    public int extraCount;

    [Tooltip("Max extra spawns per wave for this bonus. 0 or less = no cap.")]
    public int maxExtraPerWave;
}

